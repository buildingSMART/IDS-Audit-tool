using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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

		[Flags]
		internal enum IncludeBuildParameters
		{
			None = 0,
			AddOptionalDescriptor = 1 << 0,
			AddEnumerableDescriptor = 1 << 1,
			ProcessEnumerables = 1 << 2, 
			ProcessSingleValues = 1 << 3, 
			All = (ProcessSingleValues << 1) - 1,
			ExcludeOptionalDescriptor = All & ~AddOptionalDescriptor,
		}

		internal static Type? ResolveType(this ExpressMetaProperty? expressProp)
		{
			if (expressProp is null)
				return null;
			if (expressProp.PropertyInfo is not null)
			{
#pragma warning disable RS0030 // Do not use banned APIs
				var type = expressProp.PropertyInfo.PropertyType;
#pragma warning restore RS0030 // Do not use banned APIs
				type = Nullable.GetUnderlyingType(type) ?? type;
				return type;
			}
			throw new NotImplementedException();
		}

		internal static string? GetExpressTypeDefinition(this ExpressMetaProperty? expressProp, IncludeBuildParameters buildItems = IncludeBuildParameters.All)
		{
			if (expressProp is null)
				return null;
			if (expressProp.EnumerableType is not null)
			{
				if (!buildItems.HasFlag(IncludeBuildParameters.ProcessEnumerables))
					return null;
				string ent = GetExpressEnumerable(expressProp.EntityAttribute, buildItems);
				return $"{ent}{expressProp.EnumerableType.Name}";
			}
			if (expressProp.PropertyInfo is not null)
			{
				if (!buildItems.HasFlag(IncludeBuildParameters.ProcessSingleValues))
					return null;
				return GetPropertyType(expressProp.PropertyInfo, buildItems);
			}
			return null;
		}

		private static string GetPropertyType(System.Reflection.PropertyInfo propertyInfo, IncludeBuildParameters buildItems)
		{
			StringBuilder sb = new StringBuilder();
			var att = propertyInfo.GetCustomAttributes(typeof(EntityAttributeAttribute), false).FirstOrDefault() as EntityAttributeAttribute;
			if (att is not null)
			{
				if (att.IsOptional && buildItems.HasFlag(IncludeBuildParameters.AddOptionalDescriptor))
					sb.Append("OPTIONAL ");
			}
#pragma warning disable RS0030 // Do not use banned APIs
			var type = propertyInfo.PropertyType;
#pragma warning restore RS0030 // Do not use banned APIs
			type = Nullable.GetUnderlyingType(type) ?? type;
			string typeName = type.Name;
			if (typeName.Contains("`1"))
			{
				throw new NotImplementedException($"Generic types not supported for now, got {typeName}");
			}
			sb.Append(typeName);
			return sb.ToString();
		}

		private static string GetExpressEnumerable(EntityAttributeAttribute? entityAttribute, IncludeBuildParameters buildItems)
		{
			var sb = new StringBuilder();
			if (entityAttribute is not null)
			{
				if (entityAttribute.IsMandatory && buildItems.HasFlag(IncludeBuildParameters.AddOptionalDescriptor))
					sb.Append("OPTIONAL ");
				if (entityAttribute.MinCardinality.Count() != entityAttribute.MaxCardinality.Count())
				{
					throw new NotImplementedException();
				}
				if (buildItems.HasFlag(IncludeBuildParameters.AddEnumerableDescriptor))
				{
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
