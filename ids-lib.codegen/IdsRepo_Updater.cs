using idsTool.tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc4.StructuralElementsDomain;

namespace IdsLib.codegen
{
    internal class IdsRepo_Updater
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if information has been changed locally... need to reboot.</returns>
        internal static bool TryPromptUpdate()
        {
            if (!Exists)
                return false;

            var solutionDir = GetSolutionDirectory();
            if (solutionDir is null)
                return false;

            Console.WriteLine("Get updates from IDS repository? (y/n)");
            var k = Console.ReadKey();
            Console.WriteLine();
            if (k.Key != ConsoleKey.Y)
            {
                Console.WriteLine("Update skipped.");
                return false;
            }
            var ret = false;
            ret = UpdateDocumentationUnits(solutionDir) | ret;
            ret = UpdateEmbeddedSchema(solutionDir) | ret;
            return ret;
        }

        private static bool UpdateDocumentationUnits(DirectoryInfo solutionDir)
        {
            var units = BuildingSmartRepoFiles.GetDocumentation("units.md");
            if (units.Exists)
            {
                var destination = Path.Combine(
                    solutionDir.FullName,
                    "ids-lib.codegen",
                    "buildingSMART",
                    "units.md"
                    );
                if (BuildingSmartRepoFiles.FilesAreIdentical(units, new FileInfo(destination) ))
                    return false;
                File.Copy(units.FullName, destination, true);
                return true;
            }
            return false;
        }

        private static bool UpdateEmbeddedSchema(DirectoryInfo solutionDir)
        {
            var units = BuildingSmartRepoFiles.GetDevelopment("ids.xsd");
            if (units.Exists)
            {
                var destination = Path.Combine(
                   solutionDir.FullName,
                    "ids-lib",
                    "Resources",
                    "XsdSchemas",
                    "ids.xsd"
                    );
                if (BuildingSmartRepoFiles.FilesAreIdentical(units, new FileInfo(destination)))
                    return false;
                File.Copy(units.FullName, destination, true);
                return true;
            }
            return false;
        }

        private static DirectoryInfo? GetSolutionDirectory()
        {
            DirectoryInfo? d = new(".");
            while (d is not null)
            {
                var subIDS = d.GetFiles("ids-tool.sln").FirstOrDefault();
                if (subIDS != null)
                    return d;
                d = d.Parent;
            }
            return null;
        }

        

        internal static bool Exists 
        {
            get
            {
                var schema = BuildingSmartRepoFiles.GetIdsSchema();
                return schema.Exists;
            }
        }

    }
}
