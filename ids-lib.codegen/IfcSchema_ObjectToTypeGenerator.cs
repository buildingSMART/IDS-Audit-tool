using System.Diagnostics;
using System.Text;
using Xbim.Ifc2x3.SharedBldgElements;
using Xbim.Properties;

namespace IdsLib.codegen;

public class IfcSchema_ObjectToTypeGenerator
{
    private record TypeObjRel
    {
		public string ObjectName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
	}

    private static IEnumerable<TypeObjRel> GetTypeObjRels(string schema)
    {
		List<string> prevTypes = new List<string>();
		var mappings = File.ReadAllLines(@"buildingSMART\IFC_TYPES_MAPPING_BPS.csv");
		
		foreach (var mapping in mappings.Skip(1))
		{
			// IfcAirTerminal/DIFFUSER;IfcAirTerminalType/DIFFUSER;IFC4X3_ADD1;"4.3.1.0"
			var parts = mapping.Split(';');
			var schemaVersion = parts[2];
			if (schemaVersion != schema)
				continue;

			var objectName = parts[0];
			var predO = "";
			if (objectName.Contains('/'))
			{
				objectName = objectName.Substring(0, objectName.IndexOf("/"));
				predO = parts[0].Substring(parts[0].IndexOf("/"));
			}
			var typeName = parts[1];
			var predT = "";
			if (typeName.Contains('/'))
			{
				typeName = typeName.Substring(0, typeName.IndexOf("/"));
				predT = parts[1].Substring(parts[1].IndexOf("/"));
			}
			if (predT != predO)
			{
				throw new Exception("Unexpected scenario");
			}
			// check and return
			var thisT = $"{objectName}/{typeName}";
			if (prevTypes.Contains(thisT))
				continue;
			prevTypes.Add(thisT);
			yield return new TypeObjRel() { ObjectName = objectName.ToUpperInvariant(), TypeName = typeName.ToUpperInvariant() };
		}
	}


	/// <summary>
	/// We need the ability to move from an object to its type to expand the set of entities that we can attach to standard prop sets.
	/// </summary>
	public static string Execute()
    {
        List<string>vals = new List<string>();
        foreach (var item in vals)
        {
            Debug.WriteLine(item);
        }
		var source = stub;
		var genSchemas = new[] { "Ifc4", "Ifc4x3" };
        var sourceSchemas = new[] { "IFC4", "IFC4X3_ADD1" }; 
		
		for (int i = 0; i < genSchemas.Length; i++)
        {
			var schema = genSchemas[i];
			var sourceSchema = sourceSchemas[i];
			var sb = new StringBuilder();
			var rel = GetTypeObjRels(sourceSchema);
			foreach (var pair in rel)
			{
				sb.AppendLine($"\t\tschema.AddRelationType(\"{pair.ObjectName}\", \"{pair.TypeName}\");");
			}
			var replace = $"<PlaceHolder{schema}>\r\n";
			source = source.Replace(replace, sb.ToString());

		}
        source = source.Replace($"<PlaceHolderVersion>", VersionHelper.GetFileVersion(typeof(IfcWall)));
        return source;
    }

    private const string stub =
		"""
		// programmatically generated from ids-lib.codegen using Xbim <PlaceHolderVersion>
		using System.Collections.Generic;

		namespace IdsLib.IfcSchema;

		public partial class SchemaInfo
		{
			static partial void GetRelationTypesIFC4(SchemaInfo schema)
			{
		<PlaceHolderIfc4>
			}

			static partial void GetRelationTypesIFC4x3(SchemaInfo schema)
			{
		<PlaceHolderIfc4x3>
			}
		}
		""";

}