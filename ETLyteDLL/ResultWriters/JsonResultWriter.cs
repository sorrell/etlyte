using Newtonsoft.Json;
using SQLitePCL.pretty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class JsonResultWriter : IResultWriter, IDisposable
    {
        public TextWriter stdOut { get; set; }
        public TextWriter VerboseOut { get; set; }
        public TextWriter ErrorOut { get; set; }
        public TextWriter WarningOut { get; set; }
        public string Key { get; set; }
        public string ResultMode { get; }
        private int ContextsWritten { get; set; }
        Formatting JsonFormatting { get; set; }
        public Stack<ResultContext> ContextStack { get; set; }
        public ResultContext CurrentContext { get; set; }

        public JsonResultWriter(TextWriter std = null, TextWriter verbose = null, TextWriter e = null, TextWriter warning = null, string formatting = "")
        {
            stdOut = std ?? TextWriter.Null;
            VerboseOut = verbose ?? TextWriter.Null;
            ErrorOut = e ?? TextWriter.Null;
            WarningOut = warning ?? TextWriter.Null;
            Key = "";
            JsonFormatting = String.IsNullOrWhiteSpace(formatting) ? Formatting.None : Formatting.Indented;
            ResultMode = "json";
            ContextStack = new Stack<ResultContext>();
        }

        public IResultWriter WriteResult(IReadOnlyList<IResultSetValue> result, Globals.ResultWriterDestination dest)
        {
            if (CurrentContext.ResultsWritten++ > 0)
                Write("," + Environment.NewLine, dest);
            Write(CurrentContext.ResultsWritten.ToString().DoubleQuote() + ":" + GetResultWithKeyAsJsonObject(result), dest);
            return this;
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
        public IResultWriter WriteStd(string line)
        {
            stdOut.WriteLine(line);
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

        public IResultWriter WriteWarning(string line)
        {
            WarningOut.WriteLine(line);
            return this;
        }
        public void Flush()
        {
            stdOut.Flush();
            VerboseOut.Flush();
            ErrorOut.Flush();
            WarningOut.Flush();
        }

        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~JsonResultWriter()
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
                WarningOut.Close();
                stdOut.Dispose();
                VerboseOut.Dispose();
                ErrorOut.Dispose();
                WarningOut.Dispose();
            }

            _disposed = true;
        }

        private Dictionary<string, string> GetResultAsDictionary(IReadOnlyList<IResultSetValue> row)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            row.Aggregate(result, (a, b) => { a.Add(b.ColumnInfo.Name, b.ToString()); return a; });
            return result;
        }

        private string GetResultWithKeyAsJsonObject(IReadOnlyList<IResultSetValue> row)
        {
            string str = string.Empty;
            if (!String.IsNullOrWhiteSpace(Key))
            {
                Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
                var dict = GetResultAsDictionary(row);
                if (dict.ContainsKey(Key))
                {
                    var val = dict[Key];
                    dict.Remove(Key);
                    result.Add(val, dict);
                    str = JsonConvert.SerializeObject(result, JsonFormatting);
                }
                else
                    str = GetResultAsJsonObject(row);
            }
            else
                str = GetResultAsJsonObject(row);

            //reset Key
            Key = String.Empty;
            return str;
        }

        private string GetResultAsJsonObject(IReadOnlyList<IResultSetValue> row)
        {
            return JsonConvert.SerializeObject(GetResultAsDictionary(row), JsonFormatting);
        }

        public IResultWriter BeginContext(string context, Globals.ResultWriterDestination dest)
        {
            if (CurrentContext != null)
            {
                if (CurrentContext.ContextsWritten++ > 0)
                {
                    stdOut.Write("," + Environment.NewLine);
                }
                ContextStack.Push(CurrentContext);
                CurrentContext = new ResultContext(context);
            }
            else
                CurrentContext = new ResultContext(context, 1);
            Write(context.DoubleQuote() + ": {", dest);
            VerboseOut.WriteLine("Beginning context " + CurrentContext.Name);
            return this;
        }

        public IResultWriter EndContext(Globals.ResultWriterDestination dest = Globals.ResultWriterDestination.stdOut)
        {
            Write("}",dest);
            stdOut.Flush();
            VerboseOut.Flush();
            if (ContextStack.Count > 0)
                CurrentContext = ContextStack.Pop();
            return this;
        }

        public IResultWriter Reset()
        {
            ContextsWritten = 0;
            return this;
        }
        public IResultWriter WriteHeaders(List<string> headers, Globals.ResultWriterDestination dest)
        {
            return this;
        }

        public IResultWriter BeginOutput(string beginStr = "")
        {
            stdOut.WriteLine("{");
            VerboseOut.WriteLine("Beginning output");
            return this;
        }

        public IResultWriter EndOutput(string endStr = "")
        {
            stdOut.WriteLine("}");
            VerboseOut.WriteLine("Ending output");
            stdOut.Flush();
            VerboseOut.Flush();
            return this;
        }
        //public IResultWriter BeginSubsection(string beginText)
        //{
        //    if (SubsectionsWritten++ > 0)
        //        stdOut.Write("," + Environment.NewLine);
        //    stdOut.WriteLine(beginText.Quote() + ": " + "{ " + Environment.NewLine);
        //    return this;
        //}
        //public IResultWriter EndSubsection(string endText = "")
        //{
        //    stdOut.WriteLine("}");
        //    return this;
        //}
    }
}
