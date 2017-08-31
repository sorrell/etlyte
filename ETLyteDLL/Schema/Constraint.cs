using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class Constraints
    {
        public Constraints()
        {
            Minimum = Double.NaN;
            MinLength = -1;
            Maximum = Double.NaN;
            MaxLength = -1;
            DatePattern = "yyyyMMdd";
        }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public bool Required { get; set; }
        public bool Unique { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string Pattern { get; set; }
        public string DatePattern { get; set; }
        public string UriType { get; set; }
    }
}
