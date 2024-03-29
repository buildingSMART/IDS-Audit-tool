using FluentAssertions;
using IdsLib.IfcSchema;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace idsTool.tests;

public class IfcSchemaTests
{
    [Fact]
    public void CanGetAConcreteClass()
    {
        var wall = SchemaInfo.AllConcreteClasses.FirstOrDefault(x => x.PascalCaseName == "IfcWall");
        wall.Should().NotBeNull();
        wall!.ValidSchemaVersions.Should().NotBe(IfcSchemaVersions.IfcNoVersion);
        wall.ValidSchemaVersions.Should().Be(IfcSchemaVersions.IfcAllVersions);

        var element = SchemaInfo.AllConcreteClasses.FirstOrDefault(x => x.PascalCaseName == "IfcElement");
        element.Should().BeNull();
    }

    [Fact]
    public void CanGetConcreteClassProps()
    {
        var wall = SchemaInfo.SchemaIfc2x3["IFCWALL"];
        wall.Should().NotBeNull();
        wall!.FunctionalType.Should().Be(FunctionalType.ElementWithTypes);

        foreach (var schema in SchemaInfo.GetSchemas(IfcSchemaVersions.IfcAllVersions))
        {
            // Debug.WriteLine($"{schema.Version}");
            foreach (var item in schema)
            {
                if (item.FunctionalType == FunctionalType.ElementWithTypes)
                {
                    // Debug.WriteLine($"{item.Name} {string.Join(",", item.RelationTypeClasses!)}");
                }
            }
            // Debug.WriteLine($"");
        }        
	}

    [Fact]
	public void DebugTypeList()
	{
		foreach (var schema in SchemaInfo.GetSchemas(IfcSchemaVersions.IfcAllVersions))
		{
			Debug.WriteLine($"{schema.Version}");
			Debug.WriteLine($"");
		}
	}

	[Fact]
	public void CanBuildConcreteSubtree()
	{
		// Unrelated classes should not match
		var noMatch = new[] { "IFCWALL", "IFCSLAB" };
		var found = SchemaInfo.TrySearchTopClass(noMatch, IfcSchemaVersions.Ifc4, out _);
		found.Should().BeFalse();
	}


	[Fact]
	public void CanGetConcreteSubclasses()
	{
        // try the first set
		var elements = SchemaInfo.GetConcreteClassesFrom("IfcElement", IfcSchemaVersions.IfcAllVersions).Select(x=>x.ToUpperInvariant()).ToList();
        elements.Should().NotBeNull();
        var cnt = elements.Count();
        var dist = elements.Distinct();
        dist.Count().Should().Be(cnt);

        // try the reverse
        bool found = SchemaInfo.TrySearchTopClass(elements, IfcSchemaVersions.IfcAllVersions, out var topClass);
        found.Should().BeTrue();
        topClass.Should().NotBeNull();
        topClass.Should().Be("IfcElement");

        // add some other unrelated class and it should not match
        //
        var noMatch = elements.Concat(new[] { "IFCACTOR" });
		found = SchemaInfo.TrySearchTopClass(noMatch, IfcSchemaVersions.IfcAllVersions, out _);
        found.Should().BeFalse();

		// remove even one concrete and it should not match
		//
		noMatch = elements.Except(new[] { "IFCWALL" });
		found = SchemaInfo.TrySearchTopClass(noMatch, IfcSchemaVersions.IfcAllVersions, out _);
		found.Should().BeFalse();

        // Unrelated classes should not match
        noMatch = new[] { "IFCWALL", "IFCSLAB" };
		found = SchemaInfo.TrySearchTopClass(noMatch, IfcSchemaVersions.Ifc4, out _);
		found.Should().BeFalse();

		// now with a single schema
		elements = SchemaInfo.GetConcreteClassesFrom("IfcBuildingElement", IfcSchemaVersions.Ifc2x3).Select(x => x.ToUpperInvariant()).ToList();
		elements.Count().Should().BeLessThan(cnt);
		// let's try the reverse
		found = SchemaInfo.TrySearchTopClass(elements, IfcSchemaVersions.Ifc2x3, out topClass);
		found.Should().BeTrue();
		topClass.Should().NotBeNull();
		topClass.Should().Be("IfcBuildingElement");

        // 
		
	}

	[Theory]
    [InlineData("IFCOBJECTDEFINITION", 194,366)]
    [InlineData("IFCWALL",2, 3)]
    [InlineData("IFCNOTEXISTING",-1, -1)]
    public void GetSubClasses(string className, int minChildrenCount, int maxChildrenCount)
    {
        var schemas = SchemaInfo.GetSchemas(IfcSchemaVersions.IfcAllVersions);
        foreach (var schema in schemas)
        {
            var od = schema[className];
            if (minChildrenCount == -1 || maxChildrenCount == -1)
            {
                od.Should().BeNull();
            }
            else
            {
                od.Should().NotBeNull();
                od!.MatchingConcreteClasses.Count().Should().BeInRange(minChildrenCount, maxChildrenCount);
            }
        }
    }

    [Fact]
    public void CanIdentifySingleSchemas() 
    {
		IfcSchemaVersions.IfcAllVersions.IsSingleSchema().Should().BeFalse();
		IfcSchemaVersions.IfcNoVersion.IsSingleSchema().Should().BeFalse();
        IfcSchemaVersions.Ifc2x3.IsSingleSchema().Should().BeTrue();
        IfcSchemaVersions.Ifc4x3.IsSingleSchema().Should().BeTrue();
        var t = IfcSchemaVersions.Ifc4x3 | IfcSchemaVersions.Ifc2x3;
        t.IsSingleSchema().Should().BeFalse();
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
        foreach (var measure in IdsLib.IfcSchema.SchemaInfo.AllDataTypes)
        {
            var badcase = measure.IfcDataTypeClassName.Replace("IFC", "Ifc");
            // strict
            var res = SchemaInfo.TryParseIfcDataType(badcase, out _, true);
            res.Should().BeFalse($"{measure.IfcDataTypeClassName} is not capitalized");

			res = SchemaInfo.TryParseIfcDataType($"No{measure.IfcDataTypeClassName}", out _, true);
			res.Should().BeFalse("class does not exist");

            // tolerant 
			res = SchemaInfo.TryParseIfcDataType(badcase, out _, false);
            res.Should().BeTrue();

			res = SchemaInfo.TryParseIfcDataType($"No{measure.IfcDataTypeClassName}".ToUpperInvariant(), out _, false);
			res.Should().BeFalse("class does not exist");
		}
    }

    [Fact]
    public void CanGetTopLevelClassesByAttribute()
    {
        // Issue #17
        var allClasses = SchemaInfo.SchemaIfc4.GetAttributeClasses("Description");
        var topLevelClasses = SchemaInfo.SchemaIfc4.GetAttributeClasses("Description", onlyTopClasses: true);
        
        topLevelClasses.Should().BeSubsetOf(allClasses);

        topLevelClasses.Should().HaveCountLessThan(allClasses.Length);

    }

    [Fact]
    public void OnlyTopLevelClassesShouldRemoveAllSubClasses()
    {
        // Issue #20
        
        var topLevelClasses = SchemaInfo.SchemaIfc4.GetAttributeClasses("ObjectType", onlyTopClasses: true);

        topLevelClasses.Should().Contain("IFCOBJECT");

        topLevelClasses.Should().NotContain("IFCPRODUCT");

    }



}
