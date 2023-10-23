﻿using FluentAssertions;
using IdsLib;
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

    internal static Audit.Status BatchAuditWithOptions(IBatchAuditOptions c, ITestOutputHelper OutputHelper, Audit.Status? expectedOutcome = Audit.Status.Ok, int expectedWarnAndErrors = 0)
    {
        ILogger logg = GetXunitLogger(OutputHelper);
        var checkResult = Audit.Run(c, logg); // run for xunit output of logging
        if (expectedOutcome.HasValue)
            checkResult.Should().Be(expectedOutcome.Value);
        if (expectedWarnAndErrors == -1)
            return checkResult;
        var loggerMock = Substitute.For<ILogger<AuditTests>>();
        Audit.Run(c, loggerMock); // run for testing of log errors and warnings
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

    internal static void FullAudit(Stream stream, ITestOutputHelper xunitOutputHelper, Audit.Status status, int numErr = -1)
    {
        var s = new SingleAuditOptions()
        {
            OmitIdsContentAudit = false,
            IdsVersion = IdsLib.IdsSchema.IdsNodes.IdsVersion.Ids0_9_6
        };
        AuditWithStream(stream, s, xunitOutputHelper, status, numErr);
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
