using System;

namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata attribute to define if a value of <see cref="IfcSchemaVersions"/> identifies a single version of the schema
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class IfcSchemaAttribute : Attribute
{
    /// <summary>
    /// Attribute decorator to define if a value of <see cref="IfcSchemaVersions"/> identifies a single version of the schema
    /// </summary>
    public bool IsSpecificAttribute = false;
    /// <summary>
    /// default constructor
    /// </summary>
    /// <param name="isSpecific">assigns the <see cref="IsSpecificAttribute"/> value</param>
    public IfcSchemaAttribute(bool isSpecific)
    {
        IsSpecificAttribute = isSpecific;
    }
}
