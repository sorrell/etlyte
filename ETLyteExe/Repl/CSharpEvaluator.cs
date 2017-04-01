using Mono.CSharp;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ETLyteExe
{
    public class CSharpEvaluator
    {
        Evaluator evaluator;
        CustomTextWriter c;
        string lastCmd;
        public CSharpEvaluator()
        {
            c = new CustomTextWriter();
            c.InputErrorOnUsing += c_InputError;
            ReportPrinter r = new ConsoleReportPrinter(c);
            evaluator = new Evaluator(new CompilerContext(
                                new CompilerSettings(),
                                r));
            evaluator.ReferenceAssembly(Assembly.GetExecutingAssembly());
            evaluator.Run("using System;");
            evaluator.Run("using ETLyteExe;");
        }
        
        void c_InputError(object sender, EventArgs e)
        {
            try
            {
                InputErrorEventArgs ev = (InputErrorEventArgs)e;
                string regexStr = "missing `(.*)'";
                Match match = Regex.Match(ev.error, regexStr, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string key = match.Groups[1].Value;
                    evaluator.Run("using " + key + ";");
                    c.WriteLine("Automatically using " + key + ".  Try rerunning your last command.");

                }
            }
            catch (Exception ex)
            {
                c.WriteLine("Unable to automatically add missing namespace due to error: " + ((InputErrorEventArgs)e).error);
            }
        }
        public string HandleCmd(string input)
        {
            var output = "";
            if (input.EndsWith(";"))
            {
                try
                {
                    evaluator.Run(input);
                }
                catch (Exception e)
                {
                    c.WriteLine(e.Message);
                }
            }
            else
            {
                try
                {
                    object result;
                    bool res_set;
                    string s = evaluator.Evaluate(input, out result, out res_set);
                    if (res_set)
                        output = result.ToString();
                }
                catch (Exception e)
                {
                    c.WriteLine(e.Message);
                }
            }
            return output;
        }
    }
}
