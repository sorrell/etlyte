using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ETLyteDLL;
using Newtonsoft.Json;
using System.Data.SqlClient;
using CommandLine;
using CommandLine.Text;
using Cintio;

namespace ETLyteExe
{
    class Program
    {
        class Options
        {
            [Option('i', "interactive", Default = false,
              HelpText = "Starts ETLyte in interactive mode")]
            public bool Interactive { get; set; }

            [Option('c', "command", Default = "",
                HelpText = "Command to execute against database")]
            public string Command { get; set; }

            [Option('f', "file", Default = "",
                HelpText = "File to execute")]
            public string File { get; set; }
        }
        

        static int Main(string[] args)
        {
            ConfigFile configFile = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText("config.json"));
            int exitCode = 0;
            try
            {
                //if(args.Contains("--help") || args.Contains("--version")) return 0;
                bool interactive = false;
                string cmd = "";
                string filename = "";
                var option = CommandLine.Parser.Default.ParseArguments<Options>(args);  //new Options();
                option
                    .MapResult(
                        options =>
                        {
                            interactive = options.Interactive;
                            cmd = options.Command;
                            filename = options.File;
                            return 0;
                        },
                        errors => { return 1; });
                if (interactive)
                {
                    Repl.Run(args, configFile);
                }
                else
                {
                    exitCode = Automator.Run(configFile, cmd, filename);
                }
            }
            
            catch (Exception e) { }
            return exitCode;
        }
        
    }

}
