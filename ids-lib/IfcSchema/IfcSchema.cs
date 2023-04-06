using System;
using System.Collections.Generic;

namespace IdsLib.IfcSchema;

[Flags]
public enum IfcSchemaVersions
{
    [IfcSchema(false)]
    IfcNoVersion = 0,
    [IfcSchema(true)]
    Ifc2x3 = 1 << 0,
    [IfcSchema(true)]
    Ifc4 = 1 << 1,
    [IfcSchema(true)]
    Ifc4x3 = 1 << 2,
    [IfcSchema(false)]
    IfcAllVersions = (1 << 3) - 1
}

[AttributeUsage(AttributeTargets.Field)]
public class IfcSchemaAttribute : Attribute
{
    public bool IsSpecificAttribute = false;
    public IfcSchemaAttribute(bool isSpecific)
    {
        IsSpecificAttribute = isSpecific;
    }
}

internal static class IfcSchema
{
    internal static IfcSchemaVersions GetSchema(IEnumerable<string> schemas)
    {
        IfcSchemaVersions ret = IfcSchemaVersions.IfcNoVersion;
        foreach (var scheme in schemas)
        {
            IfcSchemaVersions v = scheme switch
            {
                "Ifc2x3" => IfcSchemaVersions.Ifc2x3,
                "Ifc4" => IfcSchemaVersions.Ifc4,
                "Ifc4x3" => IfcSchemaVersions.Ifc4x3,
                _ => IfcSchemaVersions.IfcNoVersion,
            };
            if (v == IfcSchemaVersions.IfcNoVersion)
                continue;
            ret |= v;
        }
        return ret;
    }
}
