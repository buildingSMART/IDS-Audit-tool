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


