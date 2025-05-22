using FluentAssertions;
using IdsLib;
using IdsLib.IdsSchema.IdsNodes;
using IdsLib.SchemaProviders;
using idsTool.tests.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute.Core;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests
{
	public class FeedbackTests
	{
		public FeedbackTests(ITestOutputHelper OutputHelper)
		{
			xlogger = LoggerAndAuditHelpers.GetXunitLogger<FeedbackTests>(OutputHelper);
		}

		ILogger<FeedbackTests> xlogger;

		[Theory(DisplayName = nameof(IdentificationIsCorrect))]
		[InlineData("InvalidFiles/InvalidAttributeForClass.ids", new [] { "/ids1/specifications1/specification1" })]
		[InlineData("IssueFiles/Issue 14 - ids file test.ids", new [] { 
			"/ids1/specifications1/specification1/applicability1/attribute2/name1/restriction1",
			"/ids1/specifications1/specification1/applicability1",
			"/ids1/specifications1/specification2", 
			})]
		[InlineData("InvalidFiles/InvalidIfcPartOf.ids", new [] { "/ids1/specifications1/specification1/applicability1/partOf1" })]
		[InlineData("InvalidFiles/InvalidAttributeNames.ids", new[] { 
			"/ids1/specifications1/specification1/requirements1/attribute1/name1/simpleValue1",
			"/ids1/specifications1/specification3/requirements1/attribute1/name1/simpleValue1",
			})]
		public void IdentificationIsCorrect(string file, string[] expectedePositionals)
		{
			var s = new SingleAuditOptions()
			{
				OmitIdsContentAudit = false,
				SchemaProvider = new FixedVersionSchemaProvider(IdsFacts.DefaultIdsVersion)
			};
			var f = new FileInfo(file);
			using var stream = f.OpenRead();
			var logger = new IdentificationLogger();
			var res = Audit.Run(stream, s, logger);
			var foundPositions = logger.Identifications.Select(x=>x.PositionalIdentifier).ToList();
			foreach (var expectedPositional in expectedePositionals)
			{
				if (!foundPositions.Contains(expectedPositional))
				{
					xlogger.LogInformation($"Found values are : {string.Join(", ", foundPositions)}");
				}
				foundPositions.Contains(expectedPositional).Should().BeTrue();
			}
		}
	}
}
