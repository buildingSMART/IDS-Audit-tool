using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common.Metadata;

namespace IdsLib.codegen
{
	internal class IfcSchema_DocumentationGenerator
	{
		internal static string Execute(Dictionary<string, typeMetadata> dataTypeDictionary)
		{
			var schemas = new string[] { "Ifc2x3", "Ifc4", "Ifc4x3" };

			StringBuilder sbDataTypes = new StringBuilder();
			foreach (var dataType in dataTypeDictionary.Values.OrderBy(x=>x.Name))
			{
				var checks = schemas.Select(x => dataType.Schemas.Contains(x) ? "✔️     " : "❌     ");
				sbDataTypes.AppendLine($"| {dataType.Name,-45} | {string.Join(" | ", checks),-24} | {dataType.XmlBackingType,-21} |");
			}


			StringBuilder sbXmlTypes = new StringBuilder();
			var xmlTypes = dataTypeDictionary.Values.Select(x => x.XmlBackingType).Where(str => !string.IsNullOrWhiteSpace(str)).Distinct();
			foreach (var dataType in xmlTypes.OrderBy(x => x))
			{
				sbXmlTypes.AppendLine($"| {dataType,-11} |");
			}

			var source = stub;
			source = source.Replace($"<PlaceHolderDataTypes>", sbDataTypes.ToString().TrimEnd('\r', '\n'));
			source = source.Replace($"<PlaceHolderXmlTypes>", sbXmlTypes.ToString().TrimEnd('\r', '\n'));
			return source;
			// Program.Message($"no change.", ConsoleColor.Green);
		}

		private const string stub = @"# Type constraining

## DataTypes

Property dataTypes can be set to any values according to the following table.

Columns of the table determine the validity of the type depending on the schema version and the required `xs:base` type for any `xs:restriction` constraint.

| dataType                                      | Ifc2x3 | Ifc4   | Ifc4x3 | Restriction base type |
| --------------------------------------------- | ------ | ------ | ------ | --------------------- |
<PlaceHolderDataTypes>

## XML base types

The list of valid XML base types for the `base` attribute of `xs:restriction` is:

| Base type   |
| ----------- |
<PlaceHolderXmlTypes>

## Notes

Please note, this document has been automatically generated via the IDS Audit Tool repository, any changes should be initiated there.
";

	}
}