﻿using System.Collections.Generic;
using System.Diagnostics;

namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata container for entities of an IfcSchema
/// </summary>
[DebuggerDisplay("{IfcClassName} ({ValidSchemaVersions})")]
public class IfcClassInformation
{
    /// <summary>
    /// relationXmlAttributeName of the attribute as a string, stored in PascalCase
    /// </summary>
    public string PascalCaseName { get; }

    /// <summary>
    /// relationXmlAttributeName of the attribute as a string, converted to UPPERCASE
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
        ValidSchemaVersions = IfcSchema.GetSchema(schemas);
    }
}
