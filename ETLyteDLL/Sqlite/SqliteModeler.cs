using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class SqliteModeler
    {
        public SqliteModeler() 
        {
        }
        public string Ddl { get; private set; }
        public string ErrorDdl { get; private set; }
        
        public SchemaFile SchemaFile { get; set; }
        public SqliteModeler SetSchemaFile(SchemaFile s)
        {
            try
            {
                SchemaFile = s;
                return this;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private string GetFieldsForDdl(bool withLineNumCol)
        {
            string output = String.Empty;
            foreach (var field in SchemaFile.Fields.Select((value, i) => new { i, value }))
            {
                output += field.value.Name + " " + TypeLookup(field.value.DataType) + " NULL";
                //output += field.value.Required ? "NOT NULL" : "NULL";
                if (field.i < SchemaFile.Fields.Count - 1)
                    output += ",";
                else
                {
                    if (withLineNumCol)
                        output += ", LineNum INTEGER NOT NULL";
                    output += "\n);";
                }
            }
            return output;
        }
        public SqliteModeler CreateDdl(bool withLineNumCol = false)
        {
            string output = String.Empty;
            try
            {
                output = "CREATE TABLE " + SchemaFile.Name + " (";
                output += GetFieldsForDdl(withLineNumCol);
                Ddl += output;
                return this;
            } 
            catch (Exception ex)
            {
                return null;
            }
        }
        public SqliteModeler CreateErrorDdl()
        {
            string output = String.Empty;
            try
            {
                output = "CREATE TABLE " + SchemaFile.Name + "_Errors ( ErrorType TEXT, ErrorColumn TEXT, ErrorLevel TEXT, ";
                output += GetFieldsForDdl(true);
                ErrorDdl += output;
                return this;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public string CreateGeneralErrorWarningDdl()
        {
            string output = String.Empty;
            try
            {
                output = "CREATE TABLE GeneralErrors (ErrorType TEXT, ErrorColumn TEXT, ErrorTable TEXT, ErrorLevel TEXT, ErrorDescription TEXT); " +
                         "CREATE TABLE TableMetadata (Tablename TEXT, ColumnName TEXT, ColumnType TEXT); " +
                         "CREATE TABLE FileAudit (Filename TEXT, Readtime TEXT, ColumnsRead INT, RowsRead INT); ";
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }
            return output;
        }


        private string TypeLookup(DataType t)
        {
            string retstr = String.Empty;
            switch (t)
            {
                case DataType.@object:
                    retstr = "BLOB";
                    break;
                //case DataType.@string:
                //    retstr = "TEXT";
                //    break;
                //case DataType.@int:
                //    retstr = "INTEGER";
                //    break;
                //case DataType.@byte:
                //    retstr = "TEXT";
                //    break;
                //case DataType.@sbyte:
                //    retstr = "TEXT";
                //    break;
                //case DataType.@uint:
                //    retstr = "TEXT";
                //    break;
                //case DataType.@ulong:
                //    retstr = "TEXT";
                //    break;
                //case DataType.@float:
                //    retstr = "REAL";
                //    break;
                //case DataType.@double:
                //    retstr = "TEXT";
                //    break;
                //case DataType.DateTime:
                //    retstr = "TEXT";
                //    break;
                //case DataType.Boolean:
                //    retstr = "INTEGER";
                //    break;
                //case DataType.URI:
                //    retstr = "TEXT";
                //    break;
                default:
                    retstr = "TEXT";
                    break;
            }
            return retstr;
        }
    }
}
