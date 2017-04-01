using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class ResultContext
    {
        public int ResultsWritten { get; set; }
        public int ContextsWritten { get; set; }
        public string Name { get; set; }
        public ResultContext(string name, int contexts = 0)
        {
            ResultsWritten = 0;
            ContextsWritten = contexts;
            Name = name;
        }

    }
}
