using SQLitePCL.pretty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public interface IResultWriter : IDisposable
    {
        //IOutputInterface stdOut { get; set; }
        //IOutputInterface ErrorOut { get; set; }
        //IOutputInterface VerboseOut { get; set; }
        IResultWriter WriteResult(IReadOnlyList<IResultSetValue> result, Globals.ResultWriterDestination dest);
        IResultWriter WriteStd(string result);

        IResultWriter WriteVerbose(string line);

        IResultWriter WriteError(string line);
        IResultWriter WriteWarning(string line);
        IResultWriter Write(string line, Globals.ResultWriterDestination destination);
        IResultWriter BeginOutput(string beginStr);
        IResultWriter EndOutput(string endStr);
        IResultWriter BeginContext(string context);
        IResultWriter EndContext();
        IResultWriter WriteHeaders(List<string> headers);
        IResultWriter Reset();
        string ResultMode { get; }
        string Key { get; set; }
        Stack<ResultContext> ContextStack { get; set; }
        ResultContext CurrentContext { get; set; }
		void Flush();
    
	
    }
}
