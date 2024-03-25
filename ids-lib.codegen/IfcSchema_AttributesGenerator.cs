using System.Diagnostics;
using System.Text;
using Xbim.Common.Metadata;
using Xbim.IO.Esent;
using Xbim.Properties;

namespace IdsLib.codegen;

class IfcSchema_AttributesGenerator
{
    private class IfcAttribute
    {
		public IfcAttribute(string name, string owningClass , string tp)
		{
            Name = name;
            AddClass(owningClass);
            AddBase(tp);
		}

		public string Name { get; init; }
		public List<string> XmlBaseTypes { get; set; } = [];
        public List<string> ClassesDefining { get; init; } = [];

		/// <summary>
		/// The IFC type of the backing types
		/// </summary>
        public List<string> IfcBaseTypes { get; init; } = [];

		internal void AddClass(string owningClass)
		{
			ClassesDefining.Add(owningClass.ToUpperInvariant());
		}

		internal void AddBase(string tp)
		{
			if (!IfcBaseTypes.Contains(tp))
			{
				IfcBaseTypes.Add(tp);
			}
		}

		internal bool TrySetXmlBase(Dictionary<string, typeMetadata> dataTypeDictionary)
		{
			var xmlBases = IfcBaseTypes.Select(x => GetXmlBase(x, dataTypeDictionary)).Distinct();
			if (!xmlBases.Any()) 		
				throw new Exception("Invalid measure detected");
			XmlBaseTypes = xmlBases.ToList();
			XmlBaseTypes.Remove("");
			return true;
		}

		private static string GetXmlBase(string x, Dictionary<string, typeMetadata> dataTypeDictionary)
		{
			switch (x)
			{
				case "": // disabled
				case "IfcCompoundPlaneAngleMeasure": // disabled
					return ""; 
				case "Int64":
					return "xs:integer";
				case "Boolean":
					return "xs:boolean";
				case "Double":
					return "xs:double";
				case "IfcDimensionExtentUsage": // enum
				case "IfcSIUnitName":
				case "IfcSIPrefix": // enum
				case "IfcSurfaceSide": // enum
				case "IfcTextPath": // enum
				case "IfcBSplineCurveForm": // enum
				case "IfcTransitionCode": // enum
				case "IfcTrimmingPreference": // enum
				case "IfcAheadOrBehind": // enum
				case "IfcKnotType": // enum
				case "IfcBSplineSurfaceForm": // enum
				case "IfcPreferredSurfaceCurveRepresentation": // enum
				case "IfcTransitionCurveType": // enum
				case "IfcBooleanOperator": // enum
					return "xs:string";
			}
			if (!dataTypeDictionary.TryGetValue(x.ToUpperInvariant(), out var dt))
			{
				Debug.WriteLine($"Invalid attribute measure: `{x}` not found.");
				return "";
			}
			return dt.XmlBackingType;
		}

	}

    /// <summary>
    /// SchemaInfo.GeneratedAttributes.cs
    /// </summary>
    static public string Execute(Dictionary<string, typeMetadata> dataTypeDictionary)
    {
        var source = stub;
        foreach (var schemaString in Program.schemas)
		{
			System.Reflection.Module module = SchemaHelper.GetModule(schemaString);
			var metaD = ExpressMetaData.GetMetadata(module);

			// create a dictionary that defines the attributes 
			var attributes = GetAttributes(metaD);
			foreach (var item in attributes.Values)
			{
				item.TrySetXmlBase(dataTypeDictionary);
			}
			var sb = BuildCode(metaD, attributes);
			source = source.Replace($"<PlaceHolder{schemaString}>\r\n", sb.ToString());
		}

		source = source.Replace($"<PlaceHolderVersion>", VersionHelper.GetFileVersion(typeof(ExpressMetaData)));
        return source;


    }

	private static StringBuilder BuildCode(ExpressMetaData metaD, Dictionary<string, IfcAttribute> owningTypesByAttribute)
	{
		var sb = new StringBuilder();

		// for each pair of attribute and class list
		foreach (var attrib in owningTypesByAttribute.Values)
		{
			var attribute = $"\"{attrib.Name}\"";
			// trying to remove all subclasses
			// initialize the list of classes to remove
			var toRemove = new HashSet<string>();
			var onlyTopClasses = attrib.ClassesDefining.ToArray().ToList(); // we start from the entire list, we later remove some
			for (int i = 0; i < onlyTopClasses.Count; i++)
			{
				var thisClassName = onlyTopClasses[i];
				var thisClass = metaD.ExpressType(thisClassName);

				foreach (var sub in thisClass.AllSubTypes)
				{
					if (!toRemove.Contains(sub.ExpressNameUpper))
					{
						toRemove.Add(sub.ExpressNameUpper);
					}
				}
			}

			var classesInQuotes = attrib.ClassesDefining.Select(x => $"\"{x}\"").ToArray();
			var XmlTypesInQuotes = attrib.XmlBaseTypes.Select(x => $"\"{x}\"").ToArray();
			
			var topClassesInQuotes = onlyTopClasses
				.Where(c => !toRemove.Contains(c))
				.Select(x => $"\"{x}\"").ToArray();

			var line = (attrib.XmlBaseTypes.Any())
				? $"\t\tdestinationSchema.AddAttribute({attribute}, new[] {{ {string.Join(", ", topClassesInQuotes)} }}, new[] {{ {string.Join(", ", classesInQuotes)} }}, new[] {{ {string.Join(", ", XmlTypesInQuotes)} }});"
				: $"\t\tdestinationSchema.AddAttribute({attribute}, new[] {{ {string.Join(", ", topClassesInQuotes)} }}, new[] {{ {string.Join(", ", classesInQuotes)} }});";


			sb.AppendLine(line);
		}

		return sb;
	}

	private static Dictionary<string, IfcAttribute> GetAttributes(ExpressMetaData metaD)
	{
		Dictionary<string, IfcAttribute> owningTypesByAttribute = new();

		foreach (var daType in metaD.Types())
		{
			foreach (var prop in daType.Properties.Values)
			{
				// we should skip derived and inverse
				if (prop.IsInverse || prop.IsDerived)
					continue; // no match
				var tp = Analyse(prop, metaD);
				//if (string.IsNullOrEmpty(tp))
				//	continue; // no value type
				
				// owning type
				if (owningTypesByAttribute.TryGetValue(prop.Name, out var lst))
				{
					lst.AddClass(daType.Name.ToUpperInvariant());
					lst.AddBase(tp);
				}
				else
				{
					owningTypesByAttribute.Add(prop.Name, new IfcAttribute(prop.Name, daType.Name, tp));
				}
			}
		}

		return owningTypesByAttribute;
	}

	private static string Analyse(ExpressMetaProperty prop, ExpressMetaData metaD)
	{
        var tp = prop.PropertyInfo.PropertyType;

        if (tp.IsValueType)
        {
            if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var t2 = prop.PropertyInfo.PropertyType.GetGenericArguments()[0];
                // Debug.WriteLine($"Prop: {prop.Name} : {t2}");
                return t2.Name;
            }
            // Debug.WriteLine($"Ok Prop: {prop.Name} : {tp.Name}");
            return tp.Name;
        }
        else
        {
            // var nm = GetFriendlyTypeName(tp);
            // Debug.WriteLine($"### Not ok 1 - Prop: {prop.Name} : {tp.Name}");
        }
        return "";
	}

	private const string stub = @"// generated code via ids-lib.codegen using Xbim.Essentials <PlaceHolderVersion> - any changes made directly here will be lost

using System;

namespace IdsLib.IfcSchema; 

public partial class SchemaInfo
{
	static partial void GetAttributesIFC2x3(SchemaInfo destinationSchema)
	{
<PlaceHolderIfc2x3>
	}

	static partial void GetAttributesIFC4(SchemaInfo destinationSchema)
	{
<PlaceHolderIfc4>
	}

    static partial void GetAttributesIFC4x3(SchemaInfo destinationSchema)
	{
<PlaceHolderIfc4x3>
	}
}
";
}