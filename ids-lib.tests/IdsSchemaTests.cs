using Castle.Components.DictionaryAdapter.Xml;
using FluentAssertions;
using idsLib.tests.Helpers;
using IdsLib;
using IdsLib.IdsSchema.IdsNodes;
using IdsLib.IdsSchema.XsNodes;
using idsTool.tests.Helpers;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace idsLib.tests
{
    public class IdsSchemaTests
    {
        public IdsSchemaTests(ITestOutputHelper outputHelper)
		{
			XunitOutputHelper = outputHelper;
		}
		private ITestOutputHelper XunitOutputHelper { get; }

		public class BatchOpts : IBatchAuditOptions
		{
            public List<string> Schemas { get; set; } = new List<string>();  
            public bool AuditSchemaDefinition { get; set; } = true;
            public string InputExtension { get; } = "ids";
			public string InputSource { get; set; } = string.Empty;
            public bool OmitIdsContentAudit => false;
            public string OmitIdsContentAuditPattern { get; set; } = string.Empty;

            public IEnumerable<string> SchemaFiles => Schemas;		
        }

		public static IEnumerable<object[]> AllBaseTypes =>
			System.Enum.GetValues(typeof(XsTypes.BaseTypes))
				.Cast<XsTypes.BaseTypes>()
				.Select(v => new object[] { v });


		[Theory]
		[MemberData(nameof(xsdTypeStrings))]
		public void TestXsdTypeStrings(string baseType)
		{
			var t = XsTypes.GetBaseFrom(baseType);

			// now we need to check the availability of the regex string 
			var regexString = XsTypes.GetRegexString(t);
			regexString.Should().NotBeNullOrWhiteSpace($"base type {baseType} should have a regex string defined");
			XunitOutputHelper.WriteLine($"base type {baseType} has regex string: {regexString}");

			// now we need to check the availability of the associated XsdAllowedFacets
			XsTypes.GetAllowedFacets(t).Should().NotBeEmpty($"base type {baseType} should have a valid array of valid facets");
			XunitOutputHelper.WriteLine($"base type {baseType} has allowed facets: {string.Join(", ", XsTypes.GetAllowedFacets(t))}");

			// now we need to check the availability of a default value for the type
			var emptyValue = XsTypes.GetDefaultEmptyValue(t);
			XunitOutputHelper.WriteLine($"base type {baseType} has default empty value: {emptyValue}");
			
			// and finally check that the default value is valid for the type
			//
			// built in regex first
			XsTypes.IsValid(emptyValue, t).Should().BeTrue($"default empty value `{emptyValue}` should be valid for base type {baseType}");
			// and then a new regex
			Regex r = new Regex(regexString);

			// and then it should match 
			r.IsMatch(emptyValue).Should().BeTrue($"default empty value `{emptyValue}` should match the new regex built for base type {baseType}");
		}


		[Theory]
		[MemberData(nameof(AllBaseTypes))]
		public void TestXsdTypes(XsTypes.BaseTypes baseType)
		{
			if (baseType == XsTypes.BaseTypes.Invalid || baseType == XsTypes.BaseTypes.Undefined)
				return; // these are not valid base types, so we skip them

			XsTypes.GetValidBaseTypes().Should().Contain(baseType, $"base type {baseType} should be in the list of valid base types");

			var str = XsTypes.GetStringFromEnum(baseType);
			str.Should().NotBeNullOrWhiteSpace();

			// now we parse the string back to the type
			XsTypes.GetBaseFrom(str).Should().Be(baseType);

			// more tests are performed for the critical types (i.e. those which are defined in the schemas)
		}

		

		public static IEnumerable<object[]> xsdTypeStrings()
		{
			var vals = IdsLib.IfcSchema.SchemaInfo.AllDataTypes
				.Where(x=> !string.IsNullOrEmpty(x.BackingType))
				.Select(dt => dt.BackingType!)
				.Distinct()
				.ToList();
			return vals.Select(v => new object[] { v });
		}
		
		[Theory]
        [InlineData(@"ValidFiles/CanonicalVersions/canonical-1.0.ids", 0)]
        [InlineData(@"ValidFiles/CanonicalVersions/canonical-0.9.7.ids", 1)]
        public void CanAuditCanonicalVersions(string fileName, int warnings)
        {
			var f = new FileInfo(fileName);
			LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, warnings);
		}

		[Fact]
        public void AuditIdsRepositoryWithItsSchema()
        {
			var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
			Assert.SkipWhen(!repoSchema.Exists, "IDS repository folder not available for extra tests.");

			var o = new BatchOpts
			{
				InputSource = repoSchema.Directory!.FullName
			};
			o.Schemas.Add(repoSchema.FullName);

            var ret = Audit.Run(o, LoggerAndAuditHelpers.GetXunitLogger(XunitOutputHelper));
			ret.Should().Be(Audit.Status.Ok);

            // now test for failing option settings
            o.OmitIdsContentAuditPattern = "[";
			ret = Audit.Run(o, LoggerAndAuditHelpers.GetXunitLogger(XunitOutputHelper));
			ret.Should().Be(Audit.Status.InvalidOptionsError);

            // nothing to audit

		}

        // This ensures that the schema in the tool is aligned with the version of the IDS repository
		[Fact]
        public void XsdSchemaIsAlignedWithIdsRepository()
        {
            var toolSchema = BuildingSmartRepoFiles.GetIdsToolSchema();
            toolSchema.Should().NotBeNull();

            var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
            Assert.SkipWhen(!repoSchema.Exists, "IDS repository folder not available for extra tests.");

            var schemasAreIdentical = BuildingSmartRepoFiles.FilesAreIdentical(repoSchema, toolSchema);
            schemasAreIdentical.Should().BeTrue("embedded schema and repository schema should be identical");
        }

		// This ensures that the schema in the testing of the tool is aligned with the version of the IDS repository
		[Fact]
        public void XsdTestSuiteSchemaIsAlignedWithIdsRepository()
        {
            var toolSchema = BuildingSmartRepoFiles.GetIdsTestSuiteSchema();
            toolSchema.Should().NotBeNull();

            var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
			Assert.SkipWhen(!repoSchema.Exists, "IDS repository folder not available for extra tests.");

            repoSchema.Exists.Should().BeTrue();
            toolSchema.Exists.Should().BeTrue();

            var repoIdsText = File.ReadAllText(repoSchema.FullName);
            var toolIdsText = File.ReadAllText(toolSchema.FullName);

            repoIdsText.Should().Be(toolIdsText);
        }

        [Theory]
        [InlineData("http://standards.buildingsmart.org/IDS/0.9.7/ids.xsd")]
        [InlineData("http://standards.buildingsmart.org/IDS/1.0/ids.xsd")]
        // [InlineData("https://www.w3.org/2001/03/xml.xsd")]
        public async Task BuildingSmartWebServerShouldReturnSchemaCorrectly(string url)
        {
			// see https://stackoverflow.com/questions/4832357/whats-the-difference-between-text-xml-vs-application-xml-for-webservice-respons
			//
			/// If an XML document -- that is, the unprocessed, source XML document -- is readable by casual users, 
			/// text/xml is preferable to application/xml. MIME user agents (and web user agents) that do not 
			/// have explicit support for text/xml will treat it as text/plain, for example, by displaying the 
			/// XML MIME entity as plain text. Application/xml is preferable when the XML MIME entity is unreadable by casual users.
			/// 
			HttpResponseMessage? t = null;
			try
			{
				var c = new HttpClient();
				t = await c.GetAsync(url, TestContext.Current.CancellationToken);
			}
			catch
			{
				Assert.Skip("Unable to connect to BuildingSmart web server for extra tests.");
			}

			if (t == null)
			{
				Assert.Skip("Unable to connect to BuildingSmart web server for extra tests.");
			}
			t.StatusCode.Should().Be(HttpStatusCode.OK);
			var acceptable = new[] { "application/xml", "text/xml" };
			Assert.NotNull(t.Content.Headers.ContentType);
			var receivedContentType = t.Content.Headers.ContentType.MediaType;
			receivedContentType.Should().NotBeNull();
			receivedContentType.Should().BeOneOf(acceptable);
		}
    }
}
