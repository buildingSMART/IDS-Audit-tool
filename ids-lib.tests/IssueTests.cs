using FluentAssertions;
using IdsLib.IfcSchema;
using idsTool.tests.Helpers;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests
{
	public class IssueTests
	{
		public IssueTests(ITestOutputHelper outputHelper)
		{
			XunitOutputHelper = outputHelper;
		}
		private ITestOutputHelper XunitOutputHelper { get; }

		[Fact]
		public void Issue08_RegexPattern()
		{
			var f = new FileInfo("IssueFiles/Issue 08 - Regex pattern.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.IdsContentError, 1);
		}

		[Fact]
		public void Issue09_XmlStructure()
		{
			var f = new FileInfo("IssueFiles/Issue 09 - XML structure.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.IdsStructureError, 1);
		}

		[Fact]
		public void Issue11_IfcLogicalIsValidDatatype()
		{
			var f = new FileInfo("IssueFiles/Issue 11 - IfcLogical.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.Ok);
		}

		[Fact]
		public void Issue25_IfcPropertySetFound()
		{
			var f = new FileInfo("IssueFiles/Issue 25 - Pset_ConstructionOccurence.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.Ok);
		}

		[Fact]
		public void Issue_28_EmptyRestriction()
		{
			var f = new FileInfo("IssueFiles/Issue 28 - Empty restriction.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.IdsContentError, 2);
		}

		[Fact]
		public void Issue_30_ShouldReturnError()
		{
			var f = new FileInfo("IssueFiles/Issue 30 - should return error.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.IdsContentError, 2);
		}

		[Fact]
		public void Issue_39_SubClassesOfObjectTypesAllowPsets()
		{
			var f = new FileInfo("IssueFiles/Issue 39 - IfcTypeObjects allowed.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.Ok);
		}

		[Fact]
		public void Issue_41_SchemaMatch()
		{
			// checking for multiple schemas should make it easy to write requirements that are trensferrable
			var f = new FileInfo("IssueFiles/Issue 41 - Schema match.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.Ok);
		}

		[Fact]
		public void Issue_43_DataType()
		{
			var f = new FileInfo("IssueFiles/Issue 43 - dataType.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.IdsContentError, 1);
		}

		[Fact]
		public void Issue_52_ErrorMessage()
		{
			// This test case is meant to check that the error message is clear enough to understand the problem and how to fix it.
			var f = new FileInfo("IssueFiles/Issue 52 - Error 306 - not clear.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.IdsStructureError);
		}

		[Fact]
		public void Issue_46_SchemaMatch()
		{
			var f = new FileInfo("IssueFiles/Issue 46 - Ensure feedback.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.Ok);
		}

		[Theory(DisplayName = "GitHub issues")]
		[InlineData("IssueFiles/Issue 53 - Userdefined.ids", IdsLib.Audit.Status.Ok)]
		[InlineData("IssueFiles/Issue 53 - Userdefined - Fail.ids", IdsLib.Audit.Status.IdsContentError)]
		[InlineData("IssueFiles/Issue 53 - IfcDistributionBoards Feeder is ok.ids", IdsLib.Audit.Status.Ok)]
		[InlineData("IssueFiles/Issue 53 - IfcDistributionBoards Feeder or Incomer (enumeration) is not ok.ids", IdsLib.Audit.Status.Ok)]
		[InlineData("IssueFiles/Issue 54 - Pset List Properties.ids", IdsLib.Audit.Status.IdsContentError, 1)] // the error is there to ensure that a test is actually performed
		[InlineData("IssueFiles/Issue 55 - RelationConstraintFail.ids", IdsLib.Audit.Status.IdsStructureError | IdsLib.Audit.Status.IdsContentError)]
		[InlineData("IssueFiles/Issue 55 - RelationConstraintOk.ids", IdsLib.Audit.Status.Ok)]
		[InlineData("IssueFiles/Issue 56 - Ifc2x3 mapping.ids", IdsLib.Audit.Status.Ok)]
		[InlineData("IssueFiles/Issue 59 - PEnums.ids", IdsLib.Audit.Status.IdsContentError, 1)]
		[InlineData("IssueFiles/Issue 60 - Restrictions.ids", IdsLib.Audit.Status.IdsStructureError | IdsLib.Audit.Status.IdsContentError, 6)]
		[InlineData("IssueFiles/Issue 61/IDS - TEST for Architectural Requriements.ids", IdsLib.Audit.Status.Ok)]
		[InlineData("IssueFiles/Issue 61/IDS - TEST for Electrical Requirements.ids", IdsLib.Audit.Status.Ok)]
		[InlineData("IssueFiles/Issue 61/IDS - TEST for Mechanical Requirements.ids", IdsLib.Audit.Status.Ok)]
		[InlineData("IssueFiles/Issue 61/IDS - TEST for Structural Requirements.ids", IdsLib.Audit.Status.Ok)]
		public void GithubIssues(string filename, IdsLib.Audit.Status expectedOutcome, int expectedErrorCount = -1)
		{
			var f = new FileInfo(filename);
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, expectedOutcome, expectedErrorCount);
		}
		
		[Fact]
		public void Issue_44_MeasureEnumeration()
		{
			var t = SchemaInfo.TryGetMeasureInformation("IfcMagneticFluxMeasure", out var measure);
			t.Should().BeTrue();
			measure.Should().NotBeNull();

			var t2 = SchemaInfo.TryGetMeasureInformation("IfcMagneticFlux", out var measure2);
			t2.Should().BeFalse();
			measure2.Should().BeNull();
		}

		[Fact]
		public void Issue_45_MeasureEnumeration()
		{
			var f = new FileInfo("IssueFiles/Issue 45 - IfcMassMeasure.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.Ok);
		}

		[InlineData("IFCFLOWTERMINAL",
			//"IFCFLOWTERMINALTYPE",
			"IFCAIRTERMINALTYPE",
			"IFCELECTRICAPPLIANCETYPE",
			"IFCELECTRICHEATERTYPE",
			"IFCFIRESUPPRESSIONTERMINALTYPE",
			"IFCGASTERMINALTYPE",
			"IFCLAMPTYPE",
			"IFCLIGHTFIXTURETYPE",
			"IFCOUTLETTYPE",
			"IFCSANITARYTERMINALTYPE",
			"IFCSTACKTERMINALTYPE",
			"IFCWASTETERMINALTYPE",

			"IFCDISTRIBUTIONCHAMBERELEMENTTYPE", "IFCDISTRIBUTIONELEMENTTYPE"	// From non-abstract super classes. Undesireable by a sideffect of supporting WallStandardCase
			)]
		[InlineData("IFCPLATE", "IFCPLATETYPE")]
		[InlineData("IFCWALL", "IFCWALLTYPE")]
		[InlineData("IFCWALLSTANDARDCASE", "IFCWALLTYPE")]
		[InlineData("IFCELEMENT", 
			"IFCDISTRIBUTIONELEMENTTYPE",
			"IFCFURNISHINGELEMENTTYPE",
			"IFCTRANSPORTELEMENTTYPE")]
		[Theory]
		public void Issue_57_RelationsAreCorrect_2x3(string ifcElement, params string[] expectedTypes)
		{
			var instance = SchemaInfo.SchemaIfc2x3[ifcElement];
			instance.Should().NotBeNull();

			instance.RelationTypeClasses.Should().BeEquivalentTo(expectedTypes, opts=> opts.WithoutStrictOrdering());
		}

		[InlineData("IFCFLOWTERMINAL",
			//"IFCFLOWTERMINALTYPE",
			"IFCFIRESUPPRESSIONTERMINALTYPE",
			"IFCSANITARYTERMINALTYPE", 
			"IFCSTACKTERMINALTYPE", 
			"IFCWASTETERMINALTYPE", 
			"IFCAIRTERMINALTYPE", 
			"IFCMEDICALDEVICETYPE", 
			"IFCSPACEHEATERTYPE", 
			"IFCAUDIOVISUALAPPLIANCETYPE", 
			"IFCCOMMUNICATIONSAPPLIANCETYPE", 
			"IFCELECTRICAPPLIANCETYPE", 
			"IFCLAMPTYPE", 
			"IFCLIGHTFIXTURETYPE", 
			"IFCOUTLETTYPE",

			"IFCDISTRIBUTIONCHAMBERELEMENTTYPE", "IFCDISTRIBUTIONELEMENTTYPE"   // From non-abstract super classes. Undesireable by a sideffect of supporting WallStandardCase
			)]
		[InlineData("IFCPLATE", "IFCPLATETYPE")]
		[InlineData("IFCWALL", "IFCWALLTYPE")]
		[InlineData("IFCWALLSTANDARDCASE", "IFCWALLTYPE")]
		[InlineData("IFCELEMENT",
			"IFCFURNISHINGELEMENTTYPE", 
			"IFCDISTRIBUTIONELEMENTTYPE", 
			"IFCCIVILELEMENTTYPE", 
			"IFCELEMENTASSEMBLYTYPE", 
			"IFCGEOGRAPHICELEMENTTYPE", 
			"IFCTRANSPORTELEMENTTYPE"
			)]
		[Theory]
		public void Issue_57_RelationsAreCorrect_Ifc4(string ifcElement, params string[] expectedTypes)
		{
			var instance = SchemaInfo.SchemaIfc4[ifcElement];
			instance.Should().NotBeNull();

			instance.RelationTypeClasses.Should().BeEquivalentTo(expectedTypes, opts => opts.WithoutStrictOrdering());
		}

	}
}
