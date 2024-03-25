using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Xbim.Common.Metadata;

namespace IdsLib.codegen;

internal record typeMetadata
{
    public string Name { get; set; } = string.Empty;
    public List<string> Schemas { get; set; } = new();
    public string Exponents { get; set; } = string.Empty;
    public string[]? Fields { get; set; }
    public string XmlBackingType { get; set; } = string.Empty;

    internal void AddSchema(string schema)
    {
        Schemas.Add(schema);
    }

    internal void SetBacking(string backing)
    {

        if (XmlBackingType ==  backing) 
            return;
        if (string.IsNullOrEmpty(XmlBackingType))
        {
            XmlBackingType = backing;
            return;
        }
        Debug.WriteLine($"Conflicting type for {Name}: {backing} vs. {XmlBackingType}");
    }
}
    
public class IfcSchema_DatatypeNamesGenerator
{
    internal static string Execute(out Dictionary<string, typeMetadata> dataTypeDictionary)
    {

        // start from documented... detail the exponents
        // then add their schemas and the others, with relevant schemas

        var documentedMeasures = GetDocumentationMeasures().ToList();
		dataTypeDictionary = documentedMeasures.ToDictionary(x => x.Name, x => x);

        //var datatypeNames = GetAllDatatypeNames().ToList();
        //var dttNames = new Dictionary<string, List<string>>();

        foreach (var schema in Program.schemas)
        {
            System.Reflection.Module module = SchemaHelper.GetModule(schema);
            var metaD = ExpressMetaData.GetMetadata(module);

            var values = GetExpressValues(metaD)
                .Concat(GetEnumValueTypes(schema))
                .Select(x=>x.ToUpperInvariant())
                .Distinct();

            foreach (var daDataType in values)
            {
                var tmpType = metaD.ExpressType(daDataType);
                var xmlType = "xs:string"; // default for enums
                if (tmpType is null)
                {
                    // this is the case of enums
                    var t = module.GetTypes().FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name.ToUpperInvariant() == daDataType);
                    if (t == null)
                    {
                        continue;
                    }
                    if (t.BaseType != typeof(Enum))
                    {
                        continue;
                    }
                }
                else
                {
                    xmlType = MapToXml(tmpType.UnderlyingType);
					if (daDataType == "IFCCOUNTMEASURE") // exception for Xbim implementation quirkiness
						xmlType = "xs:integer";
				}
                


				if (dataTypeDictionary.TryGetValue(daDataType, out var lst))
                {
                    lst.AddSchema(schema);
                    lst.SetBacking(xmlType);
                }
                else
                {
                    var t = new typeMetadata() { Name = daDataType };
                    t.AddSchema(schema);
					t.SetBacking(xmlType);
					dataTypeDictionary.Add(daDataType, t);
                }
            }
        }

        foreach (var datatype in dataTypeDictionary.Keys)
        {
            // check if measure is available
            if (datatype.EndsWith("MEASURE"))
            {
                if (!NonConvertibleMeasures.Contains(datatype) && !documentedMeasures.Any(x=>x.Name == datatype.ToUpperInvariant()))
                {
                    Program.Message($"Warning: dataType {datatype} is missing in documentation", ConsoleColor.DarkYellow);
                    Debug.WriteLine(datatype);
                }
            }
        }


        var source = stub;
        var sbMeasures = new StringBuilder();
        foreach (var clNm in dataTypeDictionary.Keys.OrderBy(x => x))
        {
            var fnd = dataTypeDictionary[clNm];		
            if (fnd.Fields is not null)
            {
                var t = $"""new IfcMeasureInformation("{fnd.Fields[0]}","{fnd.Fields[1]}","{fnd.Fields[2]}","{fnd.Fields[3]}","{fnd.Fields[4]}","{fnd.Fields[5]}","{fnd.Fields[6]}")""";
                sbMeasures.AppendLine($"""            yield return new IfcDataTypeInformation("{clNm}", {CodeHelpers.NewStringArray(fnd.Schemas)}, {t}, "{fnd.XmlBackingType}");""");
            }
            else
                sbMeasures.AppendLine($"""            yield return new IfcDataTypeInformation("{clNm}", {CodeHelpers.NewStringArray(fnd.Schemas)}, "{fnd.XmlBackingType}");""");
        }
        source = source.Replace($"<PlaceHolderDataTypes>\r\n", sbMeasures.ToString());
        source = source.Replace($"<PlaceHolderVersion>", VersionHelper.GetFileVersion(typeof(ExpressMetaData)));
        
        return source;

    }

	private static string MapToXml(Type underlyingType)
	{
        switch (underlyingType.Name)
        {
            case "Int64":
				return "xs:integer";
			case "Double":
				return "xs:double"; 
            case "String":
				return "xs:string";
			case "Boolean":
				return "xs:boolean";
			case "Byte[]":
				return ""; // todo: what is this?
		}
		if (underlyingType.Name.EndsWith("Enum"))
            return "xs:string";
        if (underlyingType == typeof(Nullable<bool>))
            return "xs:string";
        // Debug.WriteLine($"{underlyingType.Name}");
        return "";
        
	}

	public static IEnumerable<string> NonConvertibleMeasures { get; } =
	[
		"IFCCONTEXTDEPENDENTMEASURE","IFCCOUNTMEASURE","IFCDESCRIPTIVEMEASURE","IFCMONETARYMEASURE","IFCNORMALISEDRATIOMEASURE","IFCNUMERICMEASURE","IFCPOSITIVERATIOMEASURE","IFCRATIOMEASURE"
    ];

    private static IEnumerable<string> GetExpressValues(ExpressMetaData metaD)
    {
        var HandledTypes = metaD.Types().Select(x => x.Name.ToUpperInvariant()).ToList();
        foreach (var className in HandledTypes)
        {
            var daType = metaD.ExpressType(className.ToUpperInvariant());
            var t = daType.Type.GetInterfaces().Select(x => x.Name).Contains("IExpressValueType");
            if (t && !daType.UnderlyingType.Name.StartsWith("List"))
            {
                 yield return className;
            }
        }
    }

    private static IEnumerable<string> GetEnumValueTypes(string schema)
    {
        System.Reflection.Module module = SchemaHelper.GetModule(schema);
        var tp2 = module.GetTypes().Where(x => !string.IsNullOrEmpty(x.BaseType?.Name) && x.BaseType.Name == "Enum").ToList();
        return tp2.Select(x => x.Name.ToUpperInvariant()).Where(x => x.EndsWith("ENUM") && x.StartsWith("IFC"));
    }

    private static IEnumerable<typeMetadata> GetDocumentationMeasures()
    {
        var markDown = File.ReadAllLines(@"buildingSMART\units.md");
        foreach (var line in markDown)
        {
            var modline = line.Trim(' ');
            var lineCells = modline.Split('|');
            if (lineCells.Length != 9)
                continue;
            var firstCell = lineCells[1].Trim();
            if (firstCell.Contains(' ') ||  firstCell.Contains('\t') || firstCell.Contains('-'))
                continue;

            var ret = new typeMetadata() {
                Name = firstCell,
                Exponents = lineCells[6].Trim(),
                Fields = lineCells.Skip(1).Take(7).Select(x => x.Trim()).ToArray(),
            };
            yield return ret;
        }
    }

    private const string stub = @"// <auto-generated/>
// This code was automatically generated with information from Xbim.Essentials <PlaceHolderVersion>.
// Any changes made to this file will be lost.

using System;
using System.Collections.Generic;

namespace IdsLib.IfcSchema;

public partial class SchemaInfo
{
    /// <summary>
    /// The names of dataType classes across all schemas.
    /// </summary>
    public static IEnumerable<IfcDataTypeInformation> AllDataTypes
    {
        get
        {
<PlaceHolderDataTypes>
        }
    }
}

";

}
