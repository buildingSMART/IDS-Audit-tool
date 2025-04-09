using System.Reflection;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.IO.Xml.BsConf;

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

	private static IEntityFactory f2x3 = new Xbim.Ifc2x3.EntityFactoryIfc2x3();
	private static IEntityFactory f4 = new Xbim.Ifc4.EntityFactoryIfc4();
	private static IEntityFactory f4x3 = new Xbim.Ifc4x3.EntityFactoryIfc4x3Add2();

	internal static IEntityFactory GetFactory(string schema)
	{
		if (schema.Equals("IFC2X3", StringComparison.InvariantCultureIgnoreCase))
			return f2x3;
		else if (schema.Equals("IFC4", StringComparison.InvariantCultureIgnoreCase))
			return f4;
		else if (schema.Equals("IFC4X3", StringComparison.InvariantCultureIgnoreCase))
			return f4x3;
		else
			throw new NotImplementedException(schema.ToString());
	}
}
