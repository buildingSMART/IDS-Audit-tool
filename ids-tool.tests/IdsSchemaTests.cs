using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
