namespace IdsLib.codegen;

internal class CodeHelpers
{
    internal static string NewStringArray(IEnumerable<string> classes)
    {
        return @$"new[] {{ ""{string.Join("\", \"", classes)}"" }}";
    }
}
