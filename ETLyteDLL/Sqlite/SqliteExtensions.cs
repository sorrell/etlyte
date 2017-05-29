using SQLitePCL.pretty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ETLyteDLL
{
    public static class SqliteExtensions
    {
        public static Func<ISQLiteValue, ISQLiteValue, ISQLiteValue> regexFunc =
            (ISQLiteValue val, ISQLiteValue regexStr) =>
            {
                if (Regex.IsMatch(Convert.ToString(val), Convert.ToString(regexStr)))
                    return true.ToSQLiteValue();
                return false.ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue, ISQLiteValue> dateFunc =
            (ISQLiteValue val, ISQLiteValue dateFmt) =>
            {
                DateTime currentval;
                string[] formats = Convert.ToString(dateFmt).Split('|');
                bool parsed = false;
                foreach (var format in formats)
                {
                    parsed = DateTime.TryParseExact(Convert.ToString(val),
                               format,
                               System.Globalization.CultureInfo.InvariantCulture,
                               System.Globalization.DateTimeStyles.None,
                               out currentval);
                    if (parsed) break;
                }
                return parsed.ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> boolFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                if ((dbval == "1") || (dbval == "0"))
                    return true.ToSQLiteValue();
                bool currentval;
                return Boolean.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue, ISQLiteValue> uriFunc =
            (ISQLiteValue val, ISQLiteValue uriType) =>
            {
                Uri outUri;
                var dbval = Convert.ToString(val);
                var uriTypeVals = Convert.ToString(uriType).Split('|');
                bool parsed = Uri.TryCreate(dbval, UriKind.RelativeOrAbsolute, out outUri);
                if (parsed && outUri.IsAbsoluteUri && !String.IsNullOrEmpty(uriTypeVals[0]))
                    foreach (var v in uriTypeVals)
                    {
                        parsed = (outUri.Scheme.ToLower() == v.ToLower());
                        if (parsed) break;
                    }
                return parsed.ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> intFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                int currentval;
                return int.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> uintFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                uint currentval;
                return uint.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> longFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                long currentval;
                return long.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> ulongFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                ulong currentval;
                return ulong.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> byteFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                byte currentval;
                return byte.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> sbyteFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                sbyte currentval;
                return sbyte.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> shortFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                short currentval;
                return short.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> ushortFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                ushort currentval;
                return ushort.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> floatFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                float currentval;
                return float.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> doubleFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                double currentval;
                return double.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> decimalFunc =
           (ISQLiteValue val) =>
           {
               var dbval = Convert.ToString(val);
               decimal currentval;
               return decimal.TryParse(dbval, out currentval).ToSQLiteValue();
           };

        public static Func<ISQLiteValue, ISQLiteValue> charFunc =
            (ISQLiteValue val) =>
            {
                var dbval = Convert.ToString(val);
                char currentval;
                return char.TryParse(dbval, out currentval).ToSQLiteValue();
            };

        public static Func<ISQLiteValue, ISQLiteValue> timespanFunc =
            (ISQLiteValue val) =>
            {
                // example: "P1Y2MT2H"
                var dbval = Convert.ToString(val);
                bool converted = true;
                TimeSpan currentval;
                try
                {
                    currentval = XmlConvert.ToTimeSpan(dbval);
                }
                catch (Exception e)
                {
                    converted = false;
                }
                return converted.ToSQLiteValue();
            };


        public static Func<ISQLiteValue, ISQLiteValue, ISQLiteValue, ISQLiteValue, ISQLiteValue> dateCompFunc =
            (ISQLiteValue val, ISQLiteValue dateFmt, ISQLiteValue operation, ISQLiteValue comp) =>
            {
                DateTime dbval = default(DateTime);
                DateTime compval = default(DateTime);
                string[] formats = Convert.ToString(dateFmt).Split('|');
                bool parsed = false;
                foreach (var format in formats)
                {
                    parsed = DateTime.TryParseExact(Convert.ToString(val),
                               format,
                               System.Globalization.CultureInfo.InvariantCulture,
                               System.Globalization.DateTimeStyles.None,
                               out dbval);
                    if (parsed) break;
                }
                if (parsed && dbval.Year > 1)
                {
                    bool parsedComp = false;
                    foreach (var format in formats)
                    {
                        parsedComp = DateTime.TryParseExact(Convert.ToString(comp),
                                   format,
                                   System.Globalization.CultureInfo.InvariantCulture,
                                   System.Globalization.DateTimeStyles.None,
                                   out compval);
                        if (parsedComp) break;
                    }
                    if (parsedComp && compval.Year > 1)
                    {
                        var op = Convert.ToString(operation);
                        if (op == ">") { parsed = (dbval > compval); }
                        else if (op == ">=") { parsed = (dbval >= compval); }
                        else if (op == "<") { parsed = (dbval < compval); }
                        else if (op == "<=") { parsed = (dbval <= compval); }
                        else if (op == "=") { parsed = (dbval == compval); }
                        else if (op == "<>") { parsed = (dbval != compval); }
                    }
                }

                return parsed.ToSQLiteValue();
            };

        // Comparison Functions
        public static Func<ISQLiteValue, ISQLiteValue, ISQLiteValue, ISQLiteValue, ISQLiteValue> compareFunc =
                        (ISQLiteValue a, ISQLiteValue comparator, ISQLiteValue b, ISQLiteValue colType) =>
                        {
                            bool isGreater = false;
                            var dbval = Convert.ToString(a);
                            var comp = Convert.ToString(comparator);
                            switch (colType.ToString())
                            {
                                case "float":
                                    float floatVal;
                                    isGreater = float.TryParse(dbval, out floatVal);
                                    isGreater &= Compare(comp, floatVal, b.ToFloat());
                                    break;
                                case "double":
                                    double doubleVal;
                                    isGreater = double.TryParse(dbval, out doubleVal);
                                    isGreater &= Compare(comp, doubleVal, b.ToDouble());
                                    break;
                                case "decimal":
                                    decimal decimalVal;
                                    isGreater = decimal.TryParse(dbval, out decimalVal);
                                    isGreater &= Compare(comp, decimalVal, b.ToDecimal());
                                    break;
                                case "int":
                                    int intVal;
                                    isGreater = int.TryParse(dbval, out intVal);
                                    isGreater &= Compare(comp, intVal, b.ToInt());
                                    break;
                                case "uint":
                                    uint uintVal;
                                    isGreater = uint.TryParse(dbval, out uintVal);
                                    isGreater &= Compare(comp, uintVal, b.ToUInt32());
                                    break;
                                case "byte":
                                    byte byteVal;
                                    isGreater = byte.TryParse(dbval, out byteVal);
                                    isGreater &= Compare(comp, byteVal, b.ToByte());
                                    break;
                                case "sbyte":
                                    sbyte sbyteVal;
                                    isGreater = sbyte.TryParse(dbval, out sbyteVal);
                                    isGreater &= Compare(comp, sbyteVal, b.ToSByte());
                                    break;
                                case "long":
                                    long longVal;
                                    isGreater = long.TryParse(dbval, out longVal);
                                    isGreater &= Compare(comp, longVal, b.ToInt64());
                                    break;
                                case "ulong":
                                    ulong ulongVal, ulongValB;
                                    isGreater = ulong.TryParse(dbval, out ulongVal);
                                    isGreater &= ulong.TryParse(Convert.ToString(b), out ulongValB);
                                    isGreater &= Compare(comp, ulongVal, ulongValB);
                                    break;
                                case "short":
                                    short shortVal;
                                    isGreater = short.TryParse(dbval, out shortVal);
                                    isGreater &= Compare(comp, shortVal, b.ToShort());
                                    break;
                                case "ushort":
                                    ushort ushortVal;
                                    isGreater = ushort.TryParse(dbval, out ushortVal);
                                    isGreater &= Compare(comp, ushortVal, b.ToUInt16());
                                    break;
                                default:
                                    break;
                            }
                            return isGreater.ToSQLiteValue();
                        };

        public static Func<ISQLiteValue> uuidFunc = () => { return System.Guid.NewGuid().ToSQLiteValue(); };

        public static Func<ISQLiteValue, ISQLiteValue> rowNumFunc = (ISQLiteValue key) => 
            {
                string k = Convert.ToString(key);
                int retVal = 0;
                if (RowNumDictionary.ContainsKey(k))
                    retVal = RowNumDictionary[k];
                RowNumDictionary[k] = retVal + 1;
                return (retVal + 1).ToSQLiteValue();
            };

        public static Dictionary<string, int> RowNumDictionary;


        // XML specific functions
        public static Func<ISQLiteValue, ISQLiteValue> nonPosIntFunc =
            (ISQLiteValue val) =>
            {
                return (longFunc(val).ToBool() && Convert.ToInt32(val) <= 0).ToSQLiteValue();
            };
        public static Func<ISQLiteValue, ISQLiteValue> nonNegIntFunc =
            (ISQLiteValue val) =>
            {
                return (longFunc(val).ToBool() && Convert.ToInt32(val) >= 0).ToSQLiteValue();
            };
        public static Func<ISQLiteValue, ISQLiteValue> posIntFunc =
            (ISQLiteValue val) =>
            {
                return (longFunc(val).ToBool() && Convert.ToInt32(val) > 0).ToSQLiteValue();
            };
        public static Func<ISQLiteValue, ISQLiteValue> negIntFunc =
            (ISQLiteValue val) =>
            {
                return (longFunc(val).ToBool() && Convert.ToInt32(val) < 0).ToSQLiteValue();
            };

        // Helper functions
        //private static bool IsFloat(ISQLiteValue val, out float outVal = 0)
        //{
        //    var dbval = Convert.ToString(val);
        //    float currentval;
        //    return float.TryParse(dbval, out currentval);
        //}

        //private static bool IsDouble(ISQLiteValue val)
        //{
        //    var dbval = Convert.ToString(val);
        //    double currentval;
        //    return double.TryParse(dbval, out currentval);
        //}

        private static bool Compare<T>(string comparator, T lhs, T rhs)
            where T : IComparable<T>
        {
            bool comp = false;
            switch (comparator)
            {
                case ">":
                    comp = lhs.CompareTo(rhs) > 0; 
                    break;
                case ">=":
                    comp = lhs.CompareTo(rhs) >= 0;
                    break;
                case "<":
                    comp = lhs.CompareTo(rhs) < 0;
                    break;
                case "<=":
                    comp = lhs.CompareTo(rhs) <= 0;
                    break;
                case "<>":
                    comp = lhs.CompareTo(rhs) != 0;
                    break;
                case "=":
                    comp = lhs.CompareTo(rhs) == 0;
                    break;
                default:
                    break;
            }
            return comp;
        }
      
    }
}
