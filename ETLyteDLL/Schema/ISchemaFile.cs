using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLyteDLL
{
    public interface ISchemaFile
    {
        bool Header { get; set; }
        char Delimiter { get; set; }
        string Name { get; set; }
        string Flatfile { get; set; }
        List<SchemaField> Fields { get; set; }
    }
}
