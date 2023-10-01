using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace IdsLib.IfcSchema;

/// <summary>
/// Enumerations for the identification of multiple schema versions.
/// </summary>
[Flags]
public enum IfcSchemaVersions
{
    /// <summary>
    /// Matches no schema
    /// </summary>
    [IfcSchema(false)]
    IfcNoVersion = 0,
    /// <summary>
    /// Matches includes version Ifc2x3
    /// </summary>
    [IfcSchema(true)]
    Ifc2x3 = 1 << 0,
    /// <summary>
    /// Matches includes version Ifc4
    /// </summary>
    [IfcSchema(true)]
    Ifc4 = 1 << 1,
    /// <summary>
    /// Matches includes version Ifc4x3
    /// </summary>
    [IfcSchema(true)]
    Ifc4x3 = 1 << 2,
    /// <summary>
    /// Matches includes all valid Ifc versions
    /// </summary>
    [IfcSchema(false)]
    IfcAllVersions = (1 << 3) - 1
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

    internal static bool TryGetSchemaInformation(this IfcSchemaVersions schemas, out IEnumerable<SchemaInfo> schemaInfo)
    {
		return SchemaInfo.TryGetSchemaInformation(schemas, out schemaInfo);
	}

}
