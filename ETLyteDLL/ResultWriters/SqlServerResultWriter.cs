using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL.pretty;
using System.IO;
using System.Data.OleDb;

namespace ETLyteDLL
{
    public class SqlServerResultWriter : IResultWriter
    {
        public string dbConnectionString { get; set; }
        public bool dbCanConnect { get; private set; }
        private OleDbConnection stdOut { get; set; }
        public TextWriter VerboseOut { get; set; }
        public TextWriter ErrorOut { get; set; }
        public TextWriter WarningOut { get; set; }
        public SqliteDb SqliteDb { get; set; }
        private List<string> colNames { get; set; }
        public string Key { get; set; }
        public string ResultMode { get; }
        private int ContextsWritten { get; set; }
        public Stack<ResultContext> ContextStack { get; set; }
        public ResultContext CurrentContext { get; set; }
        private bool TableIsCreated { get; set; }
        private string CurrentContextName { get; set; }
        /// <summary>
        /// A tuple to handle the mapping between [DataType, MSSQLDatatype, Quoted/Unquoted field]
        /// </summary>
        private List<Tuple<DataType, string, string>> SqliteToMsSqlLookup { get; set; }
        private Dictionary<string, bool> ContextColumnIsQuoted { get; set; }
       
        public SqlServerResultWriter(string connectionString, SqliteDb db = null, TextWriter verbose = null, TextWriter e = null, TextWriter warning = null)
        {
            //var connstr = "Data Source=.;Initial Catalog=Test;Integrated Security=SSPI;Provider=SQLOLEDB;";
            dbCanConnect = false;
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                dbCanConnect = true;
                dbConnectionString = connectionString;
                connection.Close();
            }
            VerboseOut = verbose ?? TextWriter.Null;
            ErrorOut = e ?? TextWriter.Null;
            WarningOut = warning ?? TextWriter.Null;
            Key = "";
            ResultMode = "SQLSVR";
            ContextStack = new Stack<ResultContext>();
            CurrentContextName = "";
            TableIsCreated = false;
            SqliteDb = db;
            CreateLookup();
        }

        private void CreateLookup()
        {
            SqliteToMsSqlLookup = new List<Tuple<DataType, string, string>>()
            {
                Tuple.Create(DataType.Boolean, "bit", "unquoted"),
                Tuple.Create(DataType.@byte, "tinyint", "unquoted"),
                Tuple.Create(DataType.@char, "nchar(1)", "quoted"),
                Tuple.Create(DataType.DateTime, "datetime", "quoted"),
                Tuple.Create(DataType.@decimal, "decimal({0})", "unquoted"),
                Tuple.Create(DataType.@double, "numeric({0})", "unquoted"),
                Tuple.Create(DataType.@float, "numeric({0})", "unquoted"),
                Tuple.Create(DataType.@int, "int", "unquoted"),
                Tuple.Create(DataType.@long, "bigint", "unquoted"),
                Tuple.Create(DataType.@object, "sqlvariant", "quoted"),
                Tuple.Create(DataType.@sbyte, "smallint CHECK ({0} >= -127 AND {0} <= 128)", "unquoted"),
                Tuple.Create(DataType.@short, "smallint", "unquoted"),
                Tuple.Create(DataType.@string, "nvarchar(max)", "quoted"),
                Tuple.Create(DataType.TimeSpan, "nvarchar(max)", "quoted"),     // maybe later store as ticks? http://stackoverflow.com/a/8504020/974077
                Tuple.Create(DataType.@uint, "numeric(10) CHECK ({0} >= 0 AND {0} <= 4,294,967,295)", "unquoted"),
                Tuple.Create(DataType.@ulong, "numeric(20) CHECK ({0} >= 0 AND {0} <= 18,446,744,073,709,551,615)", "unquoted"),
                Tuple.Create(DataType.URI, "nvarchar(max)", "quoted"),
                Tuple.Create(DataType.@ushort, "numeric(5) CHECK ({0} >= 0 AND {0} <= 65,535)", "unquoted")

            };
        }

        public void Flush()
        {
            //stdOut.Flush();
            VerboseOut.Flush();
            ErrorOut.Flush();
        }

        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SqlServerResultWriter()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing = true)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                stdOut.Close();
                VerboseOut.Close();
                ErrorOut.Close();
                stdOut.Dispose();
                VerboseOut.Dispose();
                ErrorOut.Dispose();
            }

            stdOut.Close();
            VerboseOut.Close();
            ErrorOut.Close();
            stdOut.Dispose();
            VerboseOut.Dispose();
            ErrorOut.Dispose();
            // release any unmanaged objects
            // set the object references to null

            _disposed = true;
        }

        public IResultWriter Write(string line, Globals.ResultWriterDestination dest)
        {
            switch (dest)
            {
                case Globals.ResultWriterDestination.stdOut:
                    WriteStd(line);
                    break;
                case Globals.ResultWriterDestination.Error:
                    WriteError(line);
                    break;
                case Globals.ResultWriterDestination.Verbose:
                    WriteVerbose(line);
                    break;
                case Globals.ResultWriterDestination.Warning:
                    WriteWarning(line);
                    break;
                default:
                    break;
            }
            return this;
        }
        public IResultWriter WriteWarning(string line)
        {
            WarningOut.WriteLine(line);
            return this;
        }
        public IResultWriter WriteResult(IReadOnlyList<IResultSetValue> result, Globals.ResultWriterDestination dest)
        {
            if (!TableIsCreated)
            {
                string sql = "CREATE TABLE [" + CurrentContextName + "] ( {0} );";
                List<string> cols = new List<string>();
                foreach (var r in result)
                {
                    var colname = r.ColumnInfo.Name;
                    bool isQuoted = false;
                    string mssqlDatatype = "";
                    if (r.ColumnInfo.TableName != null)
                    { 
                        var query = "SELECT ColumnType FROM TableMetadata WHERE Tablename = " + 
                                r.ColumnInfo.TableName.SingleQuote() + " AND ColumnName = " + colname.SingleQuote() + ";";
                        var enumerator = SqliteDb.GetEnumeratorForQuery(query);
                        if (enumerator.MoveNext())
                        {
                            var sqliteDatatype = enumerator.Current[0].ToString();
                            var tuple = SqliteToMsSqlLookup
                                .Where(t => (t.Item1.ToString().ToLower() == sqliteDatatype.ToLower()))
                                .Select(t => t).ElementAt(0);
                            var etlDatatype = tuple.Item1;
                            mssqlDatatype = (tuple.Item2 != null) ? tuple.Item2 : "varchar(max)";
                            isQuoted = (tuple.Item3.ToLower() == "quoted");
                            if (etlDatatype == DataType.@decimal || etlDatatype == DataType.@double || etlDatatype == DataType.@float)
                            {
                                string precscale = "18";
                                var precisionSql = "with cte as (" +
                                                        "select distinct length(" + colname + ")-1 as precision, length(" + colname + ") - instr(" + colname + ", '.') as scale, " +
                                                        "length(" + colname + ")-1 - (length(" + colname + ") - instr(" + colname + ", '.')) as beforeDec " +
                                                        "from " + r.ColumnInfo.TableName +
                                                    ") " +
                                                   "select case when max(precision)+max(beforeDec)-1 - min(scale) >= 38 then 38 " +
                                                                "else max(precision)+max(beforeDec)-1 " +
                                                           "end as precision, max(scale) as scale " +
                                                   "from cte;";
                                enumerator = SqliteDb.GetEnumeratorForQuery(precisionSql);
                                if (enumerator.MoveNext())
                                {
                                    int precnum, scalenum = 0;
                                    int.TryParse(enumerator.Current[0].ToString(), out precnum);
                                    int.TryParse(enumerator.Current[1].ToString(), out scalenum);
                                    var prec = (string.IsNullOrWhiteSpace(enumerator.Current[0].ToString()) || precnum <= 0) ? "18," : enumerator.Current[0].ToString() + ",";
                                    var scale = (string.IsNullOrWhiteSpace(enumerator.Current[1].ToString()) || scalenum <= 0) ? "2" : enumerator.Current[1].ToString();
                                    precscale = prec + scale;
                                }
                                mssqlDatatype = string.Format(mssqlDatatype, precscale);
                            }
                            else if (etlDatatype == DataType.@sbyte || etlDatatype == DataType.@uint || etlDatatype == DataType.@ulong || etlDatatype == DataType.@ushort)
                            {
                                mssqlDatatype = string.Format(mssqlDatatype, colname);
                            }
                        }
                        else
                        {
                            mssqlDatatype = string.Format("varchar(max)", colname);
                            isQuoted = true;
                        }
                    }
                    else
                    {
                        // NPS_TODO try to ascertain datatype
                        mssqlDatatype = "nvarchar(max)";
                        ContextColumnIsQuoted.Add("null_" + colname, true);
                    }
                    cols.Add(colname + " " + mssqlDatatype);
                    ContextColumnIsQuoted.Add(r.ColumnInfo.TableName + "_" + colname, isQuoted);
                }
                sql = string.Format(sql, string.Join(",", cols));
                Console.WriteLine(sql);
                Write(sql, dest);
                TableIsCreated = true;
            }
                  
            Write(GetResultAsSqlInsertString(result), dest);
            return this;
        }

        public IResultWriter WriteStd(string line)
        {
            ExecuteSqlSvrCmd(line);
            return this;
        }

        public IResultWriter WriteVerbose(string line)
        {
            VerboseOut.WriteLine(line);
            return this;
        }

        public IResultWriter WriteError(string line)
        {
            ErrorOut.WriteLine(line);
            return this;
        }


        public IResultWriter BeginContext(string context)
        {
            if (CurrentContext != null)
                ContextStack.Push(CurrentContext);
            CurrentContextName = context.Replace(".", "_");
            CurrentContextName += String.Join("_", ContextStack.Select(x => x.Name));
            CurrentContext = new ResultContext(context);
            TableIsCreated = false;
            ContextColumnIsQuoted = new Dictionary<string, bool>();
            //stdOut.WriteLine(string.Concat(Enumerable.Repeat(Delimiter, IndentationLevel)) + context);
            return this;
        }

        public IResultWriter EndContext()
        {
            if (ContextStack.Count > 0)
                CurrentContext = ContextStack.Pop();
            colNames = new List<string>();
            return this;
        }

        public IResultWriter WriteHeaders(List<string> headers)
        {
            colNames = headers;
            return this;
        }

        private string GetResultAsSqlInsertString(IReadOnlyList<IResultSetValue> stmt)
        {
            var sql = "INSERT INTO [" + CurrentContextName + "] (" + String.Join(",", colNames) + ") VALUES (" +
                string.Join(",",
                    stmt.Select(a => (a.ColumnInfo.TableName == null) || (ContextColumnIsQuoted[((IResultSetValue)a).ColumnInfo.TableName +"_"+ ((IResultSetValue)a).ColumnInfo.Name])
                                   ? a.ToString().SingleQuote()
                                   : a.ToString())) + ");";
            return sql ;
        }


        public IResultWriter Reset()
        {
            return this;
        }

        public IResultWriter BeginOutput(string beginStr = "")
        {
            if (dbCanConnect)
            {
                stdOut = new OleDbConnection(dbConnectionString);
                stdOut.Open();
            }
            return this;
        }

        public IResultWriter EndOutput(string endStr = "")
        {
            stdOut.Close();
            stdOut.Dispose();
            return this;
        }

        private bool ExecuteSqlSvrCmd(string sql)
        {
            if (stdOut.State != System.Data.ConnectionState.Open)
                stdOut.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand(sql, stdOut);
                cmd.ExecuteNonQuery();
            }
            catch (OleDbException e)
            {
                // NPS_TODO handle exception, write to log
                var x = e;
            }
            finally
            {
                stdOut.Close();
            }
            return true;
        }
    }
}
