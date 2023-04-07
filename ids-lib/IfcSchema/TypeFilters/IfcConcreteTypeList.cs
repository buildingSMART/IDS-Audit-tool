using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IfcSchema.TypeFilters
{
    // todo: all IIfcTypeConstraint concrete classes need to be thoroughly be covered with unit tests
    internal class IfcConcreteTypeList : IIfcTypeConstraint
	{
		private static readonly IfcConcreteTypeList empty = new(Enumerable.Empty<string>());

		public static IfcConcreteTypeList Empty => empty;

		private readonly List<string> upperInvariantTypeNames;

		public IfcConcreteTypeList(IEnumerable<string> concreteTypeCollection)
		{
			upperInvariantTypeNames = new List<string>(concreteTypeCollection.Select(x=>x.ToUpperInvariant()));
		}

		public IEnumerable<string> ConcreteTypes => upperInvariantTypeNames;

		public bool IsEmpty => !upperInvariantTypeNames.Any();

		public IIfcTypeConstraint Intersect(IIfcTypeConstraint other)
		{
            if (this.IsEmpty || other.IsEmpty)
                return IfcConcreteTypeList.Empty;
            return new IfcConcreteTypeList(
                this.ConcreteTypes.Intersect(other.ConcreteTypes)
                );
        }

        internal static IfcConcreteTypeList FromTopClass(SchemaInfo schema, IfcClassInformation topClass)
        {
			var t = schema[topClass.PascalCaseName];
			if (t == null) 
				return Empty;
			return new IfcConcreteTypeList(t.MatchingConcreteClasses.Select(x=>x.Name));
        }
    }


}
