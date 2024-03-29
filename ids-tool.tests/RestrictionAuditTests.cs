﻿using idsTool.tests.Helpers;
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
}
