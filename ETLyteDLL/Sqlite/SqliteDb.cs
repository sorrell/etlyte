using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SQLitePCL.pretty;
using SQLitePCL;
using CsvHelper;

namespace ETLyteDLL
{
    public class SqliteDb
    {

		private IResultWriter writer;
        private string _dbname;
        private SQLiteDatabaseConnection _dbcon;
        private sqlite3 _rawdbcon;
        private string _lastError;
        public string Delimiter { get; set; }
        public string DbLogPath { get; set; }
        public string LastError
        {
            get { return _lastError; }
            private set
            {
                if (!string.IsNullOrWhiteSpace(DbLogPath))
                {
                    bool append = true;
                    if (_lastError == null)
                        append = false;
                    using (var tw = new StreamWriter(DbLogPath, append))
                    {
                        tw.WriteLine(value);
                        tw.Close();
                    }
                }
                _lastError = value;
            }
        }
        
        public SqliteDb(string db = null, bool useExistingDb = false, IResultWriter w = null, string delimiter = "\t", string dbLogPath = "") 
        { 
			try
			{
                if (db != null && useExistingDb == false)
                    File.Delete(db);
                _dbname = db;
	            Delimiter = delimiter;
				writer = w;
                DbLogPath = dbLogPath;
                if (!string.IsNullOrWhiteSpace(DbLogPath))
                    File.Delete(DbLogPath);
			}
			catch (Exception e) 
			{
				writer = w;
				writer.WriteError (e.Message);
			}
        }

        public sqlite3 RawDbConnection
        {
            get
            {
                if (_rawdbcon == null)
                {
                    if (_dbname == null)
                        raw.sqlite3_open(":memory:", out _rawdbcon);
                    else
                        raw.sqlite3_open(_dbname, out _rawdbcon);
                }
                return _rawdbcon;
            }
        }

        public SQLiteDatabaseConnection DbConnection
        {
            get
            {
                if (_dbcon == null)
                {
                    SQLiteDatabaseConnectionBuilder dbbuilder;
                    if (_dbname == null)
                        dbbuilder = SQLiteDatabaseConnectionBuilder
                                .InMemory;
                    else
                        dbbuilder = SQLiteDatabaseConnectionBuilder
                                .Create(_dbname);
                               
                    _dbcon = dbbuilder
                                .WithScalarFunc("REGEXP", SqliteExtensions.regexFunc)
                                .WithScalarFunc("ISDATETIME", SqliteExtensions.dateFunc)
                                .WithScalarFunc("ISBOOL", SqliteExtensions.boolFunc)
                                .WithScalarFunc("ISBYTE", SqliteExtensions.byteFunc)
                                .WithScalarFunc("ISSBYTE", SqliteExtensions.sbyteFunc)
                                .WithScalarFunc("ISSHORT", SqliteExtensions.shortFunc)
                                .WithScalarFunc("ISUSHORT", SqliteExtensions.ushortFunc)
                                .WithScalarFunc("ISINT", SqliteExtensions.intFunc)
                                .WithScalarFunc("ISUINT", SqliteExtensions.uintFunc)
                                .WithScalarFunc("ISLONG", SqliteExtensions.longFunc)
                                .WithScalarFunc("ISULONG", SqliteExtensions.ulongFunc)
                                .WithScalarFunc("ISFLOAT", SqliteExtensions.floatFunc)
                                .WithScalarFunc("ISDOUBLE", SqliteExtensions.doubleFunc)
                                .WithScalarFunc("ISDECIMAL", SqliteExtensions.decimalFunc)
                                .WithScalarFunc("ISCHAR", SqliteExtensions.charFunc)
                                .WithScalarFunc("ISTIMESPAN", SqliteExtensions.timespanFunc)
                                .WithScalarFunc("ISURI", SqliteExtensions.uriFunc)
                                .WithScalarFunc("DATECHK", SqliteExtensions.dateCompFunc)
                                .WithScalarFunc("COMPARE", SqliteExtensions.compareFunc)
                                .WithScalarFunc("UUID", SqliteExtensions.uuidFunc)
                                .WithScalarFunc("ROW_NUMBER", SqliteExtensions.rowNumFunc)
                                .Build();
                }
                return _dbcon;
            }
        }
        
        public string GetErrorStringFromCode(ErrorCode code)
        {
            return Enum.GetName(typeof(ErrorCode), code);
        }
        public string GetErrorStringFromCode(int code)
        {
            return GetErrorStringFromCode((ErrorCode)code);
        }

        public int ModifyDdlFromSqlString(string sql)
        {
            int responseCode = raw.SQLITE_OK;
            foreach (var query in sql.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList())
            {
                responseCode = raw.sqlite3_exec(RawDbConnection, query + ";");
                if (responseCode != raw.SQLITE_OK)
                {
                    LastError = "ERROR - " + GetErrorStringFromCode(responseCode) + " on " + query + ". Error Msg: " + raw.sqlite3_errmsg(RawDbConnection) ;
                }
            }
            return responseCode;
        }

        
        public bool QueryHasResults(string sql)
        {
            bool hasResults = false;
            try
            {
                hasResults = DbConnection.Query(sql).Count<IReadOnlyList<IResultSetValue>>() > 0;
            }
            catch (SQLiteException e) { }
            return hasResults;
        }
        
        public IEnumerator<IReadOnlyList<IResultSetValue>> GetEnumeratorForQuery(string sql)
        {
            var result = DbConnection.Query(sql);
            return result.GetEnumerator();
        }

        public IReadOnlyList<IResultSetValue> GetFirstResultFromQuery(string sql)
        {
            var e = GetEnumeratorForQuery(sql);
            e.MoveNext();
            return e.Current;
        }


        public int ExecuteQuery(string sql, IResultWriter sw, string context = "", Globals.ResultWriterDestination dest = Globals.ResultWriterDestination.stdOut)
        {
            Action<Int64> temp = (i => i=0);
            return ExecuteQuery(sql, sw, temp, context, dest);
        }

        public int ExecuteQuery(string sql, IResultWriter sw, Action<Int64> setResults, string context = "", Globals.ResultWriterDestination dest = Globals.ResultWriterDestination.stdOut)
        {
            Int64 numResults = 0;
            if (sql.Contains("ROW_NUMBER("))
            {
                SqliteExtensions.RowNumDictionary = new Dictionary<string, int>();
            }
                if (sql.Contains("JSON("))
            {
                var match = Regex.Match(sql, @"JSON\((?<key>.*?)\)");
                sql = Regex.Replace(sql, @"JSON\(\w*\)", "", RegexOptions.IgnoreCase);
                sw.Key = match.Groups["key"].Value;
            }
            int resp = (int)ErrorCode.Ok;
            try
            {
	            var stmts = DbConnection.PrepareAll(CleanSql(sql));
	            foreach (var stmt in stmts)
	            {
	                if (stmt.SQL != null && stmt.MoveNext())
	                {
                        var headers = stmt.Columns.Select(col => col.Name).ToList<string>();

                        sw
                            .BeginContext(context, dest)
                            .WriteHeaders(headers, dest)
                            .WriteResult(stmt.Current, dest);
    
	                    // Write the rest...
	                    while (stmt.MoveNext())
	                    {
                            numResults++;
	                        sw.WriteResult(stmt.Current, dest);
	                    }
                        sw.EndContext(dest);
                    }
	            }
            }
            catch (SQLiteException e)
            {
                resp = (int)e.ErrorCode;
                LastError = e.Message + " -- " + raw.sqlite3_errmsg(RawDbConnection) + " -- on query: " + sql;
            }
            finally
            {
                sw.Flush();
                Int64 changes = DbConnection.Changes;
                // if we didn't make any db changes, then we performed a query, so let's return the number of results in that query
                if (changes == 0 && numResults != 0)
                    changes = numResults;
                setResults(changes);
            }

            return resp;
        }

        
        private string GetInsertFmtForNormalCols(Flatfile flatfile, bool insertLineNum)
        {
            var sql = insertLineNum ? ", LineNum" : "";
            sql = string.Join(",",flatfile.Schemafile.Fields.Where(x => x.ColumnType == ColumnType.Normal).Select(x => x.Name).ToList()) + sql;
            return "INSERT INTO " + flatfile.Tablename + " (" + sql + ") VALUES ({0});";
        }
        private string GetInsertStmtForNormalCols(Flatfile flatfile, bool insertLineNum)
        {
            var sql = insertLineNum ? ", LineNum" : "";
            var lncnt = insertLineNum ? 1 : 0;
            var qmark = "";
            var colList = flatfile.Schemafile.Fields.Where(x => x.ColumnType == ColumnType.Normal).Select(x => x.Name).ToList();
            sql = string.Join(",", colList) + sql;
            for (int i = 1; i <= colList.Count+lncnt; i++)
            {
                qmark += "?" + i.ToString() + ",";
            }
            return "INSERT INTO " + flatfile.Tablename + " (" + sql + ") VALUES (" + qmark.Substring(0, qmark.Length-1) + ")";
        }

        private string CleanSql(string sql)
        {
            sql = sql.Trim();
            if (sql[sql.Length - 1] == ',')
                sql = sql.Remove(sql.Length - 1);
            if (sql[sql.Length - 1] != ';')
                sql += ";";
            return sql;
        }
        private string[] SanitizeInputs(string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] != null && fields[i].ToLower() != "null")
                    fields[i] = fields[i].Replace("'", "''").Trim().SingleQuote();
            }
            return fields;
        }
        public int ImportDelimitedFile(Flatfile flatfile, out int linesRead, ConfigFile configFile, bool insertLineNum = false)
        {
            int i = 1;
            int sqliteStatus = 0;

            try
            {
                string insertStmt = GetInsertStmtForNormalCols(flatfile, insertLineNum);
                raw.sqlite3_exec(RawDbConnection, "BEGIN");
                //NPS_TODO : Get the column count of the table
                // rc = sqlite3_prepare_v2(p->db, zSql, -1, &pStmt, 0) where zSQL is a select * from col
                // if no rc, then create table
                // then 
                // nCol = sqlite3_column_count(pStmt);
                sqlite3_stmt stmt;
                bool skipRows = flatfile.Schemafile.SkipRows != 0;
                sqliteStatus = raw.sqlite3_prepare_v2(RawDbConnection, insertStmt, out stmt);
                using (StreamReader reader = File.OpenText(flatfile.Filename))
                {
                    var parser = new CsvParser(reader);
                    parser.Configuration.Delimiter = flatfile.Delimiter;

                    while (true)
                    {
                        //Console.WriteLine("1- "+DateTime.Now.Ticks);
                        var line = parser.Read();
                        if (line == null)
                        {
                            break;
                        }
                        //Console.WriteLine("2- " + DateTime.Now.Ticks);
                        if (skipRows && i - 1 < flatfile.Schemafile.SkipRows)
                        {
                            i++;
                            continue;
                        }
                        //Console.WriteLine("3- " + DateTime.Now.Ticks);
  
                     

                        int columnCount = flatfile.Schemafile.Fields.Count - 1;
                        if ((i == 1 || (skipRows && (i - 1) == flatfile.Schemafile.SkipRows)) && flatfile.HasHeaderRow)
                        {
                            flatfile.Headers = line;
                            if (flatfile.HasValidHeaderCount())
                            {
                                var invalidHeaders = flatfile.ValidateHeaderNames();
                                if (!String.IsNullOrEmpty(invalidHeaders))
                                    sqliteStatus = raw.sqlite3_exec(RawDbConnection, "INSERT INTO GeneralErrors VALUES('Malformed Header','None', + '" + flatfile.Tablename.ToUpper() + "', '" + configFile.Validate.SchemaErrorSettings.MalformedHeaderErrorLevel + "', 'Malformed Header Row - " + invalidHeaders + "');");
                            }
                            i++;
                            continue;
                        }
                        //Console.WriteLine("5- " + DateTime.Now.Ticks);
                        var linenum = insertLineNum ? "," + parser.Row : "";
                        List<string> newLine = new List<string>();

                        if ((line.Count() > columnCount) && configFile.Extract.IgnoreTooManyColumns)
                        {

                            for (var num = 0; num < flatfile.Schemafile.Fields.Count - 1; num++)
                                newLine.Add(line[num]);
                            line = newLine.ToArray();
                        }
                        //Console.WriteLine("6- " + DateTime.Now.Ticks);
                        if ((line.Where(x => x == null).Count() > 0) && configFile.Extract.MissingColumns.MissingColumnHandling.ToLower() == "fill")
                        {
                            newLine = line.Where(col => col != null).ToList();
                            var fillVal = configFile.Extract.MissingColumns.MissingColumnFillValue;
                            while (newLine.Count < columnCount)
                            {
                                newLine.Add(fillVal);
                            }

                            line = newLine.ToArray();
                        }
                        //Console.WriteLine("7- " + DateTime.Now.Ticks);
                        raw.sqlite3_reset(stmt);
                        string sql = "";
                        //string sql = String.Format(insertStmt, String.Join(@",", SanitizeInputs(line)) + linenum) + Environment.NewLine;
                       
                        for (int k = 1; k <= line.Length; k++)
                        {
                            sqliteStatus = raw.sqlite3_bind_text(stmt, k, line[k - 1]);
                        }
                        if (linenum != "") { sqliteStatus = raw.sqlite3_bind_text(stmt, line.Length + 1, i.ToString()); }
                        //Console.WriteLine("8- " + DateTime.Now.Ticks);
                        //sqliteStatus = raw.sqlite3_exec(RawDbConnection, sql);
                        raw.sqlite3_step(stmt);
                        raw.sqlite3_clear_bindings(stmt);
                        //Console.WriteLine("9- " + DateTime.Now.Ticks);
                        // This next line really implies that MissingColumnHandling = error
                        if ((sqliteStatus != raw.SQLITE_OK && sqliteStatus != raw.SQLITE_DONE))
                        {
                            string error = raw.sqlite3_errmsg(RawDbConnection);
                            sqliteStatus = raw.sqlite3_exec(RawDbConnection, "INSERT INTO GeneralErrors VALUES('Insert Error', 'None','" + flatfile.Tablename.ToUpper() + "', 'Error','Failed to import lines in " + flatfile.Filename +
                                                                " due to error: " + error + Environment.NewLine + "Line: " + i + ": " + sql.Replace("'", "''") + "')");
                            if (configFile.Db.StopOnError)
                            {
                                raw.sqlite3_finalize(stmt);
                                raw.sqlite3_exec(RawDbConnection, "COMMIT");
                                break;
                            }
                        }
                    
                        i++;
                    }
                }
                raw.sqlite3_finalize(stmt);
                raw.sqlite3_exec(RawDbConnection, "COMMIT");
                linesRead = i;
                return (int)ErrorCode.Ok;
            }
            catch (Exception e)
            {
                linesRead = i;
                return sqliteStatus;
            }
        }

        public List<string> GetColumnNamesForTable(string tableName)
        {
            List<string> cols = new List<string>();
            var colSql = "SELECT sql FROM sqlite_master WHERE tbl_name = " + tableName.SingleQuote() + ";";
            var resultSql = GetFirstResultFromQuery(colSql)[0].ToString();
            var results = Regex.Match(resultSql.Replace("\r", "").Replace("\n", "").Trim(), @"\((.*)").Value.Replace("(", "").Replace(")", "").Split(',');
            foreach (var r in results)
                cols.Add(r.Trim().Replace("[", "").Replace("]", "").Split(' ')[0]);
            return cols;
        }

        public List<string> GetTableNames()
        {
            List<string> tables = new List<string>();
            var tblSql = "SELECT tbl_name FROM sqlite_master WHERE type = 'table';";
            var enumerator = GetEnumeratorForQuery(tblSql);
            while (enumerator.MoveNext())
                tables.Add(enumerator.Current[0].ToString());
            return tables;
        }

    }
}
