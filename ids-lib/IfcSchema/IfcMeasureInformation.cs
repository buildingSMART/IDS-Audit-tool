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
    /// Comma separated value of the basic unit exponents
    /// </summary>
    public string DimensionalExponents { get; } = string.Empty;
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
    /// <summary>
    /// Custom constructor for unit exponents
    /// </summary>
    /// <param name="name"></param>
    /// <param name="schemas"></param>
    /// <param name="exponents"></param>
    public IfcMeasureInformation(string name, IEnumerable<string> schemas, string exponents)
    {
        IfcMeasureClassName = name;
        ValidSchemaVersions = IfcSchemaVersionsExtensions.GetSchema(schemas);
        DimensionalExponents = exponents;
    }
}
