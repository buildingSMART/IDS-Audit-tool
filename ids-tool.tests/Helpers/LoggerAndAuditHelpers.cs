using FluentAssertions;
using IdsLib;
using IdsLib.IdsSchema.IdsNodes;
using IdsTool;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Core;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests.Helpers;

internal static class LoggerAndAuditHelpers
{
	public static FileInfo GetAndCheckIdsRepositoryDevelopmentFileInfo(string theFile)
    {
		FileInfo f = BuildingSmartRepoFiles.GetIdsRepositoryExampleFileInfo(theFile);
		f.Exists.Should().BeTrue("test file must be found");
		return f;
	}
	public static FileInfo GetAndCheckDocumentationTestCaseFileInfo(string theFile)
    {
		FileInfo f = BuildingSmartRepoFiles.GetDocumentationTestCaseFileInfo(theFile);
		f.Exists.Should().BeTrue("test file must be found");
        return f;
	}

    


	internal static Audit.Status AuditWithoutExpectations(BatchAuditOptions c, ITestOutputHelper OutputHelper)
    {
        return BatchAuditWithOptions(c, OutputHelper, null, -1);
    }

    internal static Audit.Status AuditWithStream(Stream stream, SingleAuditOptions s, ITestOutputHelper OutputHelper, Audit.Status? expectedOutcome = Audit.Status.Ok, int expectedWarnAndErrors = 0)
    {
        // we can only run once because we don't want to rewind the stream
        if (expectedWarnAndErrors == -1)
        {
            ILogger logg = GetXunitLogger(OutputHelper);
            var checkResult = Audit.Run(stream, s, logg); // run for xunit output of logging
            if (expectedOutcome.HasValue)
                checkResult.Should().Be(expectedOutcome.Value);
            return checkResult;
        }
        else
        {
            var loggerMock = Substitute.For<ILogger<AuditTests>>();
            var checkResult = Audit.Run(stream, s, loggerMock); // run for testing of log errors and warnings
            if (expectedOutcome.HasValue)
                checkResult.Should().Be(expectedOutcome.Value);
            CheckErrorsAndWarnings(loggerMock, expectedWarnAndErrors);
            return checkResult;
        }
    }

    internal static Audit.Status BatchAuditWithOptions(IBatchAuditOptions batchOptions, ITestOutputHelper OutputHelper, Audit.Status? expectedOutcome = Audit.Status.Ok, int expectedWarnAndErrors = 0)
    {
        ILogger logg = GetXunitLogger(OutputHelper);
        var checkResult = Audit.Run(batchOptions, logg); // run for xunit output of logging
        if (expectedOutcome.HasValue)
            checkResult.Should().Be(expectedOutcome.Value);
        if (expectedWarnAndErrors == -1)
            return checkResult;
        var loggerMock = Substitute.For<ILogger<AuditTests>>();
        Audit.Run(batchOptions, loggerMock); // run for testing of log errors and warnings
        CheckErrorsAndWarnings(loggerMock, expectedWarnAndErrors);
        return checkResult;
    }

	private static void CheckErrorsAndWarnings<T>(ILogger<T> loggerMock, int expectedWarnAndErrors)
	{
        var loggingCalls = loggerMock.ReceivedCalls().Select(x => GetFirstArg(x)).ToArray(); // this creates the array of logging calls
        var errorAndWarnings = loggingCalls.Where(x => x == "Error" || x == "Warning");
        errorAndWarnings.Count().Should().Be(expectedWarnAndErrors, "mismatch with expected error/warning count");
    }

	private static string GetFirstArg(ICall x)
	{
        var first = x.GetOriginalArguments().FirstOrDefault();
        if (first != null)
            return first.ToString() ?? "";
        return "<null>";
	}

	internal static ILogger GetXunitLogger(ITestOutputHelper helper)
    {
        var services = new ServiceCollection()
                    .AddLogging((builder) => builder.AddXUnit(helper).SetMinimumLevel(LogLevel.Debug));
        IServiceProvider provider = services.BuildServiceProvider();
        var logg = provider.GetRequiredService<ILogger<AuditTests>>();
        Assert.NotNull(logg);
        return logg;
    }

    internal static void FullAudit(FileInfo f, ITestOutputHelper xunitOutputHelper, Audit.Status expectedOutcome, int expectedWarnAndErrors = -1)
    {
        var c = new BatchAuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = false,
        };
        BatchAuditWithOptions(c, xunitOutputHelper, expectedOutcome, expectedWarnAndErrors);
    }

    internal static Audit.Status FullAudit(Stream stream, ITestOutputHelper xunitOutputHelper, Audit.Status? status = Audit.Status.Ok, int numErr = -1)
    {
        var s = new SingleAuditOptions()
        {
            OmitIdsContentAudit = false,
            IdsVersion = IdsFacts.DefaultIdsVersion
        };
        return AuditWithStream(stream, s, xunitOutputHelper, status, numErr);
    }

    internal static void OmitContentAudit(FileInfo f, ITestOutputHelper xunitOutputHelper, Audit.Status expectedOutcome, int expectedWarnAndErrors)
    {
        var c = new BatchAuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = true,
            SchemaFiles = new[] { "bsFiles/ids.xsd" }
        };
        BatchAuditWithOptions(c, xunitOutputHelper, expectedOutcome, expectedWarnAndErrors);
    }

    
}
