using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace IdsLib.IfcSchema;

/// <summary>
/// Enumerations for the identification of multiple schema versions
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
    /// Matches version Ifc2x3
    /// </summary>
    [IfcSchema(true)]
    Ifc2x3 = 1 << 0,
    /// <summary>
    /// Matches version Ifc4
    /// </summary>
    [IfcSchema(true)]
    Ifc4 = 1 << 1,
    /// <summary>
    /// Matches version Ifc4x3
    /// </summary>
    [IfcSchema(true)]
    Ifc4x3 = 1 << 2,
    /// <summary>
    /// Matches includes all valid Ifc versions
    /// </summary>
    [IfcSchema(false)]
    IfcAllVersions = (1 << 3) - 1
}

// todo: we are still determining if it's useful to release this as a public API

/// <summary>
/// Enumerations for the identification of a single schema version, used for cases where multiple versions are not expected or allowed.
/// </summary>
internal enum IfcSingleSchemaVersion : int
{
	/// <summary>
	/// Identifies version Ifc2x3
	/// </summary>
	Ifc2x3 = IfcSchemaVersions.Ifc2x3,
	/// <summary>
	/// Identifies version Ifc4
	/// </summary>
	Ifc4 = IfcSchemaVersions.Ifc4,
	/// <summary>
	/// Identifies version Ifc4x3
	/// </summary>
	Ifc4x3 = IfcSchemaVersions.Ifc4x3
}