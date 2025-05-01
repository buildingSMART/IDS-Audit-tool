using System.Collections.Generic;
using System.Diagnostics;

namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata container for entities containing measures of an IfcSchema.
/// Access the list from <see cref="SchemaInfo.AllDataTypes"/>.
/// </summary>
[DebuggerDisplay("{IfcDataTypeClassName} ({ValidSchemaVersions})")]
public class IfcDataTypeInformation
{
    /// <summary>
    /// Name of the entity as a string, stored in UPPERCASE
    /// </summary>
    public string IfcDataTypeClassName { get; }    
    /// <summary>
    /// Metadata for unit of measure conversion, if relevant.
    /// </summary>
    public IfcMeasureInformation Measure { get; } = IfcMeasureInformation.Empty;
    /// <summary>
    /// The XML type backing the datatype, if known.
    /// </summary>
	public string? BackingType { get; } = null;
	/// <summary>
	/// Versions of the schema that contain the class
	/// </summary>
	public IfcSchemaVersions ValidSchemaVersions { get; }
    /// <summary>
    /// Default constructor, ensures static nullable analysis
    /// </summary>
    public IfcDataTypeInformation(string name, IEnumerable<string> schemas, string type = "")
    {
        IfcDataTypeClassName = name;
        ValidSchemaVersions = IfcSchemaVersionsExtensions.GetSchema(schemas);
        if (!string.IsNullOrEmpty(type))
            BackingType = type;
    }
    
	/// <summary>
	/// Custom constructor for unit conversion
	/// </summary>
	public IfcDataTypeInformation(string name, IEnumerable<string> schemas, IfcMeasureInformation ifcMeasureInformation, string type = "") : this(name, schemas, type)
	{
        Measure = ifcMeasureInformation;
	}
}
