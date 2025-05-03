using Microsoft.Extensions.Logging;

namespace IdsLib.IfcSchema
{
	/// <summary>
	/// Interface for unit information, used to provide metadata about units and their conversion behaviours.
	/// </summary>
	public interface IUnitInformation
	{
		/// <summary>
		/// If the is a conversion unit, this is the base unit resolved.
		/// </summary>
		/// <returns></returns>
		IUnitInformation? GetParentUnit();

		/// <summary>
		/// string representation of the IFC measure type, e.g. IFCAREAMEASURE
		/// </summary>
		string IfcMeasure { get; }
	}
}