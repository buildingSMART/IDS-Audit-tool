using System.Collections.Generic;

namespace IdsLib.IfcSchema.TypeFilters
{
    internal interface IIfcTypeConstraint
	{
		/// <summary>
		/// Concrete types as upper invariant strings
		/// </summary>
		IEnumerable<string> ConcreteTypes { get; }
		IIfcTypeConstraint Intersect(IIfcTypeConstraint? other);
		bool IsEmpty { get; }
	}

	internal static class IfcTypeConstraint
	{
		public static IIfcTypeConstraint? Intersect(IIfcTypeConstraint? first, IIfcTypeConstraint? second)
		{
			if (first is null)
				return second;
			if (second is null) 
				return first;
			return first.Intersect(second);
		}


        public static bool IsNotNullAndEmpty(IIfcTypeConstraint? constraint)
        {
            if (constraint is null)
                return false;
            return constraint.IsEmpty;
        }


    }
}
