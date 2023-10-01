using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IdsLib.IfcSchema.TypeFilters
{
	// todo: all IIfcTypeConstraint concrete classes need to be thoroughly be covered with unit tests

	// todo: IIfcTypeConstraint need to be extended with inverse flag (prohibited)

	[DebuggerDisplay("{ConreteTypesCount} types")]
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

		internal int ConreteTypesCount => ConcreteTypes.Count();


		public bool IsEmpty => !upperInvariantTypeNames.Any();

		public IIfcTypeConstraint Intersect(IIfcTypeConstraint? other)
		{

			if (other is null)
                return this;
            if (this.IsEmpty || other.IsEmpty)
                return Empty;
            return new IfcConcreteTypeList(
                this.ConcreteTypes.Intersect(other.ConcreteTypes)
                );
        }

		public IIfcTypeConstraint Union(IIfcTypeConstraint? other)
		{
			if (other is null)
				return this;
			if (other.IsEmpty)
				return this;
			if (this.IsEmpty)
				return other;
			return new IfcConcreteTypeList(
				ConcreteTypes.Union(other.ConcreteTypes)
				);
		}

		internal static IfcConcreteTypeList FromTopClass(SchemaInfo schema, string topClassName)
        {
            var topClass = schema[topClassName.ToUpperInvariant()];
            if (topClass == null)
                return Empty;
            return new IfcConcreteTypeList(topClass.MatchingConcreteClasses.Select(x => x.Name));
        }
    }
}
