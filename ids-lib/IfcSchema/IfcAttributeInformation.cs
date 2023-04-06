using System.Collections.Generic;

namespace IdsLib.IfcSchema;

public class IfcAttributeInformation
{
    public string IfcAttributeName { get; set; }
    public IfcSchemaVersions ValidSchemaVersions { get; set; } = IfcSchemaVersions.IfcNoVersion;

    public IfcAttributeInformation(string name, IEnumerable<string> schemas)
    {
        IfcAttributeName = name;
        ValidSchemaVersions = IfcSchema.GetSchema(schemas);
    }
}
