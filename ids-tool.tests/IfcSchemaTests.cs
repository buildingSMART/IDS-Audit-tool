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
}
