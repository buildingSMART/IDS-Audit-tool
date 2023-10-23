using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using IdsTool;
using Microsoft.Extensions.Logging;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests;

public class SchemaLoadingTests : BuildingSmartRepoFiles
{
    public SchemaLoadingTests(ITestOutputHelper outputHelper)
    {
        XunitOutputHelper = outputHelper;
    }
    private ITestOutputHelper XunitOutputHelper { get; }

    /// <summary>
    /// In case this test fails, see <see cref="AuditTests.FullAuditOfDevelopmentFilesOk"/> for issues in the file.
    /// </summary>
    [SkippableTheory]
    [MemberData(nameof(GetIdsRepositoryDevelopmentIdsFiles))]
    public void CanLoadEmbeddedResourceSchema(string idsFile)
    {
        Skip.If(idsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = GetIdsRepositoryDevelopmentFileInfo(idsFile);
        var c = new BatchAuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = true,
        };
        var checkResult = Audit.Run(c, LoggerAndAuditHelpers.GetXunitLogger(XunitOutputHelper));
        checkResult.Should().Be(Audit.Status.Ok);
    }

    [Theory]
    [InlineData("InvalidFiles/InvalidSchemaLocation.ids", Audit.Status.IdsStructureError)]
    [InlineData("InvalidFiles/InvalidElementInvalidContent.ids", Audit.Status.IdsStructureError)]
    [InlineData("ValidFiles/IDS_aachen_example.ids", Audit.Status.Ok)]
    public void CanFailInvalidFileLoadingEmbeddedResourceSchema(string file, Audit.Status expected)
    {
        var f = new FileInfo(file);
        var c = new BatchAuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = true,
        };
        ILogger logg = LoggerAndAuditHelpers.GetXunitLogger(XunitOutputHelper);
        var checkResult = Audit.Run(c, logg);
        checkResult.Should().Be(expected);
    }
}
