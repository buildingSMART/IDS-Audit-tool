using idsTool.tests.Helpers;
using System.Diagnostics;
using System.Text;

namespace IdsLib.codegen
{
    internal class IdsRepo_Updater
    {
        internal static DirectoryInfo? GetSolutionDirectory()
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

        internal static string ExecuteCommandLine(string argumentsString, bool strip = true)
        {
            var d = GetSolutionDirectory();
            var pathInclude = "Release";
#if DEBUG
            pathInclude = "Debug";
#endif

            var toolPath = (d?.GetFiles("ids-tool.exe", SearchOption.AllDirectories).FirstOrDefault(x=>x.FullName.Contains(pathInclude))
                ?? throw new Exception("Tool binary not found."));
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = toolPath.FullName,
                    Arguments = argumentsString,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            StringBuilder sb = new();
            proc.Start();
            while (!proc.StandardOutput.EndOfStream || !proc.StandardError.EndOfStream)
            {
                var line = proc.StandardOutput.ReadLine();
                if (line is not null)
                    sb.AppendLine(line);

                line = proc.StandardError.ReadLine();
                if (line is not null)
                    sb.AppendLine(line);
            }
            if (!strip)
                return sb.ToString();
            // remove first 3 lines
            var retArr = sb.ToString();
            string[] lines = retArr
                .Split(Environment.NewLine)
                .Skip(3)
                .ToArray();
            string ret = string.Join(Environment.NewLine, lines);
            return ret.Trim('\r', '\n');
        }

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
            
            Console.WriteLine("There are file differences between this and the IDS repository:");
            foreach (var updatable in updatables)
                Console.WriteLine($"- {updatable.Name}");
            Console.WriteLine("Should these files be updated? (y/n)");
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
                .Concat(UpdateEmbeddedSchema(solutionDir))
                .Concat(UpdateDataTypeDoc(solutionDir))
                .Concat(UpdateTestingSchema(solutionDir));
        }

		private static IEnumerable<UpdatableFile> UpdateDataTypeDoc(DirectoryInfo solutionDir)
		{
			var source = Path.Combine(
					solutionDir.FullName,
					"ids-lib.codegen",
					"buildingSMART",
					"DataTypes.md"
					);
            var sourceFile = new FileInfo(source);
			var destFile = BuildingSmartRepoFiles.GetDocumentation("DataTypes.md");
			if (sourceFile.Exists)
			{
				if (BuildingSmartRepoFiles.FilesAreIdentical(sourceFile, destFile))
					yield break;
				yield return new UpdatableFile("DataType documentation markdown", sourceFile.FullName, destFile.FullName);
			}
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
            var sourceFile = BuildingSmartRepoFiles.GetSchemaPath("ids.xsd");
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

        private static IEnumerable<UpdatableFile> UpdateTestingSchema(DirectoryInfo solutionDir)
        {
            var sourceFile = BuildingSmartRepoFiles.GetSchemaPath("ids.xsd");
            if (sourceFile.Exists)
            {
                var destination = Path.Combine(
                   solutionDir.FullName,
                    "ids-tool.tests",
                    "bsFiles",
                    "ids.xsd"
                    );
                if (BuildingSmartRepoFiles.FilesAreIdentical(sourceFile, new FileInfo(destination)))
                    yield break;
                yield return new UpdatableFile("testing ids schema file", sourceFile.FullName, destination);
            }
        }

        internal static FileInfo GetLibraryDocumentation()
        {
            var solutionDirectory = GetSolutionDirectory();
            if (solutionDirectory == null)
                throw new ArgumentNullException(nameof(solutionDirectory));
            var libDir = new DirectoryInfo(
                Path.Combine(solutionDirectory.FullName, "ids-lib")
                );
            if (!libDir.Exists) 
                throw new ArgumentNullException(nameof(solutionDirectory));
            var pathInclude = "Release";
#if DEBUG
            pathInclude = "Debug";
#endif
            var libPath = (libDir?.GetFiles("ids-lib.dll", SearchOption.AllDirectories).FirstOrDefault(x => x.FullName.Contains(pathInclude))
                ?? throw new Exception("lib binary not found."));
            return libPath;
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