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
              HelpText = "Starts EvTLite in interactive mode")]
            public bool Interactive { get; set; }
        }
        

        static int Main(string[] args)
        {
            ConfigFile configFile = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText("config.json"));
            int exitCode = 0;
            try
            {
                if(args.Contains("--help") || args.Contains("--version")) return 0;
                bool interactive = false;
                var option = CommandLine.Parser.Default.ParseArguments<Options>(args);  //new Options();
                option
                    .MapResult(
                        options =>
                        {
                            interactive = options.Interactive;
                            return 0;
                        },
                        errors => { return 1; });
                if (interactive)
                {
                    Repl.Run(args, configFile);
                }
                else
                {
                    exitCode = Automator.Run(configFile);
                }
            }
            
            catch (Exception e) { }
            return exitCode;
        }
        
    }

}
