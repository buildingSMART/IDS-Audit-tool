using FluentAssertions;
using IdsLib.IfcSchema;
using System.Linq;
using Xunit;

namespace idsTool.tests;

public class IfcSchemaTests
{
    [Fact]
    public void CanGetConcreteClasses()
    {
        var root = SchemaInfo.AllClasses.FirstOrDefault(x => x.PascalCaseName == "IfcWall");
        root.Should().NotBeNull();
        root!.ValidSchemaVersions.Should().NotBe(IfcSchemaVersions.IfcNoVersion);
        root.ValidSchemaVersions.Should().Be(IfcSchemaVersions.IfcAllVersions);
    }

    [Theory]
    [InlineData("IFCOBJECTDEFINITION", 194,366)]
    [InlineData("IFCWALL",2, 3)]
    public void GetSubClasses(string className, int minChildrenCount, int maxChildrenCount)
    {
        var schemas = SchemaInfo.GetSchemas(IfcSchemaVersions.IfcAllVersions);
        foreach (var schema in schemas)
        {
            var od = schema[className];
            od.Should().NotBeNull();
            od!.MatchingConcreteClasses.Count().Should().BeInRange(minChildrenCount, maxChildrenCount);
        }
    }

    [Fact]
    public void HasPropertySets()
    {
        var schemas = SchemaInfo.GetSchemas(IfcSchemaVersions.IfcAllVersions);
        foreach (var schema in schemas)
        {
            schema.PropertySets.Should().NotBeNull();
            // Pset_ActionRequest is in all schemas
            var psetar = schema.PropertySets.FirstOrDefault(x => x.Name == "Pset_ActionRequest");
            psetar.Should().NotBeNull();
        }
    }

    [Fact]
    public void CanParseMeasure()
    {
        foreach (var measure in IdsLib.IfcSchema.SchemaInfo.AllMeasures)
        {
            // srtict
            var res = SchemaInfo.TryParseIfcMeasure(measure.IfcMeasureClassName, out _, true);
            res.Should().BeFalse($"{measure.IfcMeasureClassName} is not capitalized");

			res = SchemaInfo.TryParseIfcMeasure($"No{measure.IfcMeasureClassName}", out _, true);
			res.Should().BeFalse("class does not exist");

            // tolerant 
			res = SchemaInfo.TryParseIfcMeasure(measure.IfcMeasureClassName.ToUpperInvariant(), out _, false);
            res.Should().BeTrue();

			res = SchemaInfo.TryParseIfcMeasure($"No{measure.IfcMeasureClassName}".ToUpperInvariant(), out _, false);
			res.Should().BeFalse("class does not exist");
		}
    }


}
