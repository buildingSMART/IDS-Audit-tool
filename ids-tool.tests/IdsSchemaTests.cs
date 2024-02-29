﻿using FluentAssertions;
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

        [SkippableFact]
        public async void BuildingSmartWebServerShouldReturnSchemaCorrectly()
        {

            var urls = new[] {
				// "https://raw.githubusercontent.com/buildingSMART/IDS/master/Development/ids.xsd",
				"http://standards.buildingsmart.org/IDS/0.9.6/ids.xsd" 
            };

            foreach (var url in urls)
            {
                HttpClient c = new HttpClient();
                var t = await c.GetAsync(url);
                var tp = t.Content.Headers.ContentType;
                Skip.If(tp is null);
                Skip.If(tp!.ToString() != "text/xml");

                //var request = HttpWebRequest.Create(url) as HttpWebRequest;
                //var response = request.GetResponse() as HttpWebResponse;
                //var contentType = response?.ContentType;				
            }
		}
    }
}
