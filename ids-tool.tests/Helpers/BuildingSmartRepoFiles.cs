using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace idsTool.tests.Helpers;

public class BuildingSmartRepoFiles
{
    private static string IdsRepoPath
    {
        get
        {
            var d = new DirectoryInfo(".");
            while (d is not null)
            {
                var subIDS = d.GetDirectories("IDS").FirstOrDefault();
                if (subIDS != null)
                    return subIDS.FullName;
                d = d.Parent;
            }
            return ".";
        }
    }

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

    private static string IdsTestcasesPath => Path.Combine(IdsDocumentationPath, "testcases");
    private static string IdsDevelopmentPath => Path.Combine(IdsRepoPath, @"Development");
    private static string IdsDocumentationPath => Path.Combine(IdsRepoPath, @"Documentation");

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
        var schema = Path.Combine(IdsDevelopmentPath, "ids.xsd");
        return new FileInfo(schema);
    }

    internal static FileInfo GetTestCaseFileInfo(string idsFile)
    {
        var d = new DirectoryInfo(IdsTestcasesPath);
        var comb = d.FullName + idsFile;
        var f = new FileInfo(comb);
        f.Exists.Should().BeTrue("test file must be found");
        return f;
    }

    public static IEnumerable<object[]> GetTestCaseIdsFiles()
    {
        // start from current directory and look in relative position for the bs IDS repository
        var d = new DirectoryInfo(IdsTestcasesPath);
        if (!d.Exists)
        {
            yield return new object[] { "" };
            yield break;
        }
        foreach (var f in d.GetFiles("*.ids", SearchOption.AllDirectories))
        {
            yield return new object[] { f.FullName.Replace(d.FullName, "") };
        }
    }

    internal static FileInfo GetDevelopmentFileInfo(string idsFile)
    {
        var d = new DirectoryInfo(IdsDevelopmentPath);
        var comb = d.FullName + idsFile;
        var f = new FileInfo(comb);
        f.Exists.Should().BeTrue("test file must be found");
        return f;
    }

    public static IEnumerable<object[]> GetDevelopmentIdsFiles()
    {
        // start from current directory and look in relative position for the bs IDS repository
        var d = new DirectoryInfo(IdsDevelopmentPath);
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

    public static FileInfo GetDocumentation(string fileName)
    {
        return new FileInfo(Path.Combine(IdsDocumentationPath, fileName));
    }

    public static FileInfo GetDevelopment(string fileName)
    {
        return new FileInfo(Path.Combine(IdsDevelopmentPath, fileName));
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
