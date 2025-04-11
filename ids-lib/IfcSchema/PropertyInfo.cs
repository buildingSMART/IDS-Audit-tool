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
    public static bool HasDataTypes(this IPropertyTypeInfo property, out IEnumerable<string> dataType)
    {
		switch (property)
		{
			case SingleValuePropertyType svp:
                dataType = [svp.DataType.ToUpperInvariant()];
				return true;
			case EnumerationPropertyType ep:
				// We assume that enumerations are stored as labels, having had a look at a few example on bSmart
				dataType = ["IFCLABEL"];
				return true;
			case TableValuePropertyType tvp:
				dataType = [tvp.DataType1.ToUpperInvariant(), tvp.DataType2.ToUpperInvariant()];
				return true;
		}
        dataType = Enumerable.Empty<string>();
        return false;
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
public class TableValuePropertyType : NamedPropertyType
{
	/// <summary>
	/// minimal constructor
	/// </summary>
	public TableValuePropertyType(string name, string dataType1, string dataType2) : base(name)
	{
		DataType1 = dataType1;
		DataType2 = dataType2;
	}

	/// <summary>
	/// The base IFC datatype of the DefiningValue in the schema
	/// </summary>
	public string DataType1 { get; }

	/// <summary>
	/// The base IFC datatype of the DefinedValue in the schema
	/// </summary>
	public string DataType2 { get; }
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
