using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IdsLib.IfcSchema.TypeFilters
{
	/// <summary>
	/// Represents a type constraint defined by a specific list of concrete types.
	/// </summary>
	[DebuggerDisplay("{ConreteTypesCount} types")]
	public class IfcTypeConcreteListConstraint : IIfcTypeConstraint
	{
		private static readonly IfcTypeConcreteListConstraint empty = new([]);

		/// <summary>
		/// The default empty constraint, representing a constraint that does not allow any types.
		/// </summary>
		public static IfcTypeConcreteListConstraint Empty => empty;

		private readonly List<string> upperInvariantTypeNames;

		/// <summary>
		/// Default constructor, takes a collection of concrete type names, in any case, and initializes the constraint making them upper invariant.
		/// </summary>
		public IfcTypeConcreteListConstraint(IEnumerable<string> concreteTypeCollection)
		{
			upperInvariantTypeNames = new List<string>(concreteTypeCollection.Select(x=>x.ToUpperInvariant()));
		}

		/// <inheritdoc/>
		public IEnumerable<string> ConcreteTypes => upperInvariantTypeNames;

		internal int ConreteTypesCount => ConcreteTypes.Count();

		/// <inheritdoc/>
		public bool IsEmpty => !upperInvariantTypeNames.Any();

		/// <inheritdoc/>
		public IIfcTypeConstraint Intersect(IIfcTypeConstraint? other)
		{
			// a null constraint is no constraint, so the intersection with it is the current constraint
			if (other is null)
                return this;
			// if either constraint is empty, the intersection is empty
			if (this.IsEmpty || other.IsEmpty)
                return Empty;
            return new IfcTypeConcreteListConstraint(
                this.ConcreteTypes.Intersect(other.ConcreteTypes)
                );
        }

		/// <inheritdoc/>
		public IIfcTypeConstraint Union(IIfcTypeConstraint? other)
		{
			if (other is null)
				return this;
			if (other.IsEmpty)
				return this;
			if (this.IsEmpty)
				return other;
			return new IfcTypeConcreteListConstraint(
				ConcreteTypes.Union(other.ConcreteTypes)
				);
		}

		internal static IfcTypeConcreteListConstraint FromTopClass(SchemaInfo schema, string topClassName)
        {
            var topClass = schema[topClassName.ToUpperInvariant()];
            if (topClass == null)
                return Empty;
            return new IfcTypeConcreteListConstraint(topClass.MatchingConcreteClasses.Select(x => x.Name));
        }
    }
}
