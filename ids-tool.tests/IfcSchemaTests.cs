using FluentAssertions;
using IdsLib.IfcSchema;
using System.Linq;
using Xunit;

namespace idsTool.tests;

public class IfcSchemaTests
{
    [Fact]
    public void CanGetClasses()
    {
        var root = SchemaInfo.AllClasses.FirstOrDefault(x => x.PascalCaseName == "IfcRoot");
        root.Should().NotBeNull();
        root!.ValidSchemaVersions.Should().NotBe(IfcSchemaVersions.IfcNoVersion);
        root.ValidSchemaVersions.Should().Be(IfcSchemaVersions.IfcAllVersions);
    }
}
