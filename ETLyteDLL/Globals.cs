using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class Globals
    {
        public const string DbName = "fv.db";
        public const string Delimiter = ",";
        public const string Quote = "\"";
        public const string LineTerminator = "\n";
        public const string Version = "0.1.0";

        public enum ResultWriterDestination
        {
            stdOut,
            Verbose,
            Error,
            Warning
        }
    }
}
