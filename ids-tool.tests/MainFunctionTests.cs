using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using IdsTool;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using static IdsLib.Audit;

namespace idsTool.tests;

public class MainFunctionTests
{
    private const string schemaFile = @"bsFiles/ids.xsd";
    private const string badSchemaFile = @"bsFiles/badIds.xsd";
    private const string idsFile = @"bsFiles/IDS_ucms_prefab_pipes_IFC2x3.ids";

    public MainFunctionTests(ITestOutputHelper outputHelper)
    {
        XunitOutputHelper = outputHelper;
    }
    private ITestOutputHelper XunitOutputHelper { get; }

    [Fact]
    public void CanRunProvidingSchema()
    {
        var c = new AuditOptions
        {
            SchemaFiles = new List<string> { schemaFile },
            InputSource = idsFile
        };
        var ret = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        ret.Should().Be(Status.Ok);
    }

    [Fact]
    public void RunProvidingBadSchemaFailsGracefully()
    {
        var c = new AuditOptions
        {
            SchemaFiles = new List<string> { badSchemaFile },
            InputSource = idsFile
        };
        var ret = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        ret.Should().Be(Status.XsdSchemaError);
    }

    [Fact]
    public void CanRunWithNoSchema()
    {
        var c = new AuditOptions
        {
            InputSource = idsFile
        };
        var ret = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        ret.Should().Be(Status.Ok);
    }

    [Fact]
    public void DoesNotBlockFiles()
    {
        // prepare the file to delete in the end
        var tmp = Path.GetTempFileName();
        File.Copy(idsFile, tmp, true);
        var c = new AuditOptions
        {
            SchemaFiles = new List<string> { schemaFile },
            InputSource = tmp
        };
        Run(c); // does not check results.
        File.Delete(tmp);
    }
}