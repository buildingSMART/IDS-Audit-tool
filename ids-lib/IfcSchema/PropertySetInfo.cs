using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IfcSchema;

/// <summary>
/// Information about standard property sets defined from bS
/// </summary>
public partial class PropertySetInfo
{
    /// <summary>
    /// The stanard name of the IfcPropertySet being described.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// string names of the properties defined for the IfcPropertySet being described.
    /// </summary>
    public IEnumerable<string> PropertyNames => Properties.Select(p => p.Name);

    /// <summary>
    /// Valid schema classes that may be attached to the property set.
    /// </summary>
    public IList<string> ApplicableClasses { get; set; }

    /// <summary>
    /// The properties defined for the set.
    /// </summary>
    public IList<IPropertyTypeInfo> Properties { get; set; }

    /// <summary>
    /// Returns one of the defined properties in the set by name, if found
    /// </summary>
    /// <param name="name">the expected name, case sensitive</param>
    /// <returns>null if not found, otherwise information about the property</returns>
    public IPropertyTypeInfo? GetProperty(string name) => Properties.FirstOrDefault(p => p.Name == name);

    /// <summary>
    /// basic constructor
    /// </summary>
    public PropertySetInfo(
    string propertySetName,
    IEnumerable<IPropertyTypeInfo> properties,
    IEnumerable<string> applicableClasses
    )
    {
        Name = propertySetName;
        Properties = properties.ToList();
        ApplicableClasses = applicableClasses.ToList();
    }

    private static IList<PropertySetInfo>? schemaIFC4;
    /// <summary>
    /// static method to get the known metadata for the IFC4 schema
    /// </summary>
    public static IList<PropertySetInfo> SchemaIfc4
    {
        get
        {
            schemaIFC4 ??= GetPropertiesIFC4().ToList();
            return schemaIFC4;
        }
    }

    private static IList<PropertySetInfo>? schemaIFC4x3;
    /// <summary>
    /// static method to get the known metadata for the IFC4 schema
    /// </summary>
    public static IList<PropertySetInfo> SchemaIfc4x3
    {
        get
        {
            schemaIFC4x3 ??= GetPropertiesIFC4x3().ToList();
            return schemaIFC4x3;
        }
    }

    /// <summary>
    /// Resolve a property from its name within a set, given an schema.
    /// Both property set name and property name are case sensitive matches.
    /// </summary>
    /// <returns>Property metadata if exact match or null.</returns>
    public static IPropertyTypeInfo? Get(IfcSchemaVersions version, string propertySetName, string propertyName)
    {
        IList<PropertySetInfo>? schema = GetSchema(version);
        if (schema == null)
            return null;
        var set = schema.Where(x => x.Name == propertySetName).FirstOrDefault();
        if (set is null)
            return null;
        return set.GetProperty(propertyName);
    }

    /// <summary>
    /// Schema metadata from the enum
    /// </summary>
    /// <returns>null if the schema has no metadata in the current implementation</returns>
    public static IList<PropertySetInfo>? GetSchema(IfcSchemaVersions version)
    {
        IList<PropertySetInfo>? schema = version switch
        {
            IfcSchemaVersions.Ifc2x3 => SchemaIfc2x3,
            IfcSchemaVersions.Ifc4 => SchemaIfc4,
            IfcSchemaVersions.Ifc4x3 => SchemaIfc4x3,
            _ => null,
        };
        return schema;
    }

    private static IList<PropertySetInfo>? schemaIFC2x3;

    /// <summary>
    /// static method to get the known metadata for the IFC2x3 schema
    /// </summary>
    public static IList<PropertySetInfo> SchemaIfc2x3
    {
        get
        {
            schemaIFC2x3 ??= GetPropertiesIFC2x3().ToList();
            return schemaIFC2x3;
        }
    }
}