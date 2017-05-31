using ETLyteDLL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ETLyteExe
{
    public static class Automator
    {
        public static IResultWriter Validate;
        public static IResultWriter Transform;
        public static IResultWriter Load;
        public static string currentStep = String.Empty;
        public static ConfigFile configFile { get; set; }

        static SqliteDb db;
        static int sqlitecode = 0;
        public static int SqliteStatus
        {
            get { return sqlitecode; }
            set
            {
                sqlitecode = value;
                if (configFile != null && configFile.Db.StopOnError == true && value != 0)
                    throw new Exception("SQLite status <> 0");
            }
        }

        static List<string> GetFullFilePath(DirectoryInfo dir, string filename, IResultWriter writer)
        {
            Regex reg = new Regex(filename);
            List<string> fileinfo = new List<string>();
            //string file = String.Empty;
            try
            {
                fileinfo = Directory.GetFiles(dir.FullName, @"*.*")
                          .Where(path => reg.IsMatch(path))
                          .ToList();
            }
            catch (Exception e)
            {
                writer.WriteVerbose("Couldn't locate file " + filename);
            }
            return fileinfo;
        }

        static string SetCurrentStep(string desc, IResultWriter writer)
        {
            writer.WriteVerbose("[" + DateTime.Now + "] " + desc);
            return desc;
        }
        public static int Run(ConfigFile c)
        {
            configFile = c;
            try
            {
                Validate = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out, Console.Out);
                Transform = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out, Console.Out);
                Load = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out, Console.Out);
                Validations validator = null;
                SqliteStatus = 0;
                string sql;
                db = new SqliteDb(configFile.Db.DbName, configFile.Db.UseExistingDb, null, configFile.Extract.Delimiter, configFile.Db.LogFile);

                // NPS_TODO add error outputting for when these fail to load
                if (configFile.Steps.Validate)
                    Validate = ConfigFileInit.InitValidateFromConfig(configFile);
                if (configFile.Steps.Transform)
                    Transform = ConfigFileInit.InitTransformFromConfig(configFile);
                if (configFile.Steps.Load)
                    Load = ConfigFileInit.InitLoadFromConfig(configFile, db);
                
                DirectoryInfo schemaDirInfo = new DirectoryInfo(configFile.Extract.Schemas);
                DirectoryInfo sourceFilesDirInfo = new DirectoryInfo(configFile.Extract.Source);
                DirectoryInfo validationDirInfo = new DirectoryInfo(configFile.Validate.ValidationSource);
                DirectoryInfo loadDirInfo = new DirectoryInfo(configFile.Load.LoadSource);
                DirectoryInfo seedDirInfo = new DirectoryInfo(configFile.Extract.SeedData);
                SchemaFile schemaFile = null;
                SqliteModeler modeler = null;

            
                Validate.BeginOutput("");

                if (configFile.Steps.Extract)
                {
                    //NPS_TODO: Add check to see if we need to do this on reuse db
                    sql = new SqliteModeler().CreateGeneralErrorWarningDdl();
                    SqliteStatus = db.ModifyDdlFromSqlString(sql);
                    Validate.WriteVerbose(SqliteStatus.ToString() + ":" + sql + "||" + db.LastError);

                    // load seed data
                    if (seedDirInfo.Exists)
                    {
                        foreach (var seedFile in seedDirInfo.GetFiles("*.sql"))
                        {
                            //NPS_TODO: Add check to see if we need to do this on reuse db
                            currentStep = SetCurrentStep("Reading seed data from " + seedFile.Name, Validate);
                            SqliteStatus = db.ExecuteQuery(File.ReadAllText(seedFile.FullName), Validate);
                        }
                    }

                }
                foreach (var file in schemaDirInfo.GetFiles("*.json"))
                {
                    //NPS_TODO: See if Scheme has already been created
                    // create schemafile object
                    currentStep = SetCurrentStep("Reading schema file: " + file.Name, Validate);
                    schemaFile = JsonConvert.DeserializeObject<SchemaFile>(File.ReadAllText(file.FullName));

                    Validate.BeginContext(schemaFile.Name);

                    // create SQLiteModeler
                    currentStep = SetCurrentStep("Setting schema file: " + file.Name, Validate);
                    modeler = new SqliteModeler().SetSchemaFile(schemaFile);

                    // create SQL from schemafile
                    currentStep = SetCurrentStep("Creating DDL...", Validate);
                    sql = modeler.CreateDdl(true).Ddl;

                    // execute DDL in schemafile
                    currentStep = SetCurrentStep("Modifying SQLite DB with DDL...", Validate);
                    SqliteStatus = db.ModifyDdlFromSqlString(sql);
                    Validate.WriteVerbose(SqliteStatus.ToString() + ":" + sql);

                    // create SQL from schemafile
                    currentStep = SetCurrentStep("Creating DDL...", Validate);
                    sql = modeler.CreateErrorDdl().ErrorDdl;

                    // execute DDL in schemafile
                    currentStep = SetCurrentStep("Modifying SQLite DB with Error DDL...", Validate);
                    SqliteStatus = db.ModifyDdlFromSqlString(sql);
                    Validate.WriteVerbose(SqliteStatus.ToString() + ":" + sql);

                    // find linked flat file
                    var files = GetFullFilePath(sourceFilesDirInfo, schemaFile.Flatfile, Validate);
                    Flatfile flatfile = null;

                    if (configFile.Steps.Extract)
                    {

                        foreach (var f in files)
                        {
                            var reuseDbSql = "SELECT * FROM FileAudit WHERE FileName = '" + f + "';";
                            bool shouldReadFile = true;
                            if (configFile.Db.UseExistingDb && db.QueryHasResults(reuseDbSql))
                                shouldReadFile = false;

                            if (f != String.Empty && shouldReadFile)
                            {

                                //import flat file
                                currentStep = SetCurrentStep("Importing flatfile " + f + "...", Validate);

                                //NPS_TODO: setup File
                                flatfile = new Flatfile(f, schemaFile.Name, schemaFile.Delimiter.ToString(), "\"", true, null, schemaFile);
                                int linesRead = 0;
                                SqliteStatus = db.ImportDelimitedFile(flatfile, out linesRead, configFile, true);
                                // NPS_TODO: Make linenum optional in configfile

                                currentStep = SetCurrentStep("Finished reading flatfile " + f + "...", Validate);
                                var auditSql = "INSERT INTO FileAudit VALUES ('" + f + "', CURRENT_TIMESTAMP, " + schemaFile.Fields.Count + ", " + linesRead + ");";
                                SqliteStatus = db.ExecuteQuery(auditSql, Validate);
                            }
                        }

                        if (files.Count == 0)
                        {
                            SqliteStatus = db.ExecuteQuery("INSERT INTO GeneralErrors VALUES ('File Missing', 'None', '" + schemaFile.Name + "', 'Error', 'Failed to find file matching " + schemaFile.Flatfile + "');", Validate);
                            Validate.EndContext();
                            continue; // no files, continue the loop so no validation happens
                        }
                        else
                        {
                            var metadataSql = "";
                            // NPS_TODO: Handle Derivations flag
                            // DERIVATIONS
                            foreach (var schemaField in flatfile.Schemafile.Fields.Where(x => x.ColumnType == ColumnType.Derived).Select(x => x))
                            {
                                var derivationSql = "UPDATE " + flatfile.Tablename + " SET " + schemaField.Name + " = " + schemaField.Derivation + ";";
                                SqliteStatus = db.ExecuteQuery(derivationSql, Validate);
                            }
                            foreach (var schemaField in schemaFile.Fields)
                            {
                                metadataSql = "INSERT INTO TableMetadata VALUES ('" + schemaFile.Name + "', '" + schemaField.Name + "', '" + schemaField.DataType + "');";
                                // finding numeric precision/scale for sql server
                                // with cte as (select length(b)-1 as precision, length(b) - instr(b, '.') as scale from foo) select case when
                                // max(precision) - min(scale) >= 38 then 38 else max(precision) end as precision, max(scale) from cte; 
                                SqliteStatus = db.ExecuteQuery(metadataSql, Validate);
                            }
                            metadataSql = "INSERT INTO TableMetadata VALUES ('" + schemaFile.Name + "', 'LineNum', 'int');";
                            SqliteStatus = db.ExecuteQuery(metadataSql, Validate);
                        }
                    }

                #region Validate

                    // file level validations
                    if (configFile.Steps.Validate)
                    {
                        validator = new Validations(configFile.Validate.SchemaErrorSettings, db, Validate, (code => SqliteStatus = code));
                        currentStep = SetCurrentStep("Validating file", Validate);

                        foreach (var schemaField in schemaFile.Fields)
                        {
                            validator.ValidateFields(schemaField, schemaFile.Name, Validate);
                        }

                        if (schemaFile.SummarizeResults)
                            validator.PrintSummaryResults(schemaFile.Name);
                        validator.PrintDetailResults(schemaFile.Name, "Error");
                    }

                    Validate.EndContext();
                } // end for each flat file

                //
                // Custom validation checks
                // Possibly cross file / multi table joins
                //
                if (configFile.Steps.Validate && !string.IsNullOrWhiteSpace(configFile.Validate.ValidationSource))
                {
                    Validate.BeginContext("Custom Data Validation Checks");
                    
                    foreach (var validationFile in validationDirInfo.GetFiles("*.sql"))
                    {
                        currentStep = SetCurrentStep("Getting contents from: " + validationFile.Name, Validate);
                        validator.ValidateCustom(validationFile, configFile.Validate.ErrorLimit);
                    }
                    Validate.EndContext();

                }
                if (configFile.Steps.Validate)
                {
                    validator.PrintGeneralIssues(Globals.ResultWriterDestination.Error);
                    if (configFile.Validate.Outputs.Warnings)
                        validator.PrintGeneralIssues(Globals.ResultWriterDestination.Warning);
                }

                Validate.EndOutput("");
            #endregion Validate

                Load.BeginOutput("");
                if (configFile.Steps.Load)
                {
                    foreach (var loadFile in loadDirInfo.GetFiles("*.sql"))
                    {
                        currentStep = SetCurrentStep("Getting contents from: " + loadFile.Name, Validate);
                        string context = String.Empty;
                        sql = String.Empty;

                        List<string> contents = File.ReadAllText(loadFile.FullName).Split('\n').ToList();
                        foreach (var line in contents)
                        {
                            if ((line.Length > 2) && Regex.IsMatch(line, @"\s*-{2}.*")) // (line.Substring(0, 2) == "--"))
                            {
                                //if (Regex.IsMatch(line, @"\s*-{2}\s*Context:.*"))
                                //    context += line;
                            }
                            else
                                sql += " " + line;
                        }

                        context = loadFile.Name;

                        SqliteStatus = db.ExecuteQuery(sql, Load, context);
                    }
                }
            }
            catch (Exception e)
            {
                Validate.WriteStd("ERROR on step: " + currentStep);
                Validate.WriteError("[ERROR] " + DateTime.Now);
                Validate.WriteError("[ERROR MSG] " + e.Message);
                if (db != null && !string.IsNullOrWhiteSpace(db.LastError))
                    Validate.WriteError("[DB MSG] " + db.LastError);

                Validate.WriteError(e.StackTrace);
                return SqliteStatus;
            }
            finally
            {
                Validate.Dispose();
            }

            return SqliteStatus;
        }

    }
}
