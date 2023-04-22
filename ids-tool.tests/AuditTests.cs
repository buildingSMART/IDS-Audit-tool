using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using IdsTool;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

    [SkippableTheory]
    [MemberData(nameof(GetDevelopmentIdsFiles))]
    public void FullAuditOfDevelopmentFilesOk(string developmentIdsFile)
    {
        Skip.If(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = GetDevelopmentFileInfo(developmentIdsFile);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

    [SkippableTheory]
    [MemberData(nameof(GetTestCaseIdsFiles))]
    public void OmitContentAuditOfDocumentationFilesOk(string developmentIdsFile)
    {
        Skip.If(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = GetTestCaseFileInfo(developmentIdsFile);
        var c = new AuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = true,
            SchemaFiles = new[] { "bsFiles/ids.xsd" }
        };
        var auditResult = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        // hack to provide milder error because we don't have control on the test case generator
        Skip.If(auditResult != Audit.Status.Ok, "no control over sample folder.");
        auditResult.Should().Be(Audit.Status.Ok);
    }

    [SkippableTheory]
    [MemberData(nameof(GetTestCaseIdsFiles))]
    public void AuditOfDocumentationPassFilesOk(string developmentIdsFile)
    {
        Skip.If(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = GetTestCaseFileInfo(developmentIdsFile);
        var c = new AuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAuditPattern = @"\\fail-",
            SchemaFiles = new[] { "bsFiles/ids.xsd" }
        };
        var auditResult = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        // hack to provide milder error because we don't have control on the test case generator
        Skip.If(auditResult != Audit.Status.Ok, "no control over sample folder.");
        auditResult.Should().Be(Audit.Status.Ok);
    }

    [Theory]
    [InlineData("ValidFiles/nested_entity.ids")]
    [InlineData("ValidFiles/property.ids")]
    public void FullAuditPass(string path)
    {
        var f = new FileInfo(path);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }


    [Theory]
    [InlineData("InvalidFiles/InvalidIfcVersion.ids", 1, Audit.Status.IdsStructureError)]
    [InlineData("InvalidFiles/InvalidIfcOccurs.ids", 6, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidEntityNames.ids", 3, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidAttributeNames.ids", 2, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidAttributeForClass.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPattern.ids", 4, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPredefinedType.ids", 5, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/invalidPropertyMeasures.ids", 3, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/EntityImpossible.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcPartOf.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcPropertyForType.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcPropertyInPset.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidCustomPsetBecauseOfPrefix.ids", 2, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidClassificationImplication.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidMeasureForStandardProperty.ids", 2, Audit.Status.IdsContentError)]
    public void FullAuditFail(string path, int numErr, Audit.Status status)
    {
        var f = new FileInfo(path);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, status, numErr);
    }

    [Theory]
    [InlineData("InvalidFiles/InvalidIfcVersion.ids", 1, Audit.Status.IdsStructureError)]
    [InlineData("InvalidFiles/InvalidIfcOccurs.ids", 6, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidEntityNames.ids", 3, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidAttributeNames.ids", 2, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidAttributeForClass.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPattern.ids", 4, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPredefinedType.ids", 5, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/invalidPropertyMeasures.ids", 3, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/EntityImpossible.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcPartOf.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcPropertyForType.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcPropertyInPset.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidCustomPsetBecauseOfPrefix.ids", 2, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidClassificationImplication.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidMeasureForStandardProperty.ids", 2, Audit.Status.IdsContentError)]
    public void FullAuditFailWithStream(string path, int numErr, Audit.Status status)
    {
        var f = new FileInfo(path);
        using var stream = f.OpenRead();
        LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, status, numErr);

        // send again without constraints to show the feedback
        using var stream2 = f.OpenRead();
        LoggerAndAuditHelpers.FullAudit(stream2, XunitOutputHelper, status);
    }

    private const string NetworkIds = "https://raw.githubusercontent.com/buildingSMART/IDS/master/Development/IDS_ArcDox.ids";

    [Fact]
    public async Task TestSeekableNetworkStream()
    {
        var _httpClient = new HttpClient
        {
            Timeout = new TimeSpan(0, 0, 30)
        };
        _httpClient.DefaultRequestHeaders.Clear();
        using var response = await _httpClient.GetAsync(NetworkIds);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, Audit.Status.Ok);
        stream.Seek(0, SeekOrigin.Begin);
    }

    [Fact]
    public void TestNonSeekableNetworkStream()
    {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        var Request = WebRequest.Create(NetworkIds);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        var stream = Request.GetResponse().GetResponseStream();
        stream.Should().NotBeNull();
        LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, Audit.Status.Ok);
        Assert.Throws<System.NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
    }


}
