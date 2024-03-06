using idsTool.tests.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.IdsContentError, 1);
		}
	}
}
