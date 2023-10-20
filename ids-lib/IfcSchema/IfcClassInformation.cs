using System.Collections.Generic;
using System.Diagnostics;

namespace IdsLib.IfcSchema;

/// <summary>
/// Simplistic metadata container for entities of an IfcSchema
/// </summary>
[DebuggerDisplay("{IfcClassName} ({ValidSchemaVersions})")]
public class IfcClassInformation
{
    /// <summary>
    /// Name of the class as a string, stored in PascalCase
    /// </summary>
    public string PascalCaseName { get; }

    /// <summary>
    /// Name of the class as a string, converted to UPPERCASE
    /// </summary>
    public string UpperCaseName => PascalCaseName.ToUpperInvariant();

    /// <summary>
    /// Versions of the schema that contain the class
    /// </summary>
    public IfcSchemaVersions ValidSchemaVersions { get; }

    /// <summary>
    /// Default constructor, ensures static nullable analysis
    /// </summary>
    public IfcClassInformation(string nameInPascalCase, IEnumerable<string> schemas)
    {
        PascalCaseName = nameInPascalCase;
        ValidSchemaVersions = IfcSchemaVersionsExtensions.GetSchema(schemas);
    }
}
