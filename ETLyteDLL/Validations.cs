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

        private SchemaErrorSettings esettings;
        private SqliteDb db;
        private IResultWriter Validate;

        public Validations(SchemaErrorSettings inesettings, SqliteDb indb, IResultWriter inValidate, Func<int, int> cb)
        {
            SqliteStatusCallback = cb;
            esettings = inesettings;
            db = indb;
            Validate = inValidate;
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
                    sql = String.Format(sqlbase, "'Invalid Datatype'", errorLevel, "WHERE NOT ISDATETIME(" + schemaField.Name + ", '" + schemaField.DatePattern + "') ");
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
       
        public void ValidateFields(SchemaField schemaField, string tableName)
        {
            var sqlbase = @"INSERT INTO " + tableName + "_Errors" +
                          @" SELECT {0}, '" + schemaField.Name + @"', {1},*
                             FROM " + tableName + @" 
                             {2}";
            string sql = "";

            if (schemaField.Required)
            {
                sql = String.Format(sqlbase, "'Required Field'", esettings.RequiredErrorLevel.SingleQuote(), " WHERE " + schemaField.Name + " = '' OR " + schemaField.Name + " IS NULL");
                SqliteStatus = db.ExecuteQuery(sql, Validate);
            }

            if (schemaField.DataType != DataType.@string
                && (schemaField.Required || (esettings.ErrorOnUnrequiredWithBadDatatype && !schemaField.Required)))
            {
                sql = GetSqlForDatatype(schemaField, sqlbase, esettings.DatatypeErrorLevel.SingleQuote());
                SqliteStatus = db.ExecuteQuery(sql, Validate);
            }

            if (!double.IsNaN(schemaField.Maximum))
            {
                sql = String.Format(sqlbase, "'Maximum Value'", esettings.MaximumErrorLevel.SingleQuote(), "WHERE COMPARE(" + schemaField.Name + @", '>', " + schemaField.Maximum + @", '" + schemaField.Type.ToString() + "');");
                SqliteStatus = db.ExecuteQuery(sql, Validate);
            }
            if (!double.IsNaN(schemaField.Minimum))
            {
                sql = String.Format(sqlbase, "'Minimum Value'", esettings.MinimumErrorLevel.SingleQuote(), "WHERE COMPARE(" + schemaField.Name + @", '<', " + schemaField.Minimum + @", '" + schemaField.Type.ToString() + "');");
                SqliteStatus = db.ExecuteQuery(sql, Validate);
            }
            if (schemaField.MinLength > -1)
            {
                sql = String.Format(sqlbase, "'Minimum Length'", esettings.MinLengthErrorLevel.SingleQuote(), "WHERE LENGTH(" + schemaField.Name + @") < " + schemaField.MinLength + @";");
                SqliteStatus = db.ExecuteQuery(sql, Validate);
            }
            if (schemaField.MaxLength > -1)
            {
                sql = String.Format(sqlbase, "'Maximum Length'", esettings.MaxLengthErrorLevel.SingleQuote(), "WHERE LENGTH(" + schemaField.Name + @") > " + schemaField.MaxLength + @";");
                SqliteStatus = db.ExecuteQuery(sql, Validate);
            }
            if ((schemaField.Pattern != null) && (schemaField.Pattern != String.Empty))
            {
                sql = String.Format(sqlbase, "'Invalid Value(pattern)'", esettings.PatternErrorLevel.SingleQuote(), "WHERE '" + schemaField.Pattern + @"' NOT REGEXP " + schemaField.Name + @";");
                SqliteStatus = db.ExecuteQuery(sql, Validate);
            }
            if (schemaField.Unique)
            {
                sql = String.Format(sqlbase, "'Unique'", esettings.UniqueErrorLevel.SingleQuote(), " GROUP BY " + schemaField.Name + " HAVING COUNT(*) > 1;");
                SqliteStatus = db.ExecuteQuery(sql, Validate);
            }
        }

        public void PrintGeneralIssues(Globals.ResultWriterDestination dest)
        {
            var errorLevel = (dest == Globals.ResultWriterDestination.Error) ? "Error" : "Warning";
            SqliteStatus = db.ExecuteQuery("SELECT * FROM GeneralErrors WHERE ErrorLevel = '" + errorLevel + "' ORDER BY ErrorType, ErrorTable, ErrorColumn;", Validate, "General Errors");
        }
        public void PrintSummaryResults(string tableName)
        {
            SqliteStatus = db.ExecuteQuery("SELECT ErrorType, ErrorColumn, count(*) AS TotalCount FROM " + tableName + "_Errors GROUP BY ErrorType, ErrorColumn;",
                                                            Validate, "SUMMARY RESULTS");
        }
        public void PrintDetailResults(string tableName, string errorLevel)
        {
            SqliteStatus = db.ExecuteQuery("SELECT * FROM " + tableName + "_Errors WHERE ErrorLevel = '" + errorLevel + "' ORDER BY ErrorType, ErrorColumn",
                                            Validate, "DETAIL RESULTS");
        }
        public void ValidateCustom(FileInfo validationFile, int errorLimit)
        {
            string context = String.Empty;
            string sql = String.Empty;

            List<string> contents = File.ReadAllText(validationFile.FullName).Split('\n').ToList();
            foreach (var line in contents)
            {
                if ((line.Length > 2) && Regex.IsMatch(line, @"\s*-{2}.*")) // (line.Substring(0, 2) == "--"))
                {
                    if (Regex.IsMatch(line, @"\s*-{2}\s*Context:.*"))
                        context += line;
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
                        Console.WriteLine(m.Value);
                        sql = sql.Replace(m.Value, "LIMIT " + errorLimit.ToString());
                    }
                }
                else
                {
                    sql += " LIMIT " + errorLimit.ToString();
                }

            }

            SqliteStatus = db.ExecuteQuery(sql, Validate, context);
        }
    }
}