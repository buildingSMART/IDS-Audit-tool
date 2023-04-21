using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocMarkdown.Core;

namespace IdsLib.codegen
{
    internal static class IdsLib_DocumentationUpdater
    {
        static public void Execute()
        {
            var doc = IdsRepo_Updater.GetLibraryDocumentation();
            var dest = Path.Combine(
                IdsRepo_Updater.GetSolutionDirectory()!.FullName,
                "ids-lib-documentation"
                );
            var dirDest = new DirectoryInfo(dest);

            var sett = new XmlDocMarkdownSettings()
            {
                ShouldClean = true,
                VisibilityLevel = XmlDocVisibilityLevel.Public,
                SourceCodePath = "https://github.com/buildingSMART/IDS-Audit-tool/tree/main/ids-lib",
                SkipCompilerGenerated = true,
                SkipUnbrowsable = true,
            };
            XmlDocMarkdownGenerator.Generate(doc.FullName, dirDest.FullName, sett);
        }
    }
}
