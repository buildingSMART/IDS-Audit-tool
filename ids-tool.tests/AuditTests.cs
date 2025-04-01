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
using static idsTool.tests.IdsSchemaTests;

namespace idsTool.tests;

public class AuditTests : BuildingSmartRepoFiles
{
    public AuditTests(ITestOutputHelper outputHelper)
    {
        XunitOutputHelper = outputHelper;
    }
    private ITestOutputHelper XunitOutputHelper { get; }

    [SkippableTheory]
    [MemberData(nameof(GetIdsRepositoryExampleIdsFiles))]
    public void FullAuditOfDevelopmentFilesOk(string developmentIdsFile)
    {
        Skip.If(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = LoggerAndAuditHelpers.GetAndCheckIdsRepositoryDevelopmentFileInfo(developmentIdsFile);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

	[SkippableTheory]
	[MemberData(nameof(GetIdsRepositoryExampleIdsFiles))]
	public void FullAuditOfDevelopmentFilesWithItsSchemaOk(string developmentIdsFile)
	{
		var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
		Skip.IfNot(repoSchema.Exists, "IDS repository folder not available for extra tests.");
		Skip.If(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
		FileInfo f = LoggerAndAuditHelpers.GetAndCheckIdsRepositoryDevelopmentFileInfo(developmentIdsFile);

        BatchOpts opt = new BatchOpts() {
            InputSource = f.FullName
        };
		opt.Schemas.Add(repoSchema.FullName);
		LoggerAndAuditHelpers.BatchAuditWithOptions(opt, XunitOutputHelper, Audit.Status.Ok, 0);
	}

	[SkippableTheory]
    [MemberData(nameof(GetIdsRepositoryTestCaseIdsFiles))]
    public void OmitContentAuditOfDocumentationFilesOk(string developmentIdsFile)
    {
        // why does this throw an exception when we don't #define ManageReadLoopException?
        // should the exception be prevented by the schema validation?
        // 
        Skip.If(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = LoggerAndAuditHelpers.GetAndCheckDocumentationTestCaseFileInfo(developmentIdsFile);
        var c = new BatchAuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = true,
            SchemaFiles = new[] { "bsFiles/ids.xsd" }
        };
        var auditResult = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        // hack to provide milder error because we don't have control on the test case generator
        // Skip.If(auditResult != Audit.Status.Ok, "no control over sample folder.");
        auditResult.Should().Be(Audit.Status.Ok);
    }

    /// <summary>
    /// this test skips the content evaluation on ids files that are expected to fail, 
    /// because they could be failing due to invalid content.
    /// It still checks them for issues at schema level.
    /// </summary>
    [SkippableTheory]
    [MemberData(nameof(GetIdsRepositoryTestCaseIdsFiles))]
    public void AuditOfDocumentationPassFilesOk(string developmentIdsFile)
    {
        Skip.If(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = LoggerAndAuditHelpers.GetAndCheckDocumentationTestCaseFileInfo(developmentIdsFile);
        var c = new BatchAuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAuditPattern = @"\\invalid-",
            SchemaFiles = new[] { "bsFiles/ids.xsd" }
        };
        var auditResult = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        // hack to provide milder error because we don't have control on the test case generator
        // Skip.If(auditResult != Audit.Status.Ok, "no control over sample folder.");
        auditResult.Should().Be(Audit.Status.Ok);
    }	

	/// <summary>
	/// this test skips the content evaluation on ids files that are expected to fail, 
	/// because they could be failing due to invalid content.
	/// It still checks them for issues at schema level.
	/// </summary>
	[SkippableTheory]
	[MemberData(nameof(GetIdsRepositoryTestCaseIdsFiles))]
	public void AuditOfDocumentationInvalidFilesIsCoherent(string developmentIdsFile)
	{
		Skip.If(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
		FileInfo f = LoggerAndAuditHelpers.GetAndCheckDocumentationTestCaseFileInfo(developmentIdsFile);
		var c = new BatchAuditOptions()
		{
			InputSource = f.FullName,
			SchemaFiles = new[] { "bsFiles/ids.xsd" }
		};
		var auditResult = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
		// hack to provide milder error because we don't have control on the test case generator
		// Skip.If(auditResult != Audit.Status.Ok, "no control over sample folder.");
        var exp = f.Name.StartsWith("invalid-") ? Audit.Status.IdsContentError : Audit.Status.Ok;
		auditResult.Should().Be(exp);
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
        yield return new object[] { "InvalidFiles/InvalidIfcOccurs.ids", 11, Audit.Status.IdsStructureError | Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidEntityNames.ids", 3, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidAttributeNames.ids", 2, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidAttributeTypes.ids", 6, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidAttributeCardinality.ids", 3, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidAttributeForClass.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcEntityPattern.ids", 5, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcEntityPredefinedType.ids", 5, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/invalidPropertyMeasures.ids", 4, Audit.Status.IdsStructureError | Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidPropertyCardinality.ids", 4, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidMaterialCardinality.ids", 2, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/EntityImpossible.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcPartOf.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcPropertyForType.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcPropertyInPset.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidCustomPsetBecauseOfPrefix.ids", 2, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidClassificationImplication.ids", 1, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidClassification.ids", 2, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidMeasureForStandardPsetProperty.ids", 2, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/xsdFailure.ids", 2, Audit.Status.IdsStructureError };
        yield return new object[] { "InvalidFiles/structureAndContentFailure.ids", 3, Audit.Status.IdsStructureError | Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcEnumerationDoubleValues.ids", 3, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidIfcEnumerationIntegerValues.ids", 3, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidRestriction.ids", 2, Audit.Status.IdsContentError };
        yield return new object[] { "InvalidFiles/InvalidApplicability.ids", 3, Audit.Status.IdsContentError | Audit.Status.IdsStructureError };
        yield return new object[] { "InvalidFiles/InvalidRestrictions.ids", 6, Audit.Status.IdsContentError | Audit.Status.IdsStructureError };
    }

    [Theory]
    [MemberData(nameof(GetInvalidCases))]
    public void FullAuditFail(string path, int numErr, Audit.Status status)
    {
		PathCaseSensitiveMatch(path).Should().BeTrue();
		var f = new FileInfo(path);
		LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, status, numErr);
    }

	private static bool PathCaseSensitiveMatch(string path)
	{
        var parts = path.Split('/', '\\');
        var d = new DirectoryInfo(".");
        foreach (var part in parts)
        {
			var dir = d.GetDirectories().FirstOrDefault(x => x.Name == part);
            if (dir is not null)
                d = dir;
            else
				return d.GetFiles().Any(x => x.Name == part);
        }
        return true;
	}


	[Theory]
    [MemberData(nameof(GetInvalidCases))]
    public void FullAuditFailWithStream(string path, int numErr, Audit.Status status)
    {
        var f = new FileInfo(path);
        if (!f.Exists) 
            return; // when a case matching error happens in linux, we can exit gracefully

        var d = f.Directory!; // if the file exists, the directory must also
        var t = d.GetFiles(Path.ChangeExtension(f.Name, "*")).Single();
        t.Name.Should().Be(f.Name);

        using var stream = f.OpenRead();
        LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, status, numErr);

        // send again without constraints to show the feedback
        using var stream2 = f.OpenRead();
        LoggerAndAuditHelpers.FullAudit(stream2, XunitOutputHelper, status);
    }

	// todo: update tfk-regenerate-testcase-ids to master branch
	private const string ValidNetworkIds = "https://github.com/CBenghi/IDS/raw/facetsReview/Development/IDS_ArcDox.ids";

    [SkippableFact]
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
        var ret = LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, null);
        Skip.If(ret != Audit.Status.Ok, "Online stream might be out of date.");
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
