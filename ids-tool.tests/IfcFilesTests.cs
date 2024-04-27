using FluentAssertions;
using idsTool.tests.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests
{
	public class IfcFilesTests : BuildingSmartRepoFiles
	{
		public IfcFilesTests(ITestOutputHelper outputHelper)
		{
			XunitOutputHelper = outputHelper;
		}
		private ITestOutputHelper XunitOutputHelper { get; }

		[SkippableTheory]
		[MemberData(nameof(GetIdsRepositoryTestCaseIfcFiles))]
		public void IfcFileIsOk(string developmentIfcFile)
		{
			Skip.If(developmentIfcFile == string.Empty, "IDS repository folder not available for extra tests.");
			FileInfo ifcFile = LoggerAndAuditHelpers.GetAndCheckDocumentationTestCaseFileInfo(developmentIfcFile);
			string[] musthave =
				[
				"fail-the_container_predefined_type_must_match_exactly_1_2.ifc",
				"fail-the_nest_predefined_type_must_match_exactly_1_2.ifc",
				"pass-a_group_predefined_type_must_match_exactly_2_2.ifc",
				"invalid-a_group_predefined_type_must_match_exactly_2_2.ifc",
				"pass-a_group_predefined_type_must_match_exactly_2_2.ifc",
				"pass-the_nest_predefined_type_must_match_exactly_2_2.ifc",
				];

			XunitOutputHelper.WriteLine($"Opening file `{ifcFile.FullName}`");
			using var store = IfcStore.Open(ifcFile.FullName);
			TestPredefinedType(store, musthave.Contains(ifcFile.Name));
		}

		private void TestPredefinedType(IfcStore store, bool mustHaveObjType)
		{
			var toTest = store.Instances.OfType<IIfcObject>().ToList();
			var atLeastOneObjectType = false;
			foreach (IIfcObject obj in toTest) 
			{
				// var ot = obj.ModelOf.Metadata.ExpressType(obj.GetType().Name.ToUpper());
				// var pt = ot.Properties.Values.Where(x => x.Name.ToLowerInvariant() == "predefinedtype").FirstOrDefault();
				XunitOutputHelper.WriteLine($"Evaluating #{obj.EntityLabel}: {obj.GetType().Name}");
				string? objectTypeString = null;
				if (obj.ObjectType.HasValue)
				{
					objectTypeString = obj.ObjectType?.Value?.ToString();
				}
				if (objectTypeString is not null)
					atLeastOneObjectType = true;
				
					
				var predefinedTypeEnum = obj.GetPredefinedTypeValue();
				if (predefinedTypeEnum == "USERDEFINED")
				{
					objectTypeString.Should().NotBeNullOrEmpty();
				}
				else
				{
					objectTypeString.Should().BeNull();
				}
			}
			if (mustHaveObjType)
				atLeastOneObjectType.Should().BeTrue();

		}
	}
}
