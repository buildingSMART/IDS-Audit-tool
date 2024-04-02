using FluentAssertions;
using IdsLib.IdsSchema.XsNodes;
using idsTool.tests.Helpers;
using IdsTool;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests;

public class RestrictionAuditTests
{
    public RestrictionAuditTests(ITestOutputHelper outputHelper)
    {
        XunitOutputHelper = outputHelper;
    }
    private ITestOutputHelper XunitOutputHelper { get; }

    [Fact]
    public void TestEntityNameAudit()
    {
        var f = new FileInfo("ValidFiles/Restriction/enumeration.ids");
        var c = new BatchAuditOptions()
        {
            InputSource = f.FullName,
        };
        LoggerAndAuditHelpers.BatchAuditWithOptions(c, XunitOutputHelper, IdsLib.Audit.Status.IdsContentError, 1);
    }

    [Theory]
    [InlineData("", XsTypes.BaseTypes.XsDuration, false)]
    [InlineData("2002-09-24", XsTypes.BaseTypes.XsDate, true)]
    [InlineData("2002-09-24Z", XsTypes.BaseTypes.XsDate, true)]
    [InlineData("2002-09-24-06:00", XsTypes.BaseTypes.XsDate, true)]
    [InlineData("2002-09-24+06:00", XsTypes.BaseTypes.XsDate, true)]
    [InlineData("09:00:00", XsTypes.BaseTypes.XsTime, true)]
    [InlineData("09:30:10.5", XsTypes.BaseTypes.XsTime, true)]
    [InlineData("09:30:10.5Z", XsTypes.BaseTypes.XsTime, true)]
    [InlineData("09:30:10-06:00", XsTypes.BaseTypes.XsTime, true)]
    [InlineData("09:30:10+06:00", XsTypes.BaseTypes.XsTime, true)]
    [InlineData("2002-05-30T09:00:00", XsTypes.BaseTypes.XsDateTime, true)]
    [InlineData("2002-05-30T09:00:00Z", XsTypes.BaseTypes.XsDateTime, true)]
    [InlineData("2002-05-30T09:30:10.5", XsTypes.BaseTypes.XsDateTime, true)]
    [InlineData("2002-05-30T09:30:10-06:00", XsTypes.BaseTypes.XsDateTime, true)]
    [InlineData("2002-05-30T09:30:10+06:00", XsTypes.BaseTypes.XsDateTime, true)]
    [InlineData("P5Y", XsTypes.BaseTypes.XsDuration, true)]
    [InlineData("P5Y2M10D", XsTypes.BaseTypes.XsDuration, true)]
    [InlineData("P5Y2M10DT15H", XsTypes.BaseTypes.XsDuration, true)]
    [InlineData("PT15H", XsTypes.BaseTypes.XsDuration, true)]
    [InlineData("-P10D", XsTypes.BaseTypes.XsDuration, true)]
    public void Evaluates(string stringValue, XsTypes.BaseTypes type, bool expected)
    {
        var valid = XsTypes.IsValid(stringValue, type);
        if (expected)
		    valid.Should().Be(expected, $"`{stringValue}` is a valid {type}");
        else
		    valid.Should().Be(expected, $"`{stringValue}` is not a valid {type}");

    }
}
