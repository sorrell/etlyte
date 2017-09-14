using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class SchemaErrorSettings
    {
        public SchemaErrorSettings()
        {
            DatatypeErrorLevel =
                RequiredErrorLevel =
                MinimumErrorLevel =
                MaximumErrorLevel =
                MinLengthErrorLevel =
                MaxLengthErrorLevel =
                UniqueErrorLevel =
                PatternErrorLevel = "Error";
            MalformedHeaderErrorLevel = "Warning";
            ErrorOnUnrequiredWithBadDatatype = false;
        }
        public string DatatypeErrorLevel { get; set; }
        public string RequiredErrorLevel { get; set; }
        public string MinimumErrorLevel { get; set; }
        public string MaximumErrorLevel { get; set; }
        public string MinLengthErrorLevel { get; set; }
        public string MaxLengthErrorLevel { get; set; }
        public string UniqueErrorLevel { get; set; }
        public string PatternErrorLevel { get; set; }
        public string MalformedHeaderErrorLevel { get; set; }
        public bool ErrorOnUnrequiredWithBadDatatype { get; set; }
    }
}
