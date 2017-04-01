using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public static class ConfigFileInit
    {
        public static IResultWriter InitExtractFromConfig(ConfigFile configFile)
        {
            return null;
        }
        public static IResultWriter InitValidateFromConfig(ConfigFile configFile)
        {
            IResultWriter ResultWriter;
            ConfigOutputs vout = null;
            TextWriter outputConnection = TextWriter.Null;
            if (configFile.Validate != null && configFile.Validate.Outputs != null)
            {
                vout = configFile.Validate.Outputs;
                outputConnection = GetConnectionType(vout.StandardOutputConnectionString);

            }
            if (vout != null && vout.StandardOutputType != null)
            {
                if (vout.StandardOutputType == "json")
                {
                    ResultWriter = new JsonResultWriter();
                    ((JsonResultWriter)ResultWriter).stdOut = outputConnection;
                    if (vout.Verbose != false)
                    {
                        ((JsonResultWriter)ResultWriter).VerboseOut = (vout.VerboseOutputConnectionString == vout.StandardOutputConnectionString)
                            ? ((JsonResultWriter)ResultWriter).stdOut
                            : GetConnectionType(vout.VerboseOutputConnectionString);
                    }
                    if (vout.Warnings != false)
                    {
                        ((JsonResultWriter)ResultWriter).WarningOut = (vout.WarningsOutputConnectionString == vout.StandardOutputConnectionString)
                            ? ((JsonResultWriter)ResultWriter).stdOut
                            : ((vout.WarningsOutputConnectionString == vout.VerboseOutputConnectionString)
                                ? GetConnectionType(vout.VerboseOutputConnectionString)
                                : GetConnectionType(vout.WarningsOutputConnectionString));
                    }
                    ((JsonResultWriter)ResultWriter).ErrorOut = GetConnectionType(vout.ErrorOutputConnectionString);
                }

                else if (vout.StandardOutputType == "delimited" && vout.StandardOutputDelimiter != null)
                {
                    ResultWriter = new DelimitedIndentedResultWriter(vout.StandardOutputDelimiter);
                    ((DelimitedIndentedResultWriter)ResultWriter).stdOut = outputConnection;
                    if (vout.Verbose != false)
                    {
                        ((DelimitedIndentedResultWriter)ResultWriter).VerboseOut = (vout.VerboseOutputConnectionString == vout.StandardOutputConnectionString)
                            ? ((DelimitedIndentedResultWriter)ResultWriter).stdOut
                            : GetConnectionType(vout.VerboseOutputConnectionString);
                    }
                    if (vout.Warnings != false)
                    {
                        ((DelimitedIndentedResultWriter)ResultWriter).WarningOut = (vout.WarningsOutputConnectionString == vout.StandardOutputConnectionString)
                            ? ((DelimitedIndentedResultWriter)ResultWriter).stdOut
                            : ((vout.WarningsOutputConnectionString == vout.VerboseOutputConnectionString) 
                                ? GetConnectionType(vout.VerboseOutputConnectionString)
                                : GetConnectionType(vout.WarningsOutputConnectionString));
                    }
                    ((DelimitedIndentedResultWriter)ResultWriter).ErrorOut = GetConnectionType(vout.ErrorOutputConnectionString);
                }

                //NPS_TODO: sql-server
                else
                {
                    ResultWriter = new DelimitedIndentedResultWriter();
                    ((DelimitedIndentedResultWriter)ResultWriter).stdOut = outputConnection;
                }
            }

            else
            {
                ResultWriter = new DelimitedIndentedResultWriter(Globals.Delimiter);
                ((DelimitedIndentedResultWriter)ResultWriter).stdOut = Console.Out;
            }

            return ResultWriter;
        }

        public static IResultWriter InitTransformFromConfig(ConfigFile configFile)
        {
            return null;
        }

        public static IResultWriter InitLoadFromConfig(ConfigFile configFile, SqliteDb db)
        {
            var connstr = "Data Source=.;Initial Catalog=Test;Integrated Security=SSPI;Provider=SQLOLEDB;";
            SqlServerResultWriter ResultWriter = new SqlServerResultWriter(connstr, db, Console.Out, Console.Out, Console.Out);


            return ResultWriter;
        }
        static TextWriter GetConnectionType(string con)
        {
            if (con.ToLower() == "stdout")
                return Console.Out;
            else if (!string.IsNullOrWhiteSpace(con))
                return File.CreateText(con);

            return TextWriter.Null;
        }
    }
}
