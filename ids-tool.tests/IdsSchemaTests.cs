using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests
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

		[SkippableFact]
        public void AuditIdsRepositoryWithItsSchema()
        {
			var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
			Skip.IfNot(repoSchema.Exists, "IDS repository folder not available for extra tests.");

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
		[SkippableFact]
        public void XsdSchemaIsAlignedWithIdsRepository()
        {
            var toolSchema = BuildingSmartRepoFiles.GetIdsToolSchema();
            toolSchema.Should().NotBeNull();

            var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
            Skip.IfNot(repoSchema.Exists, "IDS repository folder not available for extra tests.");

            var schemasAreIdentical = BuildingSmartRepoFiles.FilesAreIdentical(repoSchema, toolSchema);
            schemasAreIdentical.Should().BeTrue("embedded schema and repository schema should be identical");
        }

		// This ensures that the schema in the testing of the tool is aligned with the version of the IDS repository
		[SkippableFact]
        public void XsdTestSuiteSchemaIsAlignedWithIdsRepository()
        {
            var toolSchema = BuildingSmartRepoFiles.GetIdsTestSuiteSchema();
            toolSchema.Should().NotBeNull();

            var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
            Skip.IfNot(repoSchema.Exists, "IDS repository folder not available for extra tests.");

            var schemasAreIdentical = BuildingSmartRepoFiles.FilesAreIdentical(repoSchema, toolSchema);
            schemasAreIdentical.Should().BeTrue("testing schema and repository schema should be identical");
        }

        [Theory]
        [InlineData("http://standards.buildingsmart.org/IDS/0.9.7/ids.xsd")]
        [InlineData("http://standards.buildingsmart.org/IDS/1.0/ids.xsd")]
        [InlineData("https://www.w3.org/2001/03/xml.xsd")]
        public async void BuildingSmartWebServerShouldReturnSchemaCorrectly(string url)
        {
            // see https://stackoverflow.com/questions/4832357/whats-the-difference-between-text-xml-vs-application-xml-for-webservice-respons
            //
            /// If an XML document -- that is, the unprocessed, source XML document -- is readable by casual users, 
            /// text/xml is preferable to application/xml. MIME user agents (and web user agents) that do not 
            /// have explicit support for text/xml will treat it as text/plain, for example, by displaying the 
            /// XML MIME entity as plain text. Application/xml is preferable when the XML MIME entity is unreadable by casual users.
            /// 
            var acceptable = new[] { "application/xml", "text/xml" };

			var c = new HttpClient();
            var t = await c.GetAsync(url);
            Assert.NotNull(t.Content.Headers.ContentType);
			var receivedContentType = t.Content.Headers.ContentType.MediaType;
            receivedContentType.Should().NotBeNull();
            receivedContentType.Should().BeOneOf(acceptable);   
        }
    }
}
