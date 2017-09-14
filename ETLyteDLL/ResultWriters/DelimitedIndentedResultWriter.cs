using System;
using System.Collections.Generic;
using System.IO;
using SQLitePCL.pretty;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class DelimitedIndentedResultWriter : IResultWriter, IDisposable
    {
        public TextWriter stdOut { get; set; }
        public TextWriter VerboseOut { get; set; }
        public TextWriter ErrorOut { get; set; }
        public TextWriter WarningOut { get; set; }
        
        public int IndentationLevel { get; set; }
        public string Delimiter { get; set; }

        public string ResultMode { get; }

        public string Key
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        public Stack<ResultContext> ContextStack { get; set; }
        public ResultContext CurrentContext { get; set; }

        public DelimitedIndentedResultWriter(string delimiter = Globals.Delimiter, TextWriter std = null, TextWriter verbose = null, TextWriter e = null, TextWriter warning = null)
        {
            stdOut = std ?? TextWriter.Null;
            VerboseOut = verbose ?? TextWriter.Null;
            ErrorOut = e ?? TextWriter.Null;
            WarningOut = warning ?? TextWriter.Null;
            IndentationLevel = 0;
            Delimiter = delimiter;
            ContextStack = new Stack<ResultContext>();
            ResultMode = "delimited";
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

        ~DelimitedIndentedResultWriter()
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

            // release any unmanaged objects
            // set the object references to null

            _disposed = true;
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
        public IResultWriter WriteWarning(string line)
        {
            WarningOut.WriteLine(line);
            return this;
        }
        public IResultWriter WriteResult(IReadOnlyList<IResultSetValue> result, Globals.ResultWriterDestination dest)
        {
            Write(string.Concat(Enumerable.Repeat(Delimiter, IndentationLevel)) + GetResultAsDelimitedString(result), dest);
            return this;
        }

        public IResultWriter WriteStd(string line)
        {
            stdOut.WriteLine(line);
            return this;
        }

        public IResultWriter WriteVerbose(string line)
        {
            VerboseOut.WriteLine(string.Concat(Enumerable.Repeat(Delimiter, IndentationLevel)) + line);
            return this;
        }

        public IResultWriter WriteError(string line)
        {
            ErrorOut.WriteLine(line);
            return this;
        }

        public IResultWriter BeginContext(string context, Globals.ResultWriterDestination dest)
        {
            //if (CurrentContext != null && CurrentContext.ContextsWritten++ > 0)
            //IncreaseIndent();
            if (CurrentContext != null)
                ContextStack.Push(CurrentContext);
            CurrentContext = new ResultContext(context);
            Write(string.Concat(Enumerable.Repeat(Delimiter, IndentationLevel)) + context, dest);
            return this.IncreaseIndent();
        }

        public IResultWriter EndContext(Globals.ResultWriterDestination dest = Globals.ResultWriterDestination.stdOut)
        {
            stdOut.Flush();
            VerboseOut.Flush();
            if (ContextStack.Count > 0)
                CurrentContext = ContextStack.Pop();
            return this.DecreaseIndent();
        }

        public IResultWriter WriteHeaders(List<string> headers, Globals.ResultWriterDestination dest)
        {
            Write(string.Concat(Enumerable.Repeat(Delimiter, IndentationLevel)) + String.Join(Delimiter.ToString(), headers.ToArray()), dest);
            return this;
        }

        private string GetResultAsDelimitedString(IReadOnlyList<IResultSetValue> stmt, string quotefield = "\"")
        {
            return string.Join(
                Delimiter.ToString(),
                stmt.Select(a => a.ToString().Contains(Delimiter)
                                ? quotefield + a.ToString() + quotefield
                                : a.ToString())
                    .ToArray()
            );
        }

        public IResultWriter IncreaseIndent(int num = 1)
        {
            IndentationLevel += num;
            return this;
        }
        public IResultWriter DecreaseIndent(int num = 1)
        {
            IndentationLevel -= num;
            return this;
        }

        //public IResultWriter BeginSubsection(string beginText)
        //{
        //    stdOut.WriteLine(beginText);
        //    return this.IncreaseIndent();
        //}
        //public IResultWriter EndSubsection(string endText = "")
        //{
        //    return this.DecreaseIndent();
        //}

        public IResultWriter Reset()
        {
            IndentationLevel = 0;
            return this;
        }

        public IResultWriter BeginOutput(string beginStr = "")
        {
            return this;
        }

        public IResultWriter EndOutput(string endStr = "")
        {
            stdOut.Flush();
            VerboseOut.Flush();
            WarningOut.Flush();
            return this;
        }
    }
}
