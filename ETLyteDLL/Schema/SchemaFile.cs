using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class SchemaFile : ISchemaFile
    {
        public SchemaFile(bool header = false, char delimiter = ',', string name = "", string flatfile = "", int skipRows = 0, bool summarizeResults = true)
        {
            Header = header;
            Delimiter = delimiter;
            Name = name;
            Flatfile = flatfile;
            SkipRows = skipRows;
            SummarizeResults = summarizeResults;
        }
        public bool Header { get; set; }
        public char Delimiter { get; set; }
        public string Name { get; set; }
        public string Flatfile { get; set; }
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int SkipRows { get; set; }
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool SummarizeResults { get; set; }
        public List<SchemaField> Fields { get; set; }

    }
}
