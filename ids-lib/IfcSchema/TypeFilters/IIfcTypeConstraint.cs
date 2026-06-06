using System;
using System.Collections.Generic;

namespace IdsLib.IfcSchema.TypeFilters
{
	// todo: IIfcTypeConstraint need to be extended with inverse flag (to deal with prohibited facets)

	/// <summary>
	/// Represents a constraint on the types of an entity, such as the allowed types for an attribute or the types of 
	/// entities that can be related in a relationship. 
	/// The constraint is defined as a set of concrete types, represented as upper invariant strings. 
	/// The interface provides methods for intersecting and unioning constraints, as well as checking if a constraint is empty (i.e., has no allowed types).
	/// The standard empty constraint is the static <see cref="IfcTypeConcreteListConstraint.Empty"/>.
	/// </summary>
	public interface IIfcTypeConstraint
	{
		/// <summary>
		/// Concrete types as upper invariant strings
		/// </summary>
		IEnumerable<string> ConcreteTypes { get; }
		/// <summary>
		/// Returns a new constraint that represents the intersection of the types allowed by this constraint with another (equal or narrower set).
		/// </summary>
		IIfcTypeConstraint Intersect(IIfcTypeConstraint? other);
		/// <summary>
		/// Returns a new constraint that represents the union of the types allowed by this constraint with another (equal or wider set).
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		IIfcTypeConstraint Union(IIfcTypeConstraint? other);
		/// <summary>
		/// Indicates whether the constraint is empty, meaning that it does not allow any types. 
		/// An empty constraint represents an impossible condition where no entity can satisfy the constraint.
		/// </summary>
		bool IsEmpty { get; }
	}

	/// <summary>
	/// Helper class for the interpretation of nullable <see cref="IIfcTypeConstraint"/> instances.
	/// </summary>
	public static class IfcTypeConstraint
	{
		/// <summary>
		/// Returns a new constraint that represents the intersection of the types allowed by two constraints.
		/// When any of the parameters is null, it is treated as an unconstrained set of types 
		/// (i.e., it does not restrict the types allowed by the other constraint).
		/// </summary>
		public static IIfcTypeConstraint? Intersect(IIfcTypeConstraint? first, IIfcTypeConstraint? second)
		{
			if (first is null)
				return second;
			if (second is null) 
				return first;
			return first.Intersect(second);
		}

		/// <summary>
		/// Returns false if the constraint has no allowed types, meaning that it represents an impossible 
		/// condition where no entity can satisfy the constraint.
		/// 
		/// A null constraint is not considered impossible, as it represents an unconstrained 
		/// set of types rather than an empty set.
		/// </summary>
		public static bool CanBeMet(this IIfcTypeConstraint? constraint) => constraint is null || !constraint.IsEmpty;

		/// <summary>
		/// Returns true if the constraint has no allowed types, meaning that it represents an impossible 
		/// condition where no entity can satisfy the constraint.
		/// 
		/// A null constraint is not considered impossible, as it represents an unconstrained 
		/// set of types rather than an empty set.
		/// </summary>
		public static bool CannotBeMet(this IIfcTypeConstraint? constraint) => constraint is not null && constraint.IsEmpty;

	}
}
