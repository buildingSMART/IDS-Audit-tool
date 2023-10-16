using System.Collections.Generic;
using System.Diagnostics;

namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata container for relations that are primarily one-to-many between IFC entities
/// </summary>
[DebuggerDisplay("{IfcName} ({ValidSchemaVersions})")]
public class PartOfRelationInformation
{
    /// <summary>
    /// Name of the IFC entity for the relation
    /// </summary>
    public string RelationIfcName { get; set; }
    /// <summary>
    /// Name of the IFC entity on the owner-side of the one-to-many relation
    /// </summary>
    public string OwnerIfcType { get; set; }
    /// <summary>
    /// Name of the IFC entity on the part-side of the one-to-many relation
    /// </summary>
    public string PartIfcType { get; set; } 
    /// <summary>
    /// Default constructor, ensures static nullable analysis
    /// </summary>
    public PartOfRelationInformation(string relationName, string oneSideType, string manySideType = "")
	{
        RelationIfcName = relationName;
        OwnerIfcType = oneSideType;
        PartIfcType = manySideType;
    }
}
