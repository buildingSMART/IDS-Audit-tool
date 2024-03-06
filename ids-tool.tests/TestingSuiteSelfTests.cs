using FluentAssertions;
using idsTool.tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace idsTool.tests
{
	/// <summary>
	/// If these tests pass but the thest files are incoherent, rebuild the soluton.
	/// </summary>
	public class TestingSuiteSelfTests
	{
		[Fact]
		public void IdsRepositoryFileMethodsAreCoherent()
		{
			var files = BuildingSmartRepoFiles.GetIdsRepositoryTestCaseIdsFiles();
			files.Should().NotBeEmpty();
			foreach (var file in files)
			{
				var Frststr = file.First();
				Frststr.Should().BeOfType<string>();
				var str = Frststr as string;
				if (str == "") // repository not found
					continue;
				var f = LoggerAndAuditHelpers.GetAndCheckDocumentationTestCaseFileInfo(str!);
				f.Exists.Should().BeTrue($"{str} is returned from the automation code");
			}
		}
	}
}
