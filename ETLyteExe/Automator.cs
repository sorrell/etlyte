﻿using ETLyteDLL;
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
                if ((configFile != null && configFile.Db.StopOnError == true && value != 0)
                    || value == 666)  // FileErrorLimit
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
        public static int Run(ConfigFile c, string cmd = "", string filename = "")
        {
            configFile = c;

            if (!String.IsNullOrWhiteSpace(cmd))
            {
                try
                {
                    Validate = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out, Console.Out, configFile.Validate.Outputs.StandardOutputDelimiter);
                    string sql = cmd;
                    db = new SqliteDb(configFile.Db.DbName, true, null, configFile.Extract.Delimiter, configFile.Db.LogFile).Init();
                    SqliteStatus = db.ExecuteQuery(sql, Validate, "Command line query");
                }

                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong trying to read the input command. Try again... " + e.Message);
                }
            }

            else if (!String.IsNullOrWhiteSpace(filename))
            {
                try
                {
                    Validate = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out, Console.Out, configFile.Validate.Outputs.StandardOutputDelimiter);
                    string sql = GetSqlContents(filename);
                    db = new SqliteDb(configFile.Db.DbName, true, null, configFile.Extract.Delimiter, configFile.Db.LogFile).Init();
                    SqliteStatus = db.ExecuteQuery(sql, Validate, "Command line file execution");
                }

                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong trying to read the input file. Try again... " + e.Message);
                }
            }

            else
            {
                try
                {
                    Validate = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out, Console.Out);
                    Transform = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out, Console.Out);
                    Load = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out, Console.Out);
                    Validations validator = null;
                    SqliteStatus = 0;
                    string sql;
                    db = new SqliteDb(configFile.Db.DbName, configFile.Db.UseExistingDb, null, configFile.Extract.Delimiter, configFile.Db.LogFile).Init();

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

                        Validate.BeginContext(schemaFile.Name, Globals.ResultWriterDestination.stdOut);
                        if (configFile.Validate.Outputs.Warnings && (configFile.Validate.Outputs.StandardOutputConnectionString != configFile.Validate.Outputs.WarningsOutputConnectionString))
                        {
                            if (Validate.ResultMode == "delimited")
                                Validate.Write(schemaFile.Name, Globals.ResultWriterDestination.Warning);
                            else if (Validate.ResultMode == "json")
                                Validate.BeginContext(schemaFile.Name, Globals.ResultWriterDestination.Warning);
                        }


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
                                Validate.EndContext(Globals.ResultWriterDestination.stdOut);
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
                            validator = new Validations(configFile.Validate.SchemaErrorSettings, db, Validate, (code => SqliteStatus = code), configFile.Validate, schemaFile.Name);
                            currentStep = SetCurrentStep("Validating file", Validate);

                            foreach (var schemaField in schemaFile.Fields)
                            {
                                validator.ValidateFields(schemaField, schemaFile.Name, Validate);
                            }

                            if (schemaFile.SummarizeResults)
                            {
                                validator.PrintSummaryResults(schemaFile.Name, Globals.ResultWriterDestination.stdOut);
                                if (configFile.Validate.Outputs.Warnings)
                                    validator.PrintSummaryResults(schemaFile.Name, Globals.ResultWriterDestination.Warning);
                            }

                            validator.PrintDetailResults(schemaFile.Name, Globals.ResultWriterDestination.stdOut);
                            if (configFile.Validate.Outputs.Warnings)
                                validator.PrintDetailResults(schemaFile.Name, Globals.ResultWriterDestination.Warning);
                        }

                        Validate.EndContext(Globals.ResultWriterDestination.stdOut);
                        if (Validate.ResultMode == "json" && configFile.Validate.Outputs.Warnings
                            && (configFile.Validate.Outputs.StandardOutputConnectionString != configFile.Validate.Outputs.WarningsOutputConnectionString))
                            Validate.EndContext(Globals.ResultWriterDestination.Warning);
                    } // end for each flat file

                    //
                    // Custom validation checks
                    // Possibly cross file / multi table joins
                    //
                    if (configFile.Steps.Validate && !string.IsNullOrWhiteSpace(configFile.Validate.ValidationSource))
                    {
                        string ctx = "Custom Data Validation Checks";
                        // Perhaps we have no flatfiles but do have a db and custom validations - in this case validator would be null
                        if (validator == null)
                            validator = new Validations(configFile.Validate.SchemaErrorSettings, db, Validate, (code => SqliteStatus = code), configFile.Validate, "GeneralErrors");
                        Validate.BeginContext(ctx, Globals.ResultWriterDestination.stdOut);
                        if (configFile.Validate.Outputs.Warnings && (configFile.Validate.Outputs.StandardOutputConnectionString != configFile.Validate.Outputs.WarningsOutputConnectionString))
                        {
                            if (Validate.ResultMode == "delimited")
                                Validate.Write(ctx, Globals.ResultWriterDestination.Warning);
                            else if (Validate.ResultMode == "json")
                                Validate.BeginContext(ctx, Globals.ResultWriterDestination.Warning);
                        }

                        foreach (var validationFile in validationDirInfo.GetFiles("*.sql"))
                        {
                            currentStep = SetCurrentStep("Getting contents from: " + validationFile.Name, Validate);
                            validator.ValidateCustom(validationFile, configFile.Validate.QueryErrorLimit, configFile.Validate.Outputs.Warnings);
                        }
                        Validate.EndContext(Globals.ResultWriterDestination.stdOut);
                        if (Validate.ResultMode == "json" && configFile.Validate.Outputs.Warnings
                            && (configFile.Validate.Outputs.StandardOutputConnectionString != configFile.Validate.Outputs.WarningsOutputConnectionString))
                            Validate.EndContext(Globals.ResultWriterDestination.Warning);

                    }
                    if (configFile.Steps.Validate)
                    {
                        validator.PrintGeneralIssues(Globals.ResultWriterDestination.stdOut);
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
                            sql = GetSqlContents(loadFile.FullName);
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
                    db.Close();
                    Validate.Dispose();
                }
            }
            return SqliteStatus;
        }

        public static string GetSqlContents(string filename)
        {
            string sql = "";
            List<string> contents = File.ReadAllText(filename).Split('\n').ToList();
            foreach (var line in contents)
            {
                if ((line.Length > 2) && Regex.IsMatch(line, @"\s*-{2}.*"))
                {
                    // ignore the comment
                }
                else
                    sql += " " + line;
            }
            return sql;
        }
    }
}
