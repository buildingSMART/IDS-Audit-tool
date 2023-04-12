using System.Collections.Generic;
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
