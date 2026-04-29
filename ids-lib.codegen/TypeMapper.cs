using Xbim.Common.Metadata;
using Xbim.Properties;

namespace IdsLib.codegen;

internal class TypeMapper
{
	internal string IdsName { get; }
	internal ExpressType IfcMapToExpressType { get; }

	public TypeMapper(string unmappedName, ExpressMetaData metaD)
	{
		IdsName = unmappedName;
		if (!metaD.TryGetExpressType(unmappedName.ToUpperInvariant(), out var expressType))
			throw new Exception($"Could not find express type for {unmappedName} in schema.");
		IfcMapToExpressType = expressType;
	}

	public TypeMapper(string idsName, string ifcName, ExpressMetaData metaD)
	{
		IdsName = idsName;
		if (!metaD.TryGetExpressType(ifcName.ToUpperInvariant(), out var expressType))
			throw new Exception($"Could not find express type for {ifcName} in schema.");
		IfcMapToExpressType = expressType;
	}

	internal static List<TypeMapper> GetFor(string schema, List<IfcSchema_Ifc2x3MapperGenerator.Ifc2x3EntityMappingInformation> maps, out ExpressMetaData metaData)
	{
		var factory = SchemaHelper.GetFactory(schema);
		var metaD = ExpressMetaData.GetMetadata(factory);
		List<TypeMapper> tpNames = metaD.Types().Select(x => new TypeMapper(x.Name, metaD)).ToList();

		// special mapping case for Ifc2x3, to include the mapped names
		// see https://github.com/buildingSMART/IDS/blob/development/Documentation/ImplementersDocumentation/ifc2x3-occurrence-type-mapping-table.md
		if (schema == "Ifc2x3")
		{
			tpNames.AddRange(maps.Select(x => new TypeMapper(x.IdsEntity, x.IfcEntity, metaD)));
		}
		metaData = metaD;
		return tpNames;
	}
}

