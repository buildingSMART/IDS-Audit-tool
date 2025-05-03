using FluentAssertions;
using IdsLib.IfcSchema;
using idsTool.tests.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xbim.Ifc.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests;

public class IfcSchemaTests
{

	public IfcSchemaTests(ITestOutputHelper outputHelper)
	{
		XunitOutputHelper = outputHelper;
	}
	private ITestOutputHelper XunitOutputHelper { get; }

	[Fact]
	public void CanGetAConcreteClass()
	{
		var wall = SchemaInfo.AllConcreteClasses.FirstOrDefault(static x => x.PascalCaseName == "IfcWall");
		wall.Should().NotBeNull();
		wall!.ValidSchemaVersions.Should().NotBe(IfcSchemaVersions.IfcNoVersion);
		wall.ValidSchemaVersions.Should().Be(IfcSchemaVersions.IfcAllVersions);

		var element = SchemaInfo.AllConcreteClasses.FirstOrDefault(static x => x.PascalCaseName == "IfcElement");
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
		var elements = SchemaInfo.GetConcreteClassesFrom("IfcElement", IfcSchemaVersions.IfcAllVersions).Select(static x => x.ToUpperInvariant()).ToList();
		elements.Should().NotBeNull();
		var cnt = elements.Count;
		var dist = elements.Distinct();
		dist.Count().Should().Be(cnt);

		// try the reverse
		bool found = SchemaInfo.TrySearchTopClass(elements, IfcSchemaVersions.IfcAllVersions, out var topClass);
		found.Should().BeTrue();
		topClass.Should().NotBeNull();
		topClass.Should().Be("IfcElement");

		// add some other unrelated class and it should not match
		//
		var noMatch = elements.Concat(["IFCACTOR"]);
		found = SchemaInfo.TrySearchTopClass(noMatch, IfcSchemaVersions.IfcAllVersions, out _);
		found.Should().BeFalse();

		// remove even one concrete and it should not match
		//
		noMatch = elements.Except(["IFCWALL"]);
		found = SchemaInfo.TrySearchTopClass(noMatch, IfcSchemaVersions.IfcAllVersions, out _);
		found.Should().BeFalse();

		// Unrelated classes should not match
		noMatch = ["IFCWALL", "IFCSLAB"];
		found = SchemaInfo.TrySearchTopClass(noMatch, IfcSchemaVersions.Ifc4, out _);
		found.Should().BeFalse();

		// now with a single schema
		elements = SchemaInfo.GetConcreteClassesFrom("IfcBuildingElement", IfcSchemaVersions.Ifc2x3).Select(static x => x.ToUpperInvariant()).ToList();
		elements.Count.Should().BeLessThan(cnt);
		// let's try the reverse
		found = SchemaInfo.TrySearchTopClass(elements, IfcSchemaVersions.Ifc2x3, out topClass);
		found.Should().BeTrue();
		topClass.Should().NotBeNull();
		topClass.Should().Be("IfcBuildingElement");

		// check that the passed schema is used
		var wind = SchemaInfo.GetConcreteClassesFrom("IfcWindow", IfcSchemaVersions.Ifc4).Select(static x => x.ToUpperInvariant()).ToList();
		wind.Count.Should().Be(2);
		wind = SchemaInfo.GetConcreteClassesFrom("IfcWindow", IfcSchemaVersions.Ifc2x3).Select(static x => x.ToUpperInvariant()).ToList();
		wind.Count.Should().Be(1);
		wind = SchemaInfo.GetConcreteClassesFrom("IfcWindow", IfcSchemaVersions.Ifc4x3).Select(static x => x.ToUpperInvariant()).ToList();
		wind.Count.Should().Be(1);

		var elements2x3 = SchemaInfo.GetConcreteClassesFrom("IfcBuiltElement", IfcSchemaVersions.Ifc2x3).Select(static x => x.ToUpperInvariant()).ToList();
		elements2x3.Should().HaveCount(0);
		var elements4x3 = SchemaInfo.GetConcreteClassesFrom("IfcBuiltElement", IfcSchemaVersions.Ifc4x3).Select(static x => x.ToUpperInvariant()).ToList();
		elements4x3.Should().HaveCount(36);


	}

	[Fact]
	public void CanGetConcreteSubclassesForSpecificSchema()
	{
		// IFCCABLECARRIERSEGMENT is new in IFC4
		var elements = SchemaInfo.GetConcreteClassesFrom("IFCCABLECARRIERSEGMENT", IfcSchemaVersions.Ifc4);
		elements.Should().NotBeNull();
		elements.Should().HaveCount(1);

		elements = SchemaInfo.GetConcreteClassesFrom("IFCFACILITYPART", IfcSchemaVersions.Ifc4x3);
		elements.Should().Contain("IfcRailwayPart");
		elements.Should().HaveCount(5);
	}

	[Theory]
	[InlineData("IFCOBJECTDEFINITION", 194, 366)]
	[InlineData("IFCWALL", 2, 3)]
	[InlineData("IFCNOTEXISTING", -1, -1)]
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
			var psetar = schema.PropertySets.FirstOrDefault(static x => x.Name == "Pset_ActionRequest");
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
	public void HasMeasureInfo()
	{
		var m = SchemaInfo.AllMeasureInformation.ToList();
		m.Should().HaveCount(83);
		foreach (var measure in m)
		{
			XunitOutputHelper.WriteLine($"{measure.Description}:\t{measure.Unit}\t{measure.UnitSymbol}\t{measure.DefaultDisplay}");
		}
	}

	[Fact]
	public void HasSiFundamentalsMeasureInfo()
	{
		var t = Enum.GetNames<DimensionType>().Should().HaveCount(7);
		foreach (var item in Enum.GetValues<DimensionType>())
		{
			var exp = DimensionalExponents.GetUnit(item);
			exp.Should().NotBeNull();
			exp.IsBasicUnit.Should().BeTrue($"{exp}");
		}
		var m = SchemaInfo.AllMeasureInformation.Where(static x => x.IsBasicUnit).ToList();
		m.Should().HaveCount(7);
		foreach (var measure in m)
		{
			XunitOutputHelper.WriteLine($"{measure.Description}:\t{measure.Unit}\t{measure.UnitSymbol}\t{measure.DefaultDisplay}");
		}
	}

	[Fact]
	public void CanNavigateUnitsUp()
	{
		IfcConversionUnitInformation.TryGetUnit("mile", out var unit).Should().BeTrue();
		unit.Should().NotBeNull();
		var parent = unit.GetParentUnit();
		parent.Should().NotBeNull();
	}

	[Fact]
	public void ConversionInformationMeasuresExist()
	{
		var missingMeasure = new List<string>();
		foreach (var conv in SchemaInfo.StandardConversionUnits)
		{
			if (!SchemaInfo.AllMeasureInformation.Where(x => x.IfcMeasure == conv.IfcMeasure).Any())
				missingMeasure.Add(conv.IfcMeasure);
		}
		missingMeasure = missingMeasure.Distinct().ToList();
		if (missingMeasure.Any())
			XunitOutputHelper.WriteLine($"Missing measures: {string.Join(", ", missingMeasure)}");
		missingMeasure.Should().BeEmpty();
	}

	[Fact]
	public void ConversionInformationIsCoherent()
	{
		var missingUnits = new List<string>();
		var invalidMeasures = new List<string>();
		var invalidTopSteps = new List<string>();
		foreach (var conv in SchemaInfo.StandardConversionUnits)
		{
			if (conv.GetParentUnit() == null)
				missingUnits.Add(conv.BaseUnit);
			var steps = conv.GetConversionSteps();
			foreach (var step in steps)
			{
				if (step.IfcMeasure != conv.IfcMeasure)
					invalidMeasures.Add($"{conv}: expected {conv.IfcMeasure}, parent {step.IfcMeasure}");
			}
			var topStep = steps.Last();
			if (topStep is not IfcMeasureInformation)
			{
				invalidTopSteps.Add($"{conv}: topstep is {topStep.GetType().Name}");
			}

		}
		missingUnits = missingUnits.Distinct().ToList();

		// feedback
		if (missingUnits.Any())
			XunitOutputHelper.WriteLine($"Missing conversion units base: {string.Join(", ", missingUnits)}");
		if (invalidMeasures.Any())
			XunitOutputHelper.WriteLine($"Invalid conversion measures: {string.Join(", ", invalidMeasures)}");
		if (invalidTopSteps.Any())
			XunitOutputHelper.WriteLine($"Invalid top steps: {string.Join(", ", invalidTopSteps)}");

		// assertions
		missingUnits.Should().BeEmpty();
		invalidMeasures.Should().BeEmpty();
	}

	[Fact]
	public void HasMeasureSiNames()
	{
		var logger = LoggerAndAuditHelpers.GetXunitLogger(XunitOutputHelper);
		var measures = SchemaInfo.AllMeasureInformation.ToList();
		var siunits = measures.Where(x => x.HasSiUnitEnum()).SelectMany(x => x.SiUnitNameEnums).ToList();
		logger.LogInformation($"Available si enum units: {string.Join(", ", siunits)}");
		var missingUnits = new List<string>();
		foreach (var unitName in Enum.GetValues<Xbim.Ifc4.Interfaces.IfcSIUnitName>())
		{
			var measurefound = measures.Where(x => x.HasSiUnitEnum(unitName.ToString())).ToList();
			if (!measurefound.Any())
			{
				missingUnits.Add(unitName.ToString());
			}
		}
		missingUnits.Should().HaveCount(0, $"no SI unit name should be missing");

		// we can use this to fix the units markdown, if needed
		var sb = new StringBuilder();
		foreach (var measure in measures)
		{
			sb.AppendLine($"{measure.Id} | {string.Join(", ", measure.SiUnitNameEnums)} |");
		}
		var tot = sb.ToString();
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

	[Fact]
	public void AllSubClassesOfTypesAreIncludedAsPossibleTypesForPropertySets()
	{
		var psets = new[] { "Pset_ManufacturerTypeInformation" };
		var classes = SchemaInfo.PossibleTypesForPropertySets(IfcSchemaVersions.Ifc2x3, psets).Select(static e => e.ToUpperInvariant());

		// Sanity checking
		classes.Should().Contain("IFCWALL");
		classes.Should().Contain("IFCWALLTYPE");
		classes.Should().Contain("IFCDISTRIBUTIONCONTROLELEMENT");

		// Bug - IFC2x3 Type Subtypes were not being returned: Issue #39
		classes.Should().Contain("IFCACTUATORTYPE");
		classes.Should().Contain("IFCAIRTERMINALTYPE");
	}
}
