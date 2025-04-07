using FluentAssertions;
using IdsLib.IfcSchema;
using idsTool.tests.Helpers;
using Microsoft.Extensions.Logging;
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
		public void Issue_49_ErrorLocation()
		{
			var f = new FileInfo("IssueFiles/Issue 49 - Error location.ids");
			var t = LoggerAndAuditHelpers.FullAuditLocations(f, XunitOutputHelper, LogLevel.Error);
			t.Any(x =>
				x.StartLineNumber == 44
				&&
				x.StartLinePosition == 12
				).Should().BeTrue();
		}
		
		[Fact]
		public void Issue_44_MeasureEnumeration()
		{
			var t = SchemaInfo.AllMeasureInformation.FirstOrDefault(x => x.IfcMeasure == "IfcMagneticFluxMeasure");
			t.Should().NotBeNull();
		}
	}
}
