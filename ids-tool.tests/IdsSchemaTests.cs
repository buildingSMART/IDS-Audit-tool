using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using Xunit;

namespace idsTool.tests
{
    public class IdsSchemaTests
    {
        [SkippableFact]
        public void XsdSchemaIsAlignedWithIdsRepository()
        {
            var toolSchema = Audit.GetLatestIdsSchema();
            toolSchema.Should().NotBeNull();    
            
            var repoSchema = BuildingSmartRepoFiles.GetIdsSchema();
            Skip.IfNot(repoSchema.Exists);

            var sameContent = BuildingSmartRepoFiles.FilesAreIdentical(repoSchema, toolSchema!);
            sameContent.Should().BeTrue("embedded schema and repository schema should be identical");
        }
    }
}
