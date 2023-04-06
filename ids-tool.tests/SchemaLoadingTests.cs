using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using IdsTool;
using System.IO;
using Xunit;

namespace idsTool.tests;

public class SchemaLoadingTests : BuildingSmartRepoFiles
{
    /// <summary>
    /// In case this test fails, see <see cref="AuditTests.FullAuditOfDevelopmentFilesOk"/> for issues in the file.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetDevelopmentIdsFiles))]
    public void CanLoadEmbeddedResourceSchema(string idsFile)
    {
        FileInfo f = GetDevelopmentFileInfo(idsFile);
        var c = new AuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = true,
        };
        var checkResult = Audit.Run(c);
        checkResult.Should().Be(Audit.Status.Ok);
    }

    [Theory]
    [InlineData("InvalidFiles/InvalidSchemaLocation.ids", Audit.Status.IdsStructureError)]
    [InlineData("InvalidFiles/InvalidElementInvalidContent.ids", Audit.Status.IdsStructureError)]
    [InlineData("ValidFiles/IDS_aachen_example.ids", Audit.Status.Ok)]
    public void CanFailInvalidFileLoadingEmbeddedResourceSchema(string file, Audit.Status expected)
    {
        var f = new FileInfo(file);
        var c = new AuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = true,
        };
        var checkResult = Audit.Run(c);
        checkResult.Should().Be(expected);
    }
}
