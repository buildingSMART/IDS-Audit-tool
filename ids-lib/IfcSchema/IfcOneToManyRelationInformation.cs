using System.Collections.Generic;
using System.Diagnostics;

namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata container for relations that are primarily one-to-many between IFC entities
/// </summary>
[DebuggerDisplay("{IfcName} ({ValidSchemaVersions})")]
public class IfcOneToManyRelationInformation
{
    /// <summary>
    /// relationXmlAttributeName of the IFC entity for the relation
    /// </summary>
    public string IfcName { get; set; }
    /// <summary>
    /// relationXmlAttributeName of the IFC entity on the one-side of the one-to-many relation
    /// </summary>
    public string OneSideIfcType { get; set; }
    /// <summary>
    /// relationXmlAttributeName of the IFC entity on the many-side of the one-to-many relation
    /// </summary>
    public string ManySideIfcType { get; set; } 
    /// <summary>
    /// Schema version in which the relation exists
    /// </summary>
    public IfcSchemaVersions ValidSchemaVersions { get; set; }
    /// <summary>
    /// Default constructor, ensures static nullable analysis
    /// </summary>
    public IfcOneToManyRelationInformation(string name, IEnumerable<string> schemas, string oneSideType, string manySideType = "")
    {
        IfcName = name;
        ValidSchemaVersions = IfcSchema.GetSchema(schemas);
        OneSideIfcType = oneSideType;
        ManySideIfcType = manySideType;
    }
}
