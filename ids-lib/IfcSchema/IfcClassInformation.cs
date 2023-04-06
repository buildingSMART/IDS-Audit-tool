using System.Collections.Generic;
using System.Diagnostics;

namespace IdsLib.IfcSchema;

[DebuggerDisplay("{IfcClassName} ({ValidSchemaVersions})")]
public class IfcClassInformation
{
    public string IfcClassName { get; set; } = string.Empty;
    public IfcSchemaVersions ValidSchemaVersions { get; set; } = IfcSchemaVersions.IfcNoVersion;
    public IfcClassInformation(string name, IEnumerable<string> schemas)
    {
        IfcClassName = name;
        ValidSchemaVersions = IfcSchema.GetSchema(schemas);
    }
}
