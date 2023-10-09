using idsTool.tests.Helpers;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests
{
	public class IdsRepositoryIssueTests
	{
        public IdsRepositoryIssueTests(ITestOutputHelper outputHelper)
		{
			XunitOutputHelper = outputHelper;
		}
		private ITestOutputHelper XunitOutputHelper { get; }

		[Fact]
		public void Issue195_BaseRestrictionIsNotMandatory()
		{
			var f = new FileInfo("IssueFiles/IDS Repo/IDS Issue 195 - base restriction.ids");
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, IdsLib.Audit.Status.Ok, 0);
		}
	}
}
