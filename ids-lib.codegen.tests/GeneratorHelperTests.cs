using AwesomeAssertions;
using IdsLib.codegen;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Ifc2x3;
using Xbim.Ifc4;
using Xbim.Ifc4x3;
using Xbim.IO.Xml.BsConf;

namespace ids_lib.codegen.tests
{
	public class GeneratorHelperTests
	{
		[Theory]
		[InlineData("Ifc4x3")]
		[InlineData("Ifc4")]
		[InlineData("Ifc2x3")]
		public void NoDuplicateEnumValues(string schema)
		{
			// some xbim schema present duplicate enum values, which causes problems for the code generator.
			// This test ensures that there are no duplicate enum values in the schema.
			//
			var t = IfcSchema_DatatypeNamesGenerator.GetEnumTypes(schema);
			var classNames = t.Select(x => x.Name).ToList();
			classNames.Should().NotBeEmpty();
			classNames.Should().OnlyHaveUniqueItems();
		}


		[Theory]
		[InlineData("Ifc4", "IfcRoot", "GlobalId", false, false)]
		[InlineData("Ifc4", "IfcRoot", "OwnerHistory", true, false)]
		[InlineData("Ifc2x3", "IfcColumnType", "HasPropertySets", false, true)]
		[InlineData("Ifc2x3", "IfcRelReferencedInSpatialStructure", "RelatedElements", true, true)]
		public void AttributeTypesAreAsExpected(string schema, string ifcType, string attributeName, bool isOptional, bool isEnumerable)
		{
			// setup
			IEntityFactory factory2x3 = new EntityFactoryIfc2x3();
			IEntityFactory factory4 = new EntityFactoryIfc4();
			IEntityFactory factory4x3 = new EntityFactoryIfc4x3Add2();

			var metadata = schema switch
			{
				"Ifc4" => ExpressMetaData.GetMetadata(factory4),
				"Ifc4x3" => ExpressMetaData.GetMetadata(factory4x3),
				"Ifc2x3" => ExpressMetaData.GetMetadata(factory2x3),
				_ => throw new NotImplementedException()
			};

			var rootType = metadata.ExpressType(ifcType.ToUpper());
			rootType.Should().NotBeNull();
			var prpGlobalId = rootType.Properties.Values.FirstOrDefault(x => x.Name == attributeName);
			prpGlobalId.Should().NotBeNull();

			var att = XbimHelper.GetExpressTypeDefinition(prpGlobalId);
			att.Should().NotBeNull();
			if (isOptional) 
				att.Should().Contain("OPTIONAL");
			else
				att.Should().NotContain("OPTIONAL");

			if (isEnumerable)
				att.Should().Contain(" OF ");
			else
				att.Should().NotContain(" OF ");
		}
	}
}
