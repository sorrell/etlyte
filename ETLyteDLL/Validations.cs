﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class Validations
    {
        private Func<int, int> SqliteStatusCallback;
        private int sqlitecode;
        private int SqliteStatus
        {
            get
            { return sqlitecode; }
            set
            {
                sqlitecode = value;
                SqliteStatusCallback(value);
            }
        }

        private Int64 errorCount = 0;
        private void ErrorCount(Int64 val)
        {
            errorCount += val;
            if (errorCount >= vOptions.FileErrorLimit)
            {
                PrintSummaryResults(currentTable, Globals.ResultWriterDestination.stdOut);
                PrintDetailResults(currentTable, Globals.ResultWriterDestination.stdOut);
                Validate.Write(Environment.NewLine + "---------------" + Environment.NewLine + "Error limit reached: " + errorCount + " errors detected " + vOptions.FileErrorLimit, Globals.ResultWriterDestination.stdOut);
                SqliteStatusCallback(666);
            }
        }

        private string currentTable;
        private SchemaErrorSettings esettings;
        private SqliteDb db;
        private IResultWriter Validate;
        private ValidateOptions vOptions;

        public Validations(SchemaErrorSettings inesettings, SqliteDb indb, IResultWriter inValidate, Func<int, int> cb, ValidateOptions validateOptions, string curTable)
        {
            SqliteStatusCallback = cb;
            esettings = inesettings;
            db = indb;
            Validate = inValidate;
            vOptions = validateOptions;
            currentTable = curTable;
        }

        public string GetSqlForDatatype(SchemaField schemaField, string sqlbase, string errorLevel)
        {
            string sql = "";
            switch (schemaField.DataType)
            {
                case DataType.Boolean:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISBOOL(" + schemaField.Name + ")");
                    break;
                case DataType.@byte:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISBYTE(" + schemaField.Name + ")");
                    break;
                case DataType.@char:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISCHAR(" + schemaField.Name + ")");
                    break;
                case DataType.DateTime:                                
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISDATETIME(" + schemaField.Name + ", '" + schemaField.Constraints.DatePattern + "') ");
                    break;
                case DataType.@decimal:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISDECIMAL(" + schemaField.Name + ")");
                    break;
                case DataType.@double:                                 
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISDOUBLE(" + schemaField.Name + ")");
                    break;                                             
                case DataType.@float:                                  
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISFLOAT(" + schemaField.Name + ")");
                    break;                                             
                case DataType.@int:                                    
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISINT(" + schemaField.Name + ")");
                    break;
                case DataType.@long:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISLONG(" + schemaField.Name + ")");
                    break;
                case DataType.@sbyte:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISSBYTE(" + schemaField.Name + ")");
                    break;
                case DataType.@short:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISSHORT(" + schemaField.Name + ")");
                    break;
                case DataType.TimeSpan:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISTIMESPAN(" + schemaField.Name + ")");
                    break;
                case DataType.@uint:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISUINT(" + schemaField.Name + ")");
                    break;
                case DataType.@ulong:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISULONG(" + schemaField.Name + ")");
                    break;
                case DataType.URI:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISURI(" + schemaField.Name + ")");
                    break;
                case DataType.@ushort:
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISUSHORT(" + schemaField.Name + ")");
                    break;
                default:
                    // object or string
                    break;
                    //NPS_TODO all other datatypes
            }
            return sql;
        }
       
        public void ValidateFields(SchemaField schemaField, string tableName, IResultWriter writer)
        {
            var sqlbase = @"INSERT INTO " + tableName + "_Errors" +
                          @" SELECT {0}, '" + schemaField.Name + @"', {1},*
                             FROM " + tableName + @" 
                             {2} LIMIT " + vOptions.QueryErrorLimit + ";";
            string sql = "";
            
            if (schemaField.Constraints.Required)
            {
                sql = String.Format(sqlbase, "'Required Field'", esettings.RequiredErrorLevel.SingleQuote(), " WHERE " + schemaField.Name + " = '' OR " + schemaField.Name + " IS NULL");
                writer.WriteVerbose("[" + DateTime.Now + "] " + "Start Required Field check for " + schemaField.Name);
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "End Required Field check for " + schemaField.Name);
            }

            if (schemaField.DataType != DataType.@string
                && (schemaField.Constraints.Required || (esettings.ErrorOnUnrequiredWithBadDatatype && !schemaField.Constraints.Required)))
            {
                sql = GetSqlForDatatype(schemaField, sqlbase, esettings.DatatypeErrorLevel.SingleQuote());
                writer.WriteVerbose("[" + DateTime.Now + "] " + "Start Datatype check for " + schemaField.Name);
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "End Datatype check for " + schemaField.Name);
            }

            if (!double.IsNaN(schemaField.Constraints.Maximum))
            {
                sql = String.Format(sqlbase, "'Maximum Value'", esettings.MaximumErrorLevel.SingleQuote(), "WHERE COMPARE(" + schemaField.Name + @", '>', " + schemaField.Constraints.Maximum + @", '" + schemaField.DataType.ToString() + "')");
                writer.WriteVerbose("[" + DateTime.Now + "] " + "Start Maximum Value check for " + schemaField.Name);
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "End Maximum Value check for " + schemaField.Name);
            }
            if (!double.IsNaN(schemaField.Constraints.Minimum))
            {
                sql = String.Format(sqlbase, "'Minimum Value'", esettings.MinimumErrorLevel.SingleQuote(), "WHERE COMPARE(" + schemaField.Name + @", '<', " + schemaField.Constraints.Minimum + @", '" + schemaField.DataType.ToString() + "')");
                writer.WriteVerbose("[" + DateTime.Now + "] " + "Start Minimum Value check for " + schemaField.Name);
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "End Minimum Value check for " + schemaField.Name);
            }
            if (schemaField.Constraints.MinLength > -1)
            {
                sql = String.Format(sqlbase, "'Minimum Length'", esettings.MinLengthErrorLevel.SingleQuote(), "WHERE LENGTH(" + schemaField.Name + @") < " + schemaField.Constraints.MinLength);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "Start Minimum Length check for " + schemaField.Name);
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "End Minimum Length check for " + schemaField.Name);
            }
            if (schemaField.Constraints.MaxLength > -1)
            {
                sql = String.Format(sqlbase, "'Maximum Length'", esettings.MaxLengthErrorLevel.SingleQuote(), "WHERE LENGTH(" + schemaField.Name + @") > " + schemaField.Constraints.MaxLength);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "Start Maximum Length check for " + schemaField.Name);
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "End Maximum Length check for " + schemaField.Name);
            }
            if ((schemaField.Constraints.Pattern != null) && (schemaField.Constraints.Pattern != String.Empty))
            {
                sql = String.Format(sqlbase, "'Invalid Value(pattern)'", esettings.PatternErrorLevel.SingleQuote(), "WHERE '" + schemaField.Constraints.Pattern + @"' NOT REGEXP " + schemaField.Name);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "Start Pattern check for " + schemaField.Name);
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "End Pattern check for " + schemaField.Name);
            }
            if (schemaField.Constraints.Enum != null && schemaField.Constraints.Enum.Length > 0)
            {
                var values = "(" + String.Join(",", schemaField.Constraints.Enum.ToList().Select(x => "'" + x + "'")) + ")";
                sql = String.Format(sqlbase, "'Enum Valid Values'", esettings.MaxLengthErrorLevel.SingleQuote(), "WHERE " + schemaField.Name + @" NOT IN " + values);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "Start Enum Valid Values check for " + schemaField.Name);
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "End Enum Valid Values check for " + schemaField.Name);
            }
            if (schemaField.Constraints.Unique)
            {
                // Unique speed hack
                sql = @"INSERT INTO " + tableName + "_Errors" +
                          @" SELECT 'Unique', '" + schemaField.Name + @"'," + esettings.UniqueErrorLevel.SingleQuote() + @" ,*
                             FROM " + tableName + @" 
                             WHERE rowid IN (SELECT rowid FROM " + tableName + @" GROUP BY " + schemaField.Name + "HAVING COUNT(*) > 1;";
                writer.WriteVerbose("[" + DateTime.Now + "] " + "Start Unique check for " + schemaField.Name);
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount);
                writer.WriteVerbose("[" + DateTime.Now + "] " + "End Unique check for " + schemaField.Name);
            }
        }

        public void PrintGeneralIssues(Globals.ResultWriterDestination dest)
        {
            string errorLevel = (dest == Globals.ResultWriterDestination.stdOut) ? "Error" : "Warning";
            SqliteStatus = db.ExecuteQuery("SELECT * FROM GeneralErrors WHERE ErrorLevel = '" + errorLevel + "' ORDER BY ErrorType, ErrorTable, ErrorColumn;", Validate, "General Errors (" + errorLevel + ")");
        }
        public void PrintSummaryResults(string tableName, Globals.ResultWriterDestination dest)
        {
            string errorLevel = (dest == Globals.ResultWriterDestination.stdOut) ? "Error" : "Warning";
            SqliteStatus = db.ExecuteQuery("SELECT ErrorType, ErrorColumn, count(*) AS TotalCount FROM " + tableName + "_Errors WHERE ErrorLevel = '" + errorLevel + "' GROUP BY ErrorType, ErrorColumn;",
                                                            Validate, "SUMMARY RESULTS (" + errorLevel + ")", dest);
        }
        public void PrintDetailResults(string tableName, Globals.ResultWriterDestination dest)
        {
            string errorLevel = (dest == Globals.ResultWriterDestination.stdOut) ? "Error" : "Warning";
            SqliteStatus = db.ExecuteQuery("SELECT * FROM " + tableName + "_Errors WHERE ErrorLevel = '" + errorLevel + "' ORDER BY ErrorType, ErrorColumn",
                                            Validate, "DETAIL RESULTS (" + errorLevel + ")", dest);
        }
        public void ValidateCustom(FileInfo validationFile, int errorLimit, bool configWarnings)
        {
            string context = String.Empty;
            string sql = String.Empty;
            bool isWarning = false;

            List<string> contents = File.ReadAllText(validationFile.FullName).Split('\n').ToList();
            foreach (var line in contents)
            {
                if ((line.Length > 2) && Regex.IsMatch(line, @"\s*-{2}.*")) // (line.Substring(0, 2) == "--"))
                {
                    if (Regex.IsMatch(line, @"\s*-{2}\s*Context:.*"))
                        context += line;
                    else if (Regex.IsMatch(line, @"\s*-{2}\s*ErrorLevel:.*[W|w]arning"))
                        isWarning = true;
                }
                else
                    sql += " " + line;
            }

            if (string.IsNullOrWhiteSpace(context))
                context = validationFile.Name;

            if (errorLimit != Int32.MaxValue) // 2147483647
            {
                var regex = new Regex(@"LIMIT (\d+)");
                var results = regex.Matches(sql);
                if (results.Count > 0)
                {
                    foreach (Match m in results)
                    {
                        sql = sql.Replace(m.Value, "LIMIT " + errorLimit.ToString());
                    }
                }
                else
                {
                    sql += " LIMIT " + errorLimit.ToString();
                }

            }

            if (isWarning && configWarnings)
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount, "(Warning) " + context, Globals.ResultWriterDestination.Warning);
            else if (!isWarning)
                SqliteStatus = db.ExecuteQuery(sql, Validate, ErrorCount, "(Error) " + context);
        }
    }
}
