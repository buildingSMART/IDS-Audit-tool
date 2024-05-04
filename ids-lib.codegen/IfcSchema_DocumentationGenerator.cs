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
			StringBuilder sb = new StringBuilder();
			var schemas = new string[] { "Ifc2x3", "Ifc4", "Ifc4x3" };

			foreach (var dataType in dataTypeDictionary.Values.OrderBy(x=>x.Name))
			{
				var checks = schemas.Select(x => dataType.Schemas.Contains(x) ? "✔️     " : "❌     ");

				sb.AppendLine($"| {dataType.Name,-45} | {string.Join(" | ", checks),-24} | {dataType.XmlBackingType,-21} |");
			}
			
			var source = stub;
			source = source.Replace($"<PlaceHolderTable>", sb.ToString());
			return source;
			// Program.Message($"no change.", ConsoleColor.Green);
		}

		private const string stub = @"# DataTypes

Property dataTypes can be set to any values according to the following table.

Columns of the table determine the validity of the type depending on the schema version and the required `xs:base` type for any `xs:restriction` constraint.

| dataType                                      | Ifc2x3 | Ifc4   | Ifc4x3 | Restriction base type |
| --------------------------------------------- | ------ | ------ | ------ | --------------------- |
<PlaceHolderTable>

Please note, this document has been automatically generated via the IDS Audit Tool repository, any changes should be initiated there.
";

	}
}