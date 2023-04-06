using System.Collections.Generic;
using System.Diagnostics;

namespace IdsLib.IfcSchema;

[DebuggerDisplay("{IfcMeasureClassName} ({ValidSchemaVersions})")]
public class IfcMeasureInformation
{
    public string IfcMeasureClassName { get; set; } = string.Empty;
    public IfcSchemaVersions ValidSchemaVersions { get; set; } = IfcSchemaVersions.IfcNoVersion;
    public IfcMeasureInformation(string name, IEnumerable<string> schemas)
    {
        IfcMeasureClassName = name;
        ValidSchemaVersions = IfcSchema.GetSchema(schemas);
    }
}
