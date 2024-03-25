using FluentAssertions;
using IdsLib;
using IdsLib.IdsSchema.IdsNodes;
using IdsLib.SchemaProviders;
using Microsoft.Extensions.Logging;
using NSubstitute.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace idsTool.tests
{
	public class FeedbackTests
	{

		class IdentificationLogger : ILogger
		{
			public IDisposable? BeginScope<TState>(TState state) where TState : notnull
			{
				return null;
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return true;
			}

			internal IList<NodeIdentification> Identifications { get; set; } = new List<NodeIdentification>();

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
			{
				// Debug.WriteLine(state.GetType());
				if (state is IReadOnlyList<KeyValuePair<string, object>> vals)
				{
					var vls = vals.Where(x => x.Value is NodeIdentification).Select(sel=>sel.Value as NodeIdentification);
                    foreach (var item in vls)
                    {
						Identifications.Add(item!);
                    }
                }
			}
		}

		[Theory]
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
					// Debug.WriteLine($"Found values are : {string.Join(", ", foundPositions)}");
				}
				foundPositions.Contains(expectedPositional).Should().BeTrue();
			}
		}
	}
}
