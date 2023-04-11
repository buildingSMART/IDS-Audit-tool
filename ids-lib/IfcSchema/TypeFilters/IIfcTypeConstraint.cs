using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
