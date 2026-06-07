using FluentAssertions;
using IdsLib.IfcSchema.TypeFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace idsLib.tests
{
	public class TypeConstraintTests
	{
		public enum TypeMode
		{
			keepInheritance,
			CollapseToConcrete
		}

		[Fact]
		public void DebugHelpingTests()
		{
			// just some quick tests to help debug the union and intersection logic
			var schemaVersion = IdsLib.IfcSchema.IfcSchemaVersions.Ifc4;
			
			GetUnionAndIntersectionWithInheritance(schemaVersion, ["IfcWall", "IfcWindow"], out var resultU, out var resultI);
			resultI.IsEmpty.Should().BeTrue();
			resultU.IsEmpty.Should().BeFalse();

			GetUnionAndIntersectionWithInheritance(schemaVersion, ["IfcWall", "IfcWallStandardCase"], out resultU, out resultI);
			resultI.IsEmpty.Should().BeFalse();
			resultU.IsEmpty.Should().BeFalse();
		}

		private static void GetUnionAndIntersectionWithInheritance(IdsLib.IfcSchema.IfcSchemaVersions schemaVersion, string[] matchedNames, out IIfcTypeConstraint resultU, out IIfcTypeConstraint resultI)
		{
			var filterS = matchedNames.Select(x => new IfcInheritanceTypeConstraint(x, schemaVersion) as IIfcTypeConstraint).ToList();

			resultU = filterS[0];
			resultI = filterS[0];
			for (var i = 1; i < filterS.Count; i++)
			{
				// Union and intersection both fail... need to fix 
				resultU = resultU.Union(filterS[i]);
				resultI = resultI.Intersect(filterS[i]);
			}
		}

		[Theory]
		[InlineData(TypeMode.keepInheritance)]
		[InlineData(TypeMode.CollapseToConcrete)]
		public void IfcTypeConstraintTest(TypeMode typeMode)
		{
			var builtElement = GetConstraint("IFCBUILTELEMENT", typeMode);
			var wall = new IfcInheritanceTypeConstraint("IFCWALL", IdsLib.IfcSchema.IfcSchemaVersions.Ifc4x3);
			var wallStd = new IfcInheritanceTypeConstraint("IFCWALLSTANDARDCASE", IdsLib.IfcSchema.IfcSchemaVersions.Ifc4x3);
			var window = new IfcInheritanceTypeConstraint("IFCWINDOW", IdsLib.IfcSchema.IfcSchemaVersions.Ifc4x3);

			var intersect = wall.Intersect(wallStd);
			intersect.IsEmpty.Should().BeFalse();
			intersect.ConcreteTypes.Should().Contain("IFCWALLSTANDARDCASE");
			wall.Intersect(window).IsEmpty.Should().BeTrue();

			// do we get an inheritance when possible for intersections?
			var elementIntersectionWithWall = builtElement.Intersect(wall);
			var wallIntersectionWithElement = wall.Intersect(builtElement);

			// check the types
			CheckType(elementIntersectionWithWall, typeMode);
			CheckType(wallIntersectionWithElement, typeMode);
			
			// ensure the top type is not included in the concrete types
			elementIntersectionWithWall.ConcreteTypes.Should().NotContain("IFCBUILTELEMENT");
			wallIntersectionWithElement.ConcreteTypes.Should().NotContain("IFCBUILTELEMENT");
			// ensure the lower type is included in the concrete types
			elementIntersectionWithWall.ConcreteTypes.Should().Contain("IFCWALL");
			wallIntersectionWithElement.ConcreteTypes.Should().Contain("IFCWALL");

			// do we get an inheritance when possible for unions?
			var elementUnionWithWall = builtElement.Union(wall);
			var wallUnionWithElement = wall.Union(builtElement);
			// check the types

			CheckType(elementUnionWithWall, typeMode);
			CheckType(wallUnionWithElement, typeMode);
			
			// ensure the top type is included in the concrete types
			elementUnionWithWall.ConcreteTypes.Should().Contain("IFCBUILTELEMENT");
			wallUnionWithElement.ConcreteTypes.Should().Contain("IFCBUILTELEMENT");

			// when we intersect with an empty constraint, we should get an empty constraint
			wall.Intersect(IfcTypeConcreteListConstraint.Empty).IsEmpty.Should().BeTrue();

			// when we intsersect with a concrete constraint we get a concrete constraint
			var concreteWall = new IfcTypeConcreteListConstraint(["IFCWALL"]);
			var hybridWall = wall.Intersect(concreteWall);
			hybridWall.Should().BeOfType<IfcTypeConcreteListConstraint>();

		}

		private void CheckType(IIfcTypeConstraint constraint, TypeMode typeMode)
		{
			if (typeMode == TypeMode.CollapseToConcrete)
				constraint.Should().BeOfType<IfcTypeConcreteListConstraint>();
			else
				constraint.Should().BeOfType<IfcInheritanceTypeConstraint>();
		}

		private static IIfcTypeConstraint GetConstraint(string typeName, TypeMode typeMode)
		{
			var t = new IfcInheritanceTypeConstraint(typeName, IdsLib.IfcSchema.IfcSchemaVersions.Ifc4x3);
			if (typeMode == TypeMode.CollapseToConcrete)
			{
				return new IfcTypeConcreteListConstraint(t.ConcreteTypes);
			}
			return t;
		}
	}
}
