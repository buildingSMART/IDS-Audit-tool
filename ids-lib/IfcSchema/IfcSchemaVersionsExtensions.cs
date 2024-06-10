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
    /// Canonical string version of the IFC2X3 schema version
    /// </summary>
    public const string IfcSchema2x3String = "IFC2X3";

	/// <summary>
	/// Canonical string version of the IFC4 schema version
	/// </summary>
	public const string IfcSchema4String = "IFC4";

	/// <summary>
	/// Canonical string version of the IFC4X3 schema version
	/// </summary>
	public const string IfcSchema4x3String = "IFC4X3_ADD2";

    /// <summary>
    /// Legacy string version of the IFC4X3 schema version
    /// </summary>
    public const string IfcSchema4x3StringLegacy = "IFC4X3";

    /// <summary>
    /// Converts a set of IFC schema name strings to the relative enum value
    /// </summary>
    /// <param name="schemaStrings">Enumerable strings to be evaluated</param>
    /// <returns>A single enumeration value representing all the <paramref name="schemaStrings"/></returns>
    public static IfcSchemaVersions GetSchema(IEnumerable<string> schemaStrings)
    {
        IfcSchemaVersions ret = IfcSchemaVersions.IfcNoVersion;
        foreach (var scheme in schemaStrings)
        {
            IfcSchemaVersions v = scheme.ToUpperInvariant() switch
            {
				IfcSchema2x3String => IfcSchemaVersions.Ifc2x3,
				IfcSchema4String => IfcSchemaVersions.Ifc4,
				IfcSchema4x3String => IfcSchemaVersions.Ifc4x3,
                IfcSchema4x3StringLegacy => IfcSchemaVersions.Ifc4x3,
                _ => IfcSchemaVersions.IfcNoVersion,
            };
            if (v == IfcSchemaVersions.IfcNoVersion)
                continue;
            ret |= v;
        }
        return ret;
    }

	/// <summary>
	/// Converts a set of IFC schema strings, concatenated in a single space separated string (useful for XML attribute reading).
	/// </summary>
	/// <param name="spaceSeparatedSchemaStrings">A single string possibly containing multiple space separated values to be evaluated</param>
	/// <returns>A single enumeration value representing all the <paramref name="spaceSeparatedSchemaStrings"/></returns>
	public static IfcSchemaVersions ParseXmlSchemasFromAttribute(this string spaceSeparatedSchemaStrings)
	{
		var tmp = spaceSeparatedSchemaStrings.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		return GetSchema(tmp);
	}

	/// <summary>
	/// Encodes a schemas enum to a string formatted as an enumerable XML attribute.
	/// </summary>
	/// <param name="versions">The versions enum to convert to XML attribute encoding</param>
	/// <returns>the encoded string</returns>
	public static string EncodeAsXmlSchemasAttribute(this IfcSchemaVersions versions)
	{
		List<string> schemas = new();
		if (versions.HasFlag(IfcSchemaVersions.Ifc2x3))
			schemas.Add(IfcSchema2x3String);
		if (versions.HasFlag(IfcSchemaVersions.Ifc4))
			schemas.Add(IfcSchema4String);
		if (versions.HasFlag(IfcSchemaVersions.Ifc4x3))
			schemas.Add(IfcSchema4x3String);
		return string.Join(" ", schemas);
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
		if (!memInfo.Any())
			return null;
		var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
		return attributes.Any() ? (T)attributes[0] : null;
	}
}