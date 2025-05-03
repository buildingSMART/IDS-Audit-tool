using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace IdsLib.IfcSchema
{
	/// <summary>
	/// Provides information to assemble conversion units, as per the documentation available on buidlingSMART's website.
	/// </summary>
	public record IfcConversionUnitInformation : IUnitInformation
	{
		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Join(", ", this.ConversionUnitNames);
		}

		/// <summary>
		/// base constructor
		/// </summary>
		private IfcConversionUnitInformation(string ifcMeasure, double conv, string baseUnit)
		{
			ConversionUnitNames = [];
			IfcMeasure = ifcMeasure;
			ConversionValue = conv;
			BaseUnit = baseUnit;
		}
		/// <summary>
		/// basic public constructor
		/// </summary>
		public IfcConversionUnitInformation(string name, string ifcMeasure, double conv, string baseUnit)
			: this(ifcMeasure, conv, baseUnit)
		{
			ConversionUnitNames = [name];
		}
		/// <summary>
		/// Full constructor
		/// </summary>
		public IfcConversionUnitInformation(string[] names, string ifcMeasure, double conv, string baseUnit, double? offset = null)
			: this(ifcMeasure, conv, baseUnit)
		{
			ConversionUnitNames = names;
			ConversionOffset = offset;
		}

		/// <summary>
		/// The name of the conversion unit
		/// </summary>
		public string[] ConversionUnitNames { get; }
		/// <summary>
		/// ratio of conversion
		/// </summary>
		public double ConversionValue { get; }
		/// <summary>
		/// conversion offset can be null, if not used
		/// </summary>
		public double? ConversionOffset { get; } = null;
		/// <summary>
		/// the base unit that the conversion refers to
		/// </summary>
		public string BaseUnit { get; }
		/// <summary>
		/// identifies the type of measure that the unit represents
		/// </summary>
		public string IfcMeasure { get; }

		/// <summary>
		/// Attempts the retrieval of a unit from the standard conversion units, including appropriate measure
		/// </summary>
		/// <param name="unitName">the name of the unit to search for</param>
		/// <param name="unit">the information souhgt, if found</param>
		/// <param name="ifcMeasure">the matching required measure type, which can be left empty if not known</param>
		/// <returns>true if found, false otherwise</returns>
		public static bool TryGetUnit(string unitName, [NotNullWhen(true)] out IUnitInformation? unit, string? ifcMeasure = null)
		{
			if (ifcMeasure is null)
				unit = SchemaInfo.StandardConversionUnits.FirstOrDefault(x =>
					x.ConversionUnitNames.Contains(unitName)
					);
			else
				unit = SchemaInfo.StandardConversionUnits.FirstOrDefault(x => 
					x.ConversionUnitNames.Contains(unitName)
					&& x.IfcMeasure == ifcMeasure
					);
			return unit != null;
		}
		/// <summary>
		/// Retrieves the parent unit of the conversion unit, if available.
		/// </summary>
		public IUnitInformation? GetParentUnit()
		{
			if (TryGetUnit(BaseUnit, out var tmp, IfcMeasure))
				return tmp;
			var fnd = SchemaInfo.AllMeasureInformation.FirstOrDefault(
				x => x.UnitSymbol == BaseUnit && x.IfcMeasure == IfcMeasure
				|| x.DefaultDisplay == BaseUnit && x.IfcMeasure == IfcMeasure && !string.IsNullOrEmpty(x.Unit)
				);
			return fnd;
		}
		/// <summary>
		/// Retrieves the entire conversion chain, starting from the current unit.
		/// </summary>
		public IEnumerable<IUnitInformation> GetConversionSteps()
		{
			yield return this;
			var parent = GetParentUnit();
			while (parent != null)
			{
				yield return parent;
				parent = parent.GetParentUnit();
			}
		}
	}
}
