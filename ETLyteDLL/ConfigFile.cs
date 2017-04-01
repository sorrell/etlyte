using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public class ConfigFile
    {
        public DbOptions Db { get; set; }
        public ExtractOptions Extract { get; set; }
        public ValidateOptions Validate { get; set; }
        public TransformOptions Transform { get; set; }
        public LoadOptions Load { get; set; }
        public StepsOptions Steps { get; set; }
    }

    public class ExtractOptions
    {
        [DefaultValue("Flatfiles")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Source { get; set; }

        [DefaultValue("Schemas")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Schemas { get; set; }

        [DefaultValue("SeedData")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string SeedData { get; set; }

        [DefaultValue(Globals.Delimiter)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Delimiter { get; set; }

        [DefaultValue(Globals.Quote)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Quote { get; set; }

        [DefaultValue(Globals.LineTerminator)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LineTerminator { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IgnoreTooManyColumns { get; set; }


        public MissingColumns MissingColumns { get; set; }

        public ExtractOptions()
        {
        }
    }

    public class ValidateOptions
    {
        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ValidationSource { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool FirstErrorOnly { get; set; }

        [DefaultValue(Int32.MaxValue)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int ErrorLimit { get; set; }

        public ConfigOutputs Outputs { get; set; }
        public SchemaErrorSettings SchemaErrorSettings { get; set; }
        public ValidateOptions() { }
    }

    public class TransformOptions
    {
        [DefaultValue("Transformations")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string TransformationSource { get; set; }        

        public ConfigOutputs Outputs { get; set; }
    }
    public class LoadOptions
    {
        [DefaultValue("Loads")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LoadSource { get; set; }

        public ConfigOutputs Outputs { get; set; }
    }
    public class StepsOptions
    {
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Extract { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Validate { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Transform { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Load { get; set; }

        public StepsOptions() { }
    }

    public class DbOptions
    {
        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LogFile { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool StopOnError { get; set; }

        [DefaultValue(Globals.DbName)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string DbName { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseExistingDb { get; set; }
    }
    public class ConfigOutputs
    {
        public ConfigOutputs() { }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string StandardOutputType { get; set; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string StandardOutputDelimiter { get; set; }

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string StandardOutputConnectionString { get; set; }

        [DefaultValue("Errors.log")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ErrorOutputConnectionString { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Verbose { get; set; }

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string VerboseOutputConnectionString { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Warnings { get; set; }

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string WarningsOutputConnectionString { get; set; }

    }

    public class MissingColumns
    {
        [DefaultValue("error")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string MissingColumnHandling { get; set; }

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string MissingColumnFillValue { get; set; }

        public MissingColumns() { }
    }
}
