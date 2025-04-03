using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace idsTool.tests.Helpers;

public class BuildingSmartRepoFiles
{
	// attempts to find the root for the buildingsmart's ids standard repository
	private static string IdsRepoPath
    {
        get
        {
            var d = new DirectoryInfo(".");
            while (d is not null)
            {
                var subIDS = d.GetDirectories("IDS").FirstOrDefault();
                if (subIDS != null)
                {
                    if (
                        subIDS.GetDirectories("RepositoryAutomation").Any()
                        && subIDS.GetDirectories(".nuke").Any()
                        )
                        return subIDS.FullName;
                }
                d = d.Parent;
            }
            return ".";
        }
    }

    // attempts to find the root for the ids tool solution
    private static string IdsToolRepoPath
    {
        get
        {
            DirectoryInfo? d = new(".");
            while (d is not null)
            {
                var solution = d.GetFiles("ids-tool.sln").FirstOrDefault();
                if (solution != null)
                    return d.FullName;
                d = d.Parent;
            }
            return ".";
        }
    }

    // private static string IfcOpenShellTestcasesPath => Path.Combine(IfcOpenShellPath, "src", "ifctester", "test", "build", "testcases");
    private static string IdsRepositoryTestcasesPath => Path.Combine(IdsRepositoryDocumentationPath, "ImplementersDocumentation", "TestCases");
    private static string IdsRepositorySchemaPath => Path.Combine(IdsRepoPath, @"Schema");
    private static string IdsRepositoryExamplePath => Path.Combine(IdsRepoPath, "Documentation", @"Examples");
    private static string IdsRepositoryDocumentationPath => Path.Combine(IdsRepoPath, @"Documentation");

    public static FileInfo GetIdsToolSchema()
    {
        var schema = Path.Combine(IdsToolRepoPath, "ids-lib", "Resources", "XsdSchemas", "ids.xsd");
        return new FileInfo(schema);
    }

    public static FileInfo GetIdsTestSuiteSchema()
    {
        var schema = Path.Combine(IdsToolRepoPath, "ids-tool.tests", "bsFiles", "ids.xsd");
        return new FileInfo(schema);
    }

    public static FileInfo GetIdsSchema()
    {
        var schema = Path.Combine(IdsRepositorySchemaPath, "ids.xsd");
        return new FileInfo(schema);
    }

    public static FileInfo GetDocumentationTestCaseFileInfo(string idsFile)
    {
        var d = new DirectoryInfo(IdsRepositoryTestcasesPath);
        var comb = d.FullName + idsFile;
        var f = new FileInfo(comb);
        return f;
    }
    
	public static IEnumerable<object[]> GetIdsRepositoryTestCaseIdsFiles()
    {
        // start from current directory and look in relative position for the bs IDS repository
        var d = new DirectoryInfo(IdsRepositoryTestcasesPath);
        return GetFilesOrEmpty(d, "*.ids");
    }

	public static IEnumerable<object[]> GetIdsRepositoryTestCaseIfcFiles()
	{
		// start from current directory and look in relative position for the bs IDS repository
		var d = new DirectoryInfo(IdsRepositoryTestcasesPath);
		return GetFilesOrEmpty(d, "*.ifc");
	}

	private static IEnumerable<object[]> GetFilesOrEmpty(DirectoryInfo d, string searchPattern)
	{
		if (!d.Exists)
		{
			yield return new object[] { "" };
			yield break;
		}
		foreach (var f in d.GetFiles(searchPattern, SearchOption.AllDirectories))
		{
			yield return new object[] { f.FullName.Replace(d.FullName, "") };
		}
	}
    
	public static FileInfo GetIdsRepositoryExampleFileInfo(string idsFile)
    {
        var d = new DirectoryInfo(IdsRepositoryExamplePath);
        var comb = d.FullName + idsFile;
        var f = new FileInfo(comb);
        return f;
    }

    public static IEnumerable<object[]> GetIdsRepositoryExampleIdsFiles()
    {
        // start from current directory and look in relative position for the bs IDS repository
        var d = new DirectoryInfo(IdsRepositoryExamplePath);
        if (!d.Exists)
        {
            // returning a single invalid entry so that it can be skipped
            yield return new object[] { "" };
            yield break;
        }
            
        foreach (var f in d.GetFiles("*.ids", SearchOption.AllDirectories))
        {
            yield return new object[] { f.FullName.Replace(d.FullName, "") };
        }
    }

    public static FileInfo GetDocumentation(string fileName, string? folder = null)
    {
        if (!string.IsNullOrWhiteSpace(folder))
            return new FileInfo(Path.Combine(IdsRepositoryDocumentationPath, folder, fileName));
        return new FileInfo(Path.Combine(IdsRepositoryDocumentationPath, fileName));
    }

    public static FileInfo GetSchemaPath(string fileName)
    {
        return new FileInfo(Path.Combine(IdsRepositorySchemaPath, fileName));
    }

    public static bool FilesAreIdentical(FileInfo repoSchema, FileInfo toolSchema)
    {
        if (!repoSchema.Exists)
            return false;
        if (!toolSchema.Exists)
            return false;

        var repoContent = File.ReadAllText(repoSchema.FullName);
        var toolContent = File.ReadAllText(toolSchema.FullName);

        return repoContent.Equals(toolContent);
    }
}
