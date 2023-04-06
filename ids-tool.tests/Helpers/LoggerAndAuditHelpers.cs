using FluentAssertions;
using IdsLib;
using IdsTool;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests.Helpers;

internal static class LoggerAndAuditHelpers
{
    internal static Audit.Status AuditWithoutExpectations(AuditOptions c, ITestOutputHelper OutputHelper)
    {
        return AuditWithOptions(c, OutputHelper, null, -1);
    }

    internal static Audit.Status AuditWithOptions(AuditOptions c, ITestOutputHelper OutputHelper, Audit.Status? expectedOutcome = Audit.Status.Ok, int expectedWarnAndErrors = 0)
    {
        ILogger logg = GetXunitLogger(OutputHelper);
        var checkResult = Audit.Run(c, logg); // run for xunit output of logging
        if (expectedOutcome.HasValue)
            checkResult.Should().Be(expectedOutcome.Value);
        if (expectedWarnAndErrors == -1)
            return checkResult;
        var loggerMock = new Mock<ILogger<AuditTests>>();
        Audit.Run(c, loggerMock.Object); // run for testing of log errors and warnings
        CheckErrorsAndWarnings(loggerMock, expectedWarnAndErrors);
        return checkResult;
    }

    private static void CheckErrorsAndWarnings(Mock<ILogger<AuditTests>> loggerMock, int expectedErrCount)
    {
        var loggingCalls = loggerMock.Invocations.Select(x => x.ToString() ?? "").ToArray(); // this creates the array of logging calls
        var errorAndWarnings = loggingCalls.Where(x => x.Contains("LogLevel.Error") || x.Contains("LogLevel.Warning"));
        errorAndWarnings.Count().Should().Be(expectedErrCount, "mismatch with expected error/warning count");
    }

    private static ILogger GetXunitLogger(ITestOutputHelper helper)
    {
        var services = new ServiceCollection()
                    .AddLogging((builder) => builder.AddXUnit(helper));
        IServiceProvider provider = services.BuildServiceProvider();
        var logg = provider.GetRequiredService<ILogger<AuditTests>>();
        Assert.NotNull(logg);
        return logg;
    }

    internal static void FullAudit(FileInfo f, ITestOutputHelper xunitOutputHelper, Audit.Status expectedOutcome, int expectedWarnAndErrors)
    {
        var c = new AuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = false,
        };
        AuditWithOptions(c, xunitOutputHelper, expectedOutcome, expectedWarnAndErrors);
    }


    internal static void OmitContentAudit(FileInfo f, ITestOutputHelper xunitOutputHelper, Audit.Status expectedOutcome, int expectedWarnAndErrors)
    {
        var c = new AuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = true,
            SchemaFiles = new[] { "/bsFiles/ids093.xsd" }
        };
        AuditWithOptions(c, xunitOutputHelper, expectedOutcome, expectedWarnAndErrors);
    }

}
