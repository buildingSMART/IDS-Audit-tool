using FluentAssertions;
using IdsLib.IfcSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests
{
	public class IfcPredefinedTypeTests
	{
        public IfcPredefinedTypeTests(ITestOutputHelper outputHelper)
		{
			XunitOutputHelper = outputHelper;
		}
		private ITestOutputHelper XunitOutputHelper { get; }
        

        [Fact]
		// we have closed enumerations of predefined types, but because they can include the USERDEFINED value
		// we can't really enforce any limitation.
		// This class checks if there are any classes that have a closed list (i.e. lacking USERDEFINED).
		public void CheckAnyBoundedPredefinedTypesFails()
		{
			var sb = new StringBuilder();
			var schemas = new SchemaInfo[] { SchemaInfo.SchemaIfc2x3, SchemaInfo.SchemaIfc4, SchemaInfo.SchemaIfc4x3 };
			var SomeLackingUserDefinedInPredefined = false;
			foreach (var schema in schemas)
			{
				var classes = schema.GetAttributeClasses("PredefinedType", true);
				foreach (var schemaName in classes)
				{
					var schemaClass = schema[schemaName];
					schemaClass.Should().NotBeNull();
					if (!schemaClass!.PredefinedTypeValues.Contains("USERDEFINED"))
					{
						var values = string.Join(", ", schemaClass!.PredefinedTypeValues);
						sb.AppendLine($"{schema.Version} has {schemaName}; values = {values}");
						SomeLackingUserDefinedInPredefined = true;
					}
				}
			}
			SomeLackingUserDefinedInPredefined.Should().BeTrue();
			// if this test fails we can remove the test for PredefinedType constraints.
			XunitOutputHelper.WriteLine(sb.ToString());
		}
	}
}
