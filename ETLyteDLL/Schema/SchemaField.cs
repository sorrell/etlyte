using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ETLyteDLL
{
    public class SchemaField
    {
        private DataType datatype;
        private Type type = typeof(string);

        public SchemaField(string name)
        {
            Name = name;
            Minimum = Double.NaN;
            MinLength = -1;
            Maximum = Double.NaN;
            MaxLength = -1;
            DatePattern = "yyyyMMdd";
            ColumnType = ColumnType.Normal;
        }
        public string Name { get; set; }
        public bool Required { get; set; }
        public bool Unique { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string Pattern { get; set; }
        public string DatePattern { get; set; }
        public string UriType { get; set; }
        public ColumnType ColumnType { get; set; }
        public string Derivation { get; set; }
        public Type Type { get { return type; } set { type = value; } }
        public DataType DataType
        {
            get { return datatype; }
            set
            {
                datatype = value;
                switch (value.ToString())
                {
                    case "string":
                        Type = typeof(string);
                        break;
                    case "byte":
                        Type = typeof(byte);
                        break;
                    case "sbyte":
                        Type = typeof(sbyte);
                        break;
                    case "short":
                        Type = typeof(short);
                        break;
                    case "ushort":
                        Type = typeof(ushort);
                        break;
                    case "int":
                        Type = typeof(int);
                        break;
                    case "uint":
                        Type = typeof(uint);
                        break;
                    case "long":
                        Type = typeof(long);
                        break;
                    case "ulong":
                        Type = typeof(ulong);
                        break;
                    case "DateTime":
                        Type = typeof(DateTime);
                        break;
                    case "float":
                        Type = typeof(float);
                        break;
                    case "double":
                        Type = typeof(double);
                        break;
                    case "decimal":
                        Type = typeof(decimal);
                        break;
                    case "Boolean":
                        Type = typeof(bool);
                        break;
                    case "char":
                        Type = typeof(char);
                        break;
                    case "TimeSpan":
                        Type = typeof(TimeSpan);
                        break;
                    case "URI":
                        Type = typeof(Uri);
                        break;
                    default:
                        Type = typeof(string);
                        break;
                }
            }
        }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
    }

    public enum ColumnType
    {
        Normal,
        Derived
    }
    public enum DataType
    {
        @string      = 0,
        @byte,
        @sbyte,
        @short,
        @ushort,
        @int,
        @uint,
        @long,
        @ulong,
        @float,
        @double,
        @decimal,
        Boolean,
        DateTime,
        @object,
        @char,
        @TimeSpan,
        URI
    }
}
