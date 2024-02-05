using System.Collections.Generic;
using System.Diagnostics;

namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata container for entities containing measures of an IfcSchema
/// </summary>
[DebuggerDisplay("{IfcMeasureClassName} ({ValidSchemaVersions})")]
public class IfcMeasureInformation
{
    /// <summary>
    /// Name of the entity as a string, stored in UPPERCASE
    /// </summary>
    public string IfcMeasureClassName { get; }
    /// <summary>
    /// Versions of the schema that contain the class
    /// </summary>
    public IfcSchemaVersions ValidSchemaVersions { get; }
    /// <summary>
    /// Default constructor, ensures static nullable analysis
    /// </summary>
    public IfcMeasureInformation(string name, IEnumerable<string> schemas)
    {
        IfcMeasureClassName = name;
        ValidSchemaVersions = IfcSchemaVersionsExtensions.GetSchema(schemas);
    }
}
