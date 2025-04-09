using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xbim.Common.Metadata;

namespace IdsLib.codegen;

public class IfcSchema_ClassGenerator
{
    /// <summary>
    /// SchemaInfo.GeneratedClass.cs
    /// </summary>
    public static string Execute()
    {
        var source = stub;
        foreach (var schema in Program.schemas)
        {
            var module = SchemaHelper.GetFactory(schema);
            // Debug.WriteLine(module.Name);
            var metaD = ExpressMetaData.GetMetadata(module);

            var sb = new StringBuilder();

            // trying to find a set of classes that matches the property types
            var HandledTypes = metaD.Types().Select(x=>x.Name.ToUpperInvariant()).ToList();
            foreach (var className in HandledTypes)
            {
                var daType = metaD.ExpressType(className.ToUpperInvariant());
                var t = daType.Type.GetInterfaces().Select(x => x.Name).Contains("IExpressValueType");
                //if (t ) //!string.IsNullOrEmpty(daType?.UnderlyingType?.Name))
                //{
                //    Debug.WriteLine($"{daType.Name}: {daType.UnderlyingType.Name} - {t}");
                //}

                // Enriching schema with predefined types
                var propPdefT = daType.Properties.Values.FirstOrDefault(x => x.Name == "PredefinedType");
                var predType = "Enumerable.Empty<string>()";
                if (propPdefT != null)
                {
                    var pt = propPdefT.PropertyInfo.PropertyType;
                    pt = Nullable.GetUnderlyingType(pt) ?? pt;
                    var vals = Enum.GetValues(pt);

                    List<string> pdtypes = new();
                    foreach (var val in vals)
                    {
                        if (val is null)
                            continue;
                        pdtypes.Add(val.ToString()!);
                    }
                    predType = NewStringArray(pdtypes.ToArray());
                }

                // other fields
                var abstractOrNot = daType.Type.IsAbstract ? "ClassType.Abstract" : "ClassType.Concrete";
                var ns = daType.Type.Namespace![5..];

                // Enriching schema with attribute names
                var attnames = NewStringArray(daType.Properties.Values.Select(x => x.Name).ToArray());

                sb.AppendLine($@"			new ClassInfo(""{daType.Name}"", ""{daType.SuperType?.Name}"", {abstractOrNot}, {predType}, ""{ns}"", {attnames}),");
            }
            source = source.Replace($"<PlaceHolder{schema}>\r\n", sb.ToString());
        }
        source = source.Replace($"<PlaceHolderVersion>", VersionHelper.GetFileVersion(typeof(ExpressMetaData)));
        return source;
    }

    private static string NewStringArray(string[] classes)
    {
        return @$"new[] {{ ""{string.Join("\", \"", classes)}"" }}";
    }

    private const string stub = @"// generated code via ids-lib.codegen using Xbim.Essentials <PlaceHolderVersion>, any changes to this file will be lost at next regeneration

using System.Linq;

namespace IdsLib.IfcSchema;

public partial class SchemaInfo
{
	private static partial SchemaInfo GetClassesIFC2x3()
	{
		var schema = new SchemaInfo(IfcSchemaVersions.Ifc2x3) {
<PlaceHolderIfc2x3>
		};
		return schema;
	}

	private static partial SchemaInfo GetClassesIFC4() 
	{
		var schema = new SchemaInfo(IfcSchemaVersions.Ifc4) {
<PlaceHolderIfc4>
		};
		return schema;
	}

    private static partial SchemaInfo GetClassesIFC4x3() 
	{
		var schema = new SchemaInfo(IfcSchemaVersions.Ifc4x3) {
<PlaceHolderIfc4x3>
		};
		return schema;
	}
}

";
}