using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SQLitePCL.pretty;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    public class PlainTextResultWriter : IResultWriter, IDisposable
    {
        public TextWriter stdOut { get; set; }
        public TextWriter VerboseOut { get; set; }
        public TextWriter ErrorOut { get; set; }

        public TextWriter WarningOut { get; set; }
       
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

        public PlainTextResultWriter(TextWriter std = null, TextWriter verbose = null, TextWriter e = null, TextWriter warning = null)
        {
            stdOut = std ?? TextWriter.Null;
            VerboseOut = verbose ?? TextWriter.Null;
            ErrorOut = e ?? TextWriter.Null;
            WarningOut = warning ?? TextWriter.Null;
            Delimiter = "|";
            ContextStack = new Stack<ResultContext>();
        }

        public void Flush()
        {
            stdOut.Flush();
            VerboseOut.Flush();
            ErrorOut.Flush();
        }

        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PlainTextResultWriter()
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
                stdOut.Dispose();
                VerboseOut.Dispose();
                ErrorOut.Dispose();
            }

            stdOut.Close();
            VerboseOut.Close();
            ErrorOut.Close();
            stdOut.Dispose();
            VerboseOut.Dispose();
            ErrorOut.Dispose();
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
            Write(GetResultAsDelimitedString(result), dest);
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

        public IResultWriter BeginContext(string context)
        {
            //if (CurrentContext != null && CurrentContext.ContextsWritten++ > 0)
            //IncreaseIndent();
            if (CurrentContext != null)
                ContextStack.Push(CurrentContext);
            CurrentContext = new ResultContext(context);
            stdOut.WriteLine(context);
            return this;
        }

        public IResultWriter EndContext()
        {
            if (ContextStack.Count > 0)
                CurrentContext = ContextStack.Pop();
            return this;
        }

        public IResultWriter WriteHeaders(List<string> headers)
        {
            stdOut.WriteLine(String.Join(Delimiter.ToString(), headers.ToArray()));
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
            return this;
        }

        public IResultWriter BeginOutput(string beginStr = "")
        {
            return this;
        }

        public IResultWriter EndOutput(string endStr = "")
        {
            return this;
        }
    }
}
