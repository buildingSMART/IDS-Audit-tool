using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IfcSchema;

/// <summary>
/// Provides utility methods for the <see cref="IfcSchemaVersions"/> enum.
/// </summary>
public static class IfcSchemaVersionsExtensions
{
    /// <summary>
    /// Converts a set of IFC schema name strings to the relative enum value
    /// </summary>
    /// <param name="schemasStrings">Enumerable strings to be evaluated</param>
    /// <returns>A single enumeration value representing all the <paramref name="schemasStrings"/></returns>
    public static IfcSchemaVersions GetSchema(IEnumerable<string> schemasStrings)
    {
        IfcSchemaVersions ret = IfcSchemaVersions.IfcNoVersion;
        foreach (var scheme in schemasStrings)
        {
            IfcSchemaVersions v = scheme switch
            {
                "Ifc2x3" => IfcSchemaVersions.Ifc2x3,
                "Ifc4" => IfcSchemaVersions.Ifc4,
                "Ifc4x3" => IfcSchemaVersions.Ifc4x3,
                _ => IfcSchemaVersions.IfcNoVersion,
            };
            if (v == IfcSchemaVersions.IfcNoVersion)
                continue;
            ret |= v;
        }
        return ret;
    }

    /// <summary>
    /// Makes it easy to use the <see cref="SchemaInfo.TryGetSchemaInformation(IfcSchemaVersions, out IEnumerable{SchemaInfo})"/> directly from a schema enum.
    /// </summary>
    /// <param name="schemas">the enum to evaluate</param>
    /// <param name="schemaInfo">the identified schema infos</param>
    /// <returns>true if the values are matched</returns>
	public static bool TryGetSchemaInformation(this IfcSchemaVersions schemas, out IEnumerable<SchemaInfo> schemaInfo)
    {
		return SchemaInfo.TryGetSchemaInformation(schemas, out schemaInfo);
	}


    /// <summary>
    /// Utility extension to help identify if the flag enum <see cref="IfcSchemaVersions"/> represents a single schema value.
    /// </summary>
    /// <param name="schemaEnum">the enum to evaluate</param>
    /// <returns>true if the enum represents a single version of the schema, false otherwise</returns>
	public static bool IsSingleSchema(this IfcSchemaVersions schemaEnum)
	{
        var att = schemaEnum.GetAttributeOfType<IfcSchemaAttribute>();
        if (att == null) 
            return false;
        return att.IsSpecificAttribute;
	}
}

internal static class EnumHelper
{
	/// <summary>
	/// Gets an attribute on an enum field value
	/// </summary>
	/// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
	/// <param name="enumVal">The enum value</param>
	/// <returns>The attribute of type T that exists on the enum value</returns>
	/// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
	public static T? GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
	{
		var type = enumVal.GetType();
		var memInfo = type.GetMember(enumVal.ToString());
		var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
		return attributes.Any() ? (T)attributes[0] : null;
	}
}