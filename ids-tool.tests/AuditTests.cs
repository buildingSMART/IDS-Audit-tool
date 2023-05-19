using FluentAssertions;
using IdsLib;
using IdsLib.SchemaProviders;
using idsTool.tests.Helpers;
using IdsTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        // why does this throw an exception when we don't #define ManageReadLoopException?
        // should the exception be prevented by the schema validation?
        // 
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


    public static IEnumerable<object[]> GetInvalidCases()
    {
        yield return new object[] { "InvalidFiles/InvalidIfcVersion.ids", 2, Audit.Status.IdsStructureError };
        yield return new object[] { "InvalidFiles/InvalidIfcOccurs.ids", 6, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidEntityNames.ids", 3, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidAttributeNames.ids", 2, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidAttributeForClass.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcEntityPattern.ids", 4, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcEntityPredefinedType.ids", 5, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/invalidPropertyMeasures.ids", 3, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/EntityImpossible.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcPartOf.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcPropertyForType.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcPropertyInPset.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidCustomPsetBecauseOfPrefix.ids", 2, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidClassificationImplication.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidMeasureForStandardPsetProperty.ids", 2, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/xsdFailure.ids", 2, Audit.Status.IdsStructureError };
        yield return new object[] { "InvalidFiles/structureAndContentFailure.ids", 3, Audit.Status.IdsStructureError | Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcEnumerationDoubleValues.ids", 3, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcEnumerationIntegerValues.ids", 3, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcRestrictionType.ids", 1, Audit.Status.IdsContentError };
    }

    [Theory]
    [MemberData(nameof(GetInvalidCases))]
    public void FullAuditFail(string path, int numErr, Audit.Status status)
    {
        var f = new FileInfo(path);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, status, numErr);
    }

    [Theory]
    [MemberData(nameof(GetInvalidCases))]
    public void FullAuditFailWithStream(string path, int numErr, Audit.Status status)
    {
        var f = new FileInfo(path);
        if (!f.Exists) 
            return; // when a case matchiing error happens in linux, we can exit gracefully

        var d = f.Directory!; // if the file exists, the directory must also
        var t = d.GetFiles(Path.ChangeExtension(f.Name, "*")).Single();
        t.Name.Should().Be(f.Name);

        using var stream = f.OpenRead();
        LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, status, numErr);

        // send again without constraints to show the feedback
        using var stream2 = f.OpenRead();
        LoggerAndAuditHelpers.FullAudit(stream2, XunitOutputHelper, status);
    }

    private const string ValidNetworkIds = "https://raw.githubusercontent.com/buildingSMART/IDS/master/Development/IDS_ArcDox.ids";

    [Fact]
    public async Task TestSeekableNetworkStream()
    {
        var _httpClient = new HttpClient
        {
            Timeout = new TimeSpan(0, 0, 30)
        };
        _httpClient.DefaultRequestHeaders.Clear();
        using var response = await _httpClient.GetAsync(ValidNetworkIds);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, Audit.Status.Ok);
        stream.Seek(0, SeekOrigin.Begin);
    }

    [Fact]
    public void TestNonSeekableNetworkStream()
    {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        var Request = WebRequest.Create(ValidNetworkIds);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        var stream = Request.GetResponse().GetResponseStream();
        stream.Should().NotBeNull();
        LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, Audit.Status.UnhandledError);
        Assert.Throws<System.NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
    }


    [Fact]
    public void TestNonSeekableNetworkStream2()
    {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        var Request = WebRequest.Create(ValidNetworkIds);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        var stream = Request.GetResponse().GetResponseStream();
        stream.Should().NotBeNull();
        var s = new SingleAuditOptions()
        {
            OmitIdsContentAudit = false,
            SchemaProvider = new SeekableStreamSchemaProvider()
        };
        LoggerAndAuditHelpers.AuditWithStream(stream, s, XunitOutputHelper, Audit.Status.UnhandledError, -1);

        Assert.Throws<System.NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
    }

}
