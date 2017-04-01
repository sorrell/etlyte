using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public static class ExtMethods
    {
        public static string DoubleQuote(this string str)
        {
            return "\"" + str + "\"";
        }
        public static string SingleQuote(this string str)
        {
            return "'" + str + "'";
        }
        public static OleDbConnection WriteLine(this OleDbConnection db, string line)
        {
            // do stuff
            return db;
        }
    }
}
