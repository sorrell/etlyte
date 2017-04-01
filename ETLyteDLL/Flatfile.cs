using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class Flatfile
    {
        public string Filename { get; set; }
        public string Tablename { get; set; }
        public string Delimiter { get; set; }
        public string Quoter { get; set; }
        public bool HasHeaderRow { get; set; }
        public string[] Headers { get; set; }
        public SchemaFile Schemafile { get; set; }
        public Flatfile(string filename = "", string tablename = "", string delimiter = Globals.Delimiter, string quoter = Globals.Quote, bool hasHeader = true, string[] headers = null, SchemaFile schema = null)
        {
            Filename = filename;
            Tablename = tablename;
            Delimiter = delimiter;
            Quoter = quoter;
            HasHeaderRow = hasHeader;
            Headers = headers;
            Schemafile = schema;
        }
        public string ValidateHeaderNames()
        {
            string invalidHeaders = "";
            for (int i = 0; i < Schemafile.Fields.Count; ++i)
            {
                if (Headers[i].StartsWith("'"))
                    Headers[i] = Headers[i].Substring(1, Headers[i].Length - 1);
                if (Headers[i].EndsWith("'"))
                    Headers[i] = Headers[i].Substring(0, Headers[i].Length - 1);
                if (Headers[i].ToUpper() != Schemafile.Fields.ElementAt(i).Name.ToUpper())
                // DAC Specific if (Headers[i].ToUpper() != "4WD_AWD")
                {
                    invalidHeaders += (invalidHeaders.Length > 0) ? " | " : "";
                    invalidHeaders += Headers[i] + "->" + Schemafile.Fields.ElementAt(i).Name;
                }
            }
            return invalidHeaders;
        }

        public bool HasValidHeaderCount()
        {
            return (Headers.Length == Schemafile.Fields.Count);
        }
    }
}
