using IdsLib.IfcSchema.TypeFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdsLib.IfcSchema
{
	public partial class SchemaInfo
	{
		/// <summary>
		/// Returns the set of the classes that match the provided type filter.
		/// </summary>
		/// <param name="typeFilter"></param>
		/// <returns></returns>
		public IEnumerable<ClassInfo> GetClassesByType(IIfcTypeConstraint typeFilter)
		{
			var concrete = typeFilter.ConcreteTypes.ToHashSet();
			return this.Where(x => typeFilter.ConcreteTypes.Contains(x.Name.ToUpperInvariant()));
		}

		/// <summary>
		/// Returns a distinct set of the attributes of the classes that match the provided type filter.
		/// </summary>
		public IEnumerable<AttributeInfo> GetAttributesByType(IIfcTypeConstraint typeFilter)
		{
			var clss = GetClassesByType(typeFilter).ToArray();
			return clss.SelectMany(x => x.DirectAttributesInfo ?? []).Distinct();
		}
	}
}
