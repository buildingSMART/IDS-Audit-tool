using System.Reflection;

namespace IdsLib.codegen;

internal static class SchemaHelper
{
    public static Module GetModule(string schema)
    {
        if (schema.ToUpperInvariant() == "IFC2X3")
            return typeof(Xbim.Ifc2x3.Kernel.IfcProduct).Module;
        else if (schema.ToUpperInvariant() == "IFC4")
            return typeof(Xbim.Ifc4.Kernel.IfcProduct).Module;
        else if (schema.ToUpperInvariant() == "IFC4X3")
            return typeof(Xbim.Ifc4x3.Kernel.IfcProduct).Module;
        else
            throw new NotImplementedException(schema.ToString());
    }
}
