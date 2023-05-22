using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;
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

		public class opts : IBatchAuditOptions
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

            opts o = new opts();
            o.Schemas.Add(repoSchema.FullName);
            o.InputSource = repoSchema.Directory!.FullName;

            var ret = Audit.Run(o, LoggerAndAuditHelpers.GetXunitLogger(XunitOutputHelper));
			ret.Should().Be(Audit.Status.Ok);

            // now test for failing option settings
            o.OmitIdsContentAuditPattern = "[";
			ret = Audit.Run(o, LoggerAndAuditHelpers.GetXunitLogger(XunitOutputHelper));
			ret.Should().Be(Audit.Status.InvalidOptionsError);

            // nothing to audit







		}

		[SkippableFact]
        public void XsdSchemaIsAlignedWithIdsRepository()
        {
            var toolSchema = BuildingSmartRepoFiles.GetIdsToolSchema();
            toolSchema.Should().NotBeNull();

            var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
            Skip.IfNot(repoSchema.Exists, "IDS repository folder not available for extra tests.");

            var sameContent = BuildingSmartRepoFiles.FilesAreIdentical(repoSchema, toolSchema);
            sameContent.Should().BeTrue("embedded schema and repository schema should be identical");
        }

        [SkippableFact]
        public void XsdTestSuiteSchemaIsAlignedWithIdsRepository()
        {
            var toolSchema = BuildingSmartRepoFiles.GetIdsTestSuiteSchema();
            toolSchema.Should().NotBeNull();

            var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
            Skip.IfNot(repoSchema.Exists, "IDS repository folder not available for extra tests.");

            var sameContent = BuildingSmartRepoFiles.FilesAreIdentical(repoSchema, toolSchema);
            sameContent.Should().BeTrue("testing schema and repository schema should be identical");
        }
    }
}
