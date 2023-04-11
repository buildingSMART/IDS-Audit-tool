using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IfcSchema.TypeFilters
{
    // todo: all IIfcTypeConstraint concrete classes need to be thoroughly be covered with unit tests

    // todo: IIfcTypeConstraint need to be extended with inverse flag (prohibited)

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

		public IIfcTypeConstraint Intersect(IIfcTypeConstraint? other)
		{
			if (other is null)
                return IfcConcreteTypeList.Empty;
            if (this.IsEmpty || other.IsEmpty)
                return IfcConcreteTypeList.Empty;
            return new IfcConcreteTypeList(
                this.ConcreteTypes.Intersect(other.ConcreteTypes)
                );
        }

        // todo: this is a bit of a hack at the moment, there is probably a more efficient way
        // for example a null constraint could mean no constraint, but  
        // but that needs to be reflected in the property of the interface and in the intersect logic
        //
        internal const string SpecialTopClassName = "*";

        internal static IfcConcreteTypeList FromTopClass(SchemaInfo schema, string topClassName)
        {
            if (topClassName == SpecialTopClassName)
            {
                // special case for no filter at all
                var allConcreteNames = schema.Where(x => x.Type == ClassType.Concrete).Select(y => y.Name);
                return new IfcConcreteTypeList(allConcreteNames);
            }
            var topClass = schema[topClassName.ToUpperInvariant()];
            if (topClass == null)
                return Empty;
            return new IfcConcreteTypeList(topClass.MatchingConcreteClasses.Select(x => x.Name));
        }
    }
}
