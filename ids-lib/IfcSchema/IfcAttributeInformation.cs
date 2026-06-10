using System.Collections.Generic;

namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata container for the association of attributes to entities in the IfcSchema, identified by their name
/// </summary>
public class IfcAttributeInformation
{
    /// <summary>
    /// Name of the attribute as a string
    /// </summary>
    public string IfcAttributeName { get; }
    /// <summary>
    /// Schemas in which the attribute can be found.
    /// </summary>
    public IfcSchemaVersions ValidSchemaVersions { get; } = IfcSchemaVersions.IfcNoVersion;

    /// <summary>
    /// Default constructor, ensures static nullable analysis
    /// </summary>
    public IfcAttributeInformation(string name, IEnumerable<string> schemas)
    {
        IfcAttributeName = name;
        ValidSchemaVersions = IfcSchemaVersionsExtensions.GetSchema(schemas);
    }
}
