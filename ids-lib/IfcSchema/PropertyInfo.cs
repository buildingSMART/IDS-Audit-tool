using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace IdsLib.IfcSchema;

/// <summary>
/// Generalised metadata on IFC properties
/// </summary>
public interface IPropertyTypeInfo
{
    /// <summary>
    /// Property name
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Property descriptive definition.
    /// </summary>
    public string Definition { get; set; }
}

/// <summary>
/// Static class to contain extension method helpers for <see cref="IPropertyTypeInfo"/>.
/// </summary>
public static class IPropertyTypeInfoExtensions
{
    /// <summary>
    /// Extension method to determine if the property constrains a specific data type in the IFC schema
    /// </summary>
    /// <param name="property">The property to be evaluated</param>
    /// <param name="dataType">a nullable string if there's no constraint or the string name of the IFC class constraint</param>
    /// <returns>true if a type constraint is enforced</returns>
    public static bool HasDataType(this IPropertyTypeInfo property, [NotNullWhen(true)] out string? dataType)
    {
        if (property is not SingleValuePropertyType svp)
        {
            dataType = null;
            return false;
        }
        dataType = svp.DataType;
        return true;
    }
}

/// <summary>
/// Schema metadata for properties with name
/// </summary>
public class NamedPropertyType : IPropertyTypeInfo
{
    /// <inheritdoc />
    public string Name { get; }
    /// <inheritdoc />
    public string Definition { get; set; } = string.Empty;

    /// <summary>
    /// Minimal public constructor
    /// </summary>
    public NamedPropertyType(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Schema metadata for enumeration properties
/// </summary>
public class EnumerationPropertyType : NamedPropertyType
{
    /// <summary>
    /// Possible enumeration values
    /// </summary>
    public IList<string> EnumerationValues { get; }

    /// <summary>
    /// Minimal public constructor
    /// </summary>
    public EnumerationPropertyType(string name, IEnumerable<string> values) : base(name)
    {
        EnumerationValues = values.ToList();
    }
}

/// <summary>
/// Schema metadata for single value properties
/// </summary>
public class SingleValuePropertyType : NamedPropertyType
{
    /// <summary>
    /// minimal constructor
    /// </summary>
    public SingleValuePropertyType(string name, string dataType) : base(name)
    {
        DataType = dataType;
    }

    /// <summary>
    /// The base IFC datatype of the property in the schema
    /// </summary>
    public string DataType { get; }
}
