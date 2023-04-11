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
        internal class UpdatableFile
        {
            public string Name { get; set; }
            public string Source { get; set; }
            public string Destination { get; set; }

            public UpdatableFile(string name, string source, string destination)
            {
                Name = name;
                Source = source;
                Destination = destination;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if information has been changed locally... need to reboot.</returns>
        internal static bool UpdateRequiresRestart()
        {
            if (!Exists)
                return false;

            var solutionDir = GetSolutionDirectory();
            if (solutionDir is null)
                return false;

            var updatables = GetAllUpdatable(solutionDir);
            if (!updatables.Any())
                return false;
            
            Console.WriteLine("Updates are available from the IDS repository:");
            foreach (var updatable in updatables)
                Console.WriteLine($"- {updatable.Name}");

            Console.WriteLine("Get these updates from IDS repository? (y/n)");
            var k = Console.ReadKey();
            Console.WriteLine();
            if (k.Key != ConsoleKey.Y)
            {
                Console.WriteLine("Update skipped.");
                return false;
            }
            foreach (var updatableFile in updatables)
                File.Copy(updatableFile.Source, updatableFile.Destination, true);   

            return true;
        }

        public static IEnumerable<UpdatableFile> GetAllUpdatable(DirectoryInfo solutionDir)
        {
            return UpdateDocumentationUnits(solutionDir)
                .Concat(UpdateEmbeddedSchema(solutionDir));
        }

        private static IEnumerable<UpdatableFile> UpdateDocumentationUnits(DirectoryInfo solutionDir)
        {
            var sourceFile = BuildingSmartRepoFiles.GetDocumentation("units.md");
            if (sourceFile.Exists)
            {
                var destination = Path.Combine(
                    solutionDir.FullName,
                    "ids-lib.codegen",
                    "buildingSMART",
                    "units.md"
                    );
                if (BuildingSmartRepoFiles.FilesAreIdentical(sourceFile, new FileInfo(destination)))
                    yield break;
                yield return new UpdatableFile("Units documentation file", sourceFile.FullName, destination);
            }
        }

        private static IEnumerable<UpdatableFile> UpdateEmbeddedSchema(DirectoryInfo solutionDir)
        {
            var sourceFile = BuildingSmartRepoFiles.GetDevelopment("ids.xsd");
            if (sourceFile.Exists)
            {
                var destination = Path.Combine(
                   solutionDir.FullName,
                    "ids-lib",
                    "Resources",
                    "XsdSchemas",
                    "ids.xsd"
                    );
                if (BuildingSmartRepoFiles.FilesAreIdentical(sourceFile, new FileInfo(destination)))
                    yield break;
                yield return new UpdatableFile("Ids schema file", sourceFile.FullName, destination);
            }
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