using Xbim.Common;

namespace IdsLib.codegen;

internal static class VersionHelper
{
    public static string GetFileVersion(Type type)
    {
        var info = new XbimAssemblyInfo(type);
        return info.FileVersion;
    }
}
