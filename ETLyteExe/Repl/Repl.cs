using Cintio;
using ETLyteDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteExe
{
    public static class Repl
    {
        public static IResultWriter Writer;
        public static void Run(string[] args, ConfigFile configFile)
        {
            string dbname = (args.Length > 1) && (args[1] != null) ? args[1] : null;
            SqliteDb db = new SqliteDb(dbname, true, null, configFile.Extract.Delimiter, configFile.Db.LogFile);
            if (dbname == null)
                dbname = "a transient in-memory database.";

            Writer = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out);
            var tables = db.GetTableNames();
            var columns = new List<string>();
            foreach (var t in tables)
                foreach (var col in db.GetColumnNamesForTable(t))
                    if (!columns.Contains(col))
                        columns.Add(col);
            var startupMsg = "ETLyte v. " + Globals.Version;
            startupMsg += Environment.NewLine + "Connected to " + dbname;
            string prompt = "ETLyte> ";
            CSharpEvaluator eval = new CSharpEvaluator();
            InteractivePrompt.Run(
                ((cmd, rawinput) =>
                {
                    string retstr = "";
                    var arg = cmd.Split(' ');
                    var firstArg = arg[0];
                    if (firstArg.Trim()[0] == '.')
                    {
                        switch (firstArg.ToLower())
                        {
                            case ".tables":
                                var sqlcmd = "SELECT name FROM sqlite_master WHERE type='table';";
                                if (db.ExecuteQuery(sqlcmd, Writer) != 0)
                                    Writer.WriteStd(db.LastError);
                                break;
                            case ".output":
                                if (arg[1].ToLower() == "json")
                                {
                                    Writer = new JsonResultWriter(Console.Out, Console.Out, Console.Out);
                                    retstr = "Switched to JSON output" + Environment.NewLine;
                                }
                                else if (arg[1].ToLower() == "plain")
                                {
                                    Writer = new PlainTextResultWriter(Console.Out, Console.Out, Console.Out);
                                    retstr = "Switched to plain output" + Environment.NewLine;
                                }
                                break;

                            default:
                                retstr = "Unrecognized command." + Environment.NewLine;
                                break;
                        }
                    }
                    else if (firstArg.Trim()[0] == '{')
                    {
                        retstr = eval.HandleCmd(cmd.Trim().Substring(1)) + Environment.NewLine;
                    }
                    else
                    {
                        if (db.ExecuteQuery(cmd, Writer) != 0)
                            Writer.WriteStd(db.LastError);
                    }

                    return retstr;
                }), prompt, startupMsg, tables.Concat(columns).ToList());
        }
    }
}
