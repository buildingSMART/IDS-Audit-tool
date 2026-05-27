using FluentAssertions;
using idsLib.tests.Helpers;
using IdsLib;
using IdsLib.SchemaProviders;
using idsTool.tests.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static idsLib.tests.IdsSchemaTests;

namespace idsLib.tests;

public class AuditTests : BuildingSmartRepoFiles
{
    public AuditTests(ITestOutputHelper outputHelper)
    {
        XunitOutputHelper = outputHelper;
    }
    private ITestOutputHelper XunitOutputHelper { get; }

    [Theory]
	[MemberData(nameof(GetIdsRepositoryExampleIdsFiles))]
    public void FullAuditOfDevelopmentFilesOk(string developmentIdsFile)
    {
        Assert.SkipWhen(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = LoggerAndAuditHelpers.GetAndCheckIdsRepositoryDevelopmentFileInfo(developmentIdsFile);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

	[Theory]
	[MemberData(nameof(GetIdsRepositoryExampleIdsFiles))]
	public void FullAuditOfDevelopmentFilesWithItsSchemaOk(string developmentIdsFile)
	{
		var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
		Assert.SkipWhen(!repoSchema.Exists, "IDS repository folder not available for extra tests.");
		Assert.SkipWhen(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
		FileInfo f = LoggerAndAuditHelpers.GetAndCheckIdsRepositoryDevelopmentFileInfo(developmentIdsFile);

        BatchOpts opt = new BatchOpts() {
            InputSource = f.FullName
        };
		opt.Schemas.Add(repoSchema.FullName);
		LoggerAndAuditHelpers.BatchAuditWithOptions(opt, XunitOutputHelper, Audit.Status.Ok, 0);
	}

	[Theory]
    [MemberData(nameof(GetIdsRepositoryTestCaseIdsFiles))]
    public void OmitContentAuditOfDocumentationFilesOk(string developmentIdsFile)
    {
        // why does this throw an exception when we don't #define ManageReadLoopException?
        // should the exception be prevented by the schema validation?
        // 
        Assert.SkipWhen(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = LoggerAndAuditHelpers.GetAndCheckDocumentationTestCaseFileInfo(developmentIdsFile);
        var c = new BatchAuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAudit = true,
            SchemaFiles = new[] { "bsFiles/ids.xsd" }
        };
        var auditResult = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        // hack to provide milder error because we don't have control on the test case generator
        // Assert.SkipWhen(auditResult != Audit.Status.Ok, "no control over sample folder.");
        auditResult.Should().Be(Audit.Status.Ok);
    }

    /// <summary>
    /// this test skips the content evaluation on ids files that are expected to fail, 
    /// because they could be failing due to invalid content.
    /// It still checks them for issues at schema level.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetIdsRepositoryTestCaseIdsFiles))]
    public void AuditOfDocumentationPassFilesOk(string developmentIdsFile)
    {
        Assert.SkipWhen(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
        FileInfo f = LoggerAndAuditHelpers.GetAndCheckDocumentationTestCaseFileInfo(developmentIdsFile);
        var c = new BatchAuditOptions()
        {
            InputSource = f.FullName,
            OmitIdsContentAuditPattern = @"[\\/]invalid-",
            SchemaFiles = new[] { "bsFiles/ids.xsd" }
        };
        var auditResult = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
        // hack to provide milder error because we don't have control on the test case generator
        // Assert.SkipWhen(auditResult != Audit.Status.Ok, "no control over sample folder.");
        auditResult.Should().Be(Audit.Status.Ok);
    }	

	/// <summary>
	/// this test skips the content evaluation on ids files that are expected to fail, 
	/// because they could be failing due to invalid content.
	/// It still checks them for issues at schema level.
	/// </summary>
	[Theory]
	[MemberData(nameof(GetIdsRepositoryTestCaseIdsFiles))]
	public void AuditOfDocumentationInvalidFilesIsCoherent(string developmentIdsFile)
	{
		Assert.SkipWhen(developmentIdsFile == string.Empty, "IDS repository folder not available for extra tests.");
		FileInfo f = LoggerAndAuditHelpers.GetAndCheckDocumentationTestCaseFileInfo(developmentIdsFile);
		var c = new BatchAuditOptions()
		{
			InputSource = f.FullName,
			SchemaFiles = ["bsFiles/ids.xsd"]
		};
		var auditResult = LoggerAndAuditHelpers.AuditWithoutExpectations(c, XunitOutputHelper);
		// hack to provide milder error because we don't have control on the test case generator
		// Assert.SkipWhen(auditResult != Audit.Status.Ok, "no control over sample folder.");
        var exp = f.Name.StartsWith("invalid-") ? Audit.Status.IdsContentError : Audit.Status.Ok;
		auditResult.Should().Be(exp);
	}

	[Theory]
    [InlineData("ValidFiles/nested_entity.ids")]
    [InlineData("ValidFiles/property.ids")]
    [InlineData("ValidFiles/validEmptyPredefinedType.ids")]
    [InlineData("ValidFiles/RichFile.ids")]
    [InlineData("ValidFiles/RichFileAttribute_Ifc2x3.ids")]
    [InlineData("ValidFiles/RichFileAttribute_Ifc4.ids")]
    [InlineData("ValidFiles/RichFileAttribute_Ifc4x3.ids")]
    public void FullAuditPass(string path)
    {
        var f = new FileInfo(path);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

    public static TheoryData<string, int, Audit.Status> GetInvalidCases()
    {
        return new TheoryData<string, int, Audit.Status>
        {
            { "InvalidFiles/BadRichFile.ids", 6, Audit.Status.IdsContentError },
            { "InvalidFiles/EntityImpossible.ids", 1, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidApplicability.ids", 3, Audit.Status.IdsContentError | Audit.Status.IdsStructureError },
            { "InvalidFiles/InvalidAttributeCardinality.ids", 3, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidAttributeForClass.ids", 1, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidAttributeNames.ids", 2, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidAttributeTypes.ids", 7, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidClassification.ids", 2, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidClassificationImplication.ids", 1, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidCustomPsetBecauseOfPrefix.ids", 2, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidEmptyRequirements.ids", 1, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidEntityNames.ids", 3, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcEntityPattern.ids", 4, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcEntityPredefinedType.ids", 6, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcEnumerationDoubleValues.ids", 3, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcEnumerationIntegerValues.ids", 3, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcOccurs.ids", 11, Audit.Status.IdsStructureError | Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcPartOf.ids", 1, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcPartOfType.ids", 3, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcPropertyForType.ids", 1, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcPropertyInPset.ids", 1, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidIfcVersion.ids", 2, Audit.Status.IdsStructureError },
            { "InvalidFiles/InvalidMaterialCardinality.ids", 2, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidMeasureForStandardPsetProperty.ids", 2, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidPropertyCardinality.ids", 4, Audit.Status.IdsContentError },
            { "InvalidFiles/invalidPropertyMeasures.ids", 4, Audit.Status.IdsStructureError | Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidRestriction.ids", 2, Audit.Status.IdsContentError },
            { "InvalidFiles/InvalidRestrictions.ids", 6, Audit.Status.IdsContentError | Audit.Status.IdsStructureError },
            { "InvalidFiles/RichFileAttribute_Ifc2x3.ids", 4, Audit.Status.IdsContentError  },
            { "InvalidFiles/RichFileAttribute_Ifc4.ids", 7, Audit.Status.IdsContentError },
            { "InvalidFiles/RichFileAttribute_Ifc4x3.ids", 3, Audit.Status.IdsContentError },
            { "InvalidFiles/structureAndContentFailure.ids", 3, Audit.Status.IdsStructureError | Audit.Status.IdsContentError },
            { "InvalidFiles/xsdFailure.ids", 2, Audit.Status.IdsStructureError }
        };
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

		// send without constraints to show the feedback
		using var stream2 = f.OpenRead();
		LoggerAndAuditHelpers.FullAudit(stream2, XunitOutputHelper, status);

        // send again with constraints to run the check
		using var stream = f.OpenRead();
        LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, status, numErr);
    }

	// todo: update tfk-regenerate-testcase-ids to master branch
	private const string ValidNetworkIds = "https://github.com/CBenghi/IDS/raw/facetsReview/Development/IDS_ArcDox.ids";

    [Fact]
    public async Task TestSeekableNetworkStream()
    {
		bool gotResponse = false;
		try
		{

			var _httpClient = new HttpClient
			{
				Timeout = new TimeSpan(0, 0, 30)
			};
			_httpClient.DefaultRequestHeaders.Clear();
			using HttpResponseMessage response = await _httpClient.GetAsync(ValidNetworkIds, TestContext.Current.CancellationToken);
			gotResponse = true;
			response.EnsureSuccessStatusCode();
			var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
			var ret = LoggerAndAuditHelpers.FullAudit(stream, XunitOutputHelper, null);
			Assert.SkipWhen(ret != Audit.Status.Ok, "Online stream might be out of date.");
			stream.Seek(0, SeekOrigin.Begin);
		}
		catch (Exception)
		{
			Assert.SkipWhen(!gotResponse, "Network issue or file not available at the moment.");	
		}
        
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
