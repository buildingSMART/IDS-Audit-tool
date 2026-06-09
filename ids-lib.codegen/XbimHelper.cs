using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xbim.Common;
using Xbim.Common.Metadata;

namespace IdsLib.codegen
{
	internal static class XbimHelper
	{
		public static string GetTypeName(string type)
		{
			if (type.StartsWith("Ifc"))
				return type.Substring(3);
			return type;
		}

		internal static string GetExpressTypeDefinition(ExpressMetaProperty expressProp)
		{
			if (expressProp.EnumerableType is not null)
			{
				string ent = GetExpressEnumerable(expressProp.EntityAttribute);
				return $"{ent}{expressProp.EnumerableType.Name}";
			}
			if (expressProp.PropertyInfo is not null)
			{
				return GetPropertyType(expressProp.PropertyInfo);
			}
			return "";
		}

		private static string GetPropertyType(System.Reflection.PropertyInfo propertyInfo)
		{
			StringBuilder sb = new StringBuilder();
			var att = propertyInfo.GetCustomAttributes(typeof(EntityAttributeAttribute), false).FirstOrDefault() as EntityAttributeAttribute;
			if (att is not null)
			{
				if (att.IsOptional)
					sb.Append("OPTIONAL ");
			}
			var type = propertyInfo.PropertyType;
			type = Nullable.GetUnderlyingType(type) ?? type;
			string typeName = type.Name;
			if (typeName.Contains("`1"))
			{
				throw new NotImplementedException($"Generic types not supported for now, got {typeName}");
			}
			sb.Append(typeName);
			return sb.ToString();
		}

		private static string GetExpressEnumerable(EntityAttributeAttribute? entityAttribute)
		{
			
			var sb = new StringBuilder();
			if (entityAttribute is not null)
			{
				if (entityAttribute.IsMandatory)
					sb.Append("OPTIONAL ");
				if (entityAttribute.MinCardinality.Count() != entityAttribute.MaxCardinality.Count())
				{
					throw new NotImplementedException();
				}
				for (int i = 0; i < entityAttribute.MinCardinality.Count(); i++)
				{
					if (i > 0)
					{
						// todo: this is not precise, because the type of the inner list can be different,
						// but for now we do not have metadata available in the Xbim library to determine this, so we assume it is the same as the outer list
					}
					var minC = entityAttribute.MinCardinality[i];
					var maxC = entityAttribute.MaxCardinality[i];
					sb.Append($"{FixSetname(entityAttribute.ListType)} [{FixCardinality(minC)}:{FixCardinality(maxC)}] OF ");
				}
			}
			return sb.ToString();

		}

		private static string FixSetname(string listType)
		{
			return listType switch
			{
				"set" => "SET",
				"list" => "LIST",
				"list-unique" => "UNIQUE LIST",
				_ => "ARRAY",
			};
		}

		private static string FixCardinality(int v)
		{
			if (v == -1)
				return "?";
			return v.ToString();
		}

		internal static IEnumerable<ExpressMetaProperty> GetRelevantProperties(this ExpressType expressSource)
		{
			// if (prop.IsInverse || prop.IsDerived)
#pragma warning disable RS0030 // Do not use banned APIs, this is the legitimate way to centralise the use of the API
			return expressSource.Properties.Values.Where(
				x => !x.IsInverse && !x.IsDerived &&
				(
					x.PropertyInfo is not null || x.EnumerableType is not null
				));
#pragma warning restore RS0030 // Do not use banned APIs
		}
	}
}
