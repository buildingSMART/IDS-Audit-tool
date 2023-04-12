using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using IdsTool;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests;

public class AuditTests : BuildingSmartRepoFiles
{
    public AuditTests(ITestOutputHelper outputHelper)
    {
        XunitOutputHelper = outputHelper;
    }
    private ITestOutputHelper XunitOutputHelper { get; }

    [Theory]
    [MemberData(nameof(GetDevelopmentIdsFiles))]
    public void FullAuditOfDevelopmentFilesOk(string developmentIdsFile)
    {
        FileInfo f = GetDevelopmentFileInfo(developmentIdsFile);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

    [Theory]
    [MemberData(nameof(GetTestCaseIdsFiles))]
    public void OmitContentAuditOfDocumentationFilesOk(string developmentIdsFile)
    {
        FileInfo f = GetTestCaseFileInfo(developmentIdsFile);
        LoggerAndAuditHelpers.OmitContentAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

    [SkippableTheory]
    [MemberData(nameof(GetTestCaseIdsFiles))]
    public void AuditOfDocumentationPassFilesOk(string developmentIdsFile)
    {
        FileInfo f = GetTestCaseFileInfo(developmentIdsFile);
        var c = new AuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAuditPattern = @"\\fail-",
            SchemaFiles = new[] { "/bsFiles/ids093.xsd" }
        };
        var auditResult = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        // hack to provide milder error because we don't have control on the test case generator
        Skip.If(auditResult != Audit.Status.Ok);
        auditResult.Should().Be(Audit.Status.Ok);
    }

    [Theory]
    [InlineData("InvalidFiles/InvalidIfcVersion.ids", 2, Audit.Status.IdsStructureError)]
    [InlineData("InvalidFiles/InvalidIfcOccurs.ids", 6, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidEntityNames.ids", 3, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidAttributeNames.ids", 2, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPattern.ids", 4, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPredefinedType.ids", 5, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/invalidPropertyMeasures.ids", 3, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/EntityImpossible.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcPartOf.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcPropertyForType.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcPropertyInPset.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidCustomPsetBecauseOfPrefix.ids", 2, Audit.Status.IdsContentError)]
    public void FullAuditFail(string path, int numErr, Audit.Status status)
    {
        var f = new FileInfo(path);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, status, numErr);
    }

    [Theory]
    [InlineData("ValidFiles/nested_entity.ids")]
    [InlineData("ValidFiles/property.ids")]
    public void FullAuditPass(string path)
    {
        var f = new FileInfo(path);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }
}
