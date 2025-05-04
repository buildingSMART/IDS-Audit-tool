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
	public partial record IfcConversionUnitInformation : IUnitInformation
	{
		/// <summary>
		/// Converts the given SI prefix to its corresponding multiplier.
		/// </summary>
		/// <param name="prefix">the SI prefix</param>
		/// <returns>the associated multiplication factor</returns>
		public static double SiMultiplier(SiPrefix prefix)
		{
			return prefix switch
			{
				SiPrefix.EXA => 1e+18,
				SiPrefix.PETA => 1e+15,
				SiPrefix.TERA => 1e+12,
				SiPrefix.GIGA => 1e+9,
				SiPrefix.MEGA => 1e+6,
				SiPrefix.KILO => 1e+3,
				SiPrefix.HECTO => 1e+2,
				SiPrefix.DECA => 1e+1,
				SiPrefix.NONE => 1e+0,
				SiPrefix.DECI => 1e-1,
				SiPrefix.CENTI => 1e-2,
				SiPrefix.MILLI => 1e-3,
				SiPrefix.MICRO => 1e-6,
				SiPrefix.NANO => 1e-9,
				SiPrefix.PICO => 1e-12,
				SiPrefix.FEMTO => 1e-15,
				SiPrefix.ATTO => 1e-18,
				_ => 1
			};
		}

		/// <summary>
		/// Converts the given SI prefix to its corresponding short string (e.g. k for KILO)
		/// </summary>
		/// <param name="prefix">the SI prefix</param>
		/// <returns>the associated short string prefix (e.g. k, m, da)</returns>
		public static string ShortStringPrefix(SiPrefix prefix)
		{
			return prefix switch
			{
				SiPrefix.EXA => "E",
				SiPrefix.PETA =>  "P",
				SiPrefix.TERA =>  "T",
				SiPrefix.GIGA =>  "G",
				SiPrefix.MEGA =>  "M",
				SiPrefix.KILO =>  "k",
				SiPrefix.HECTO => "h",
				SiPrefix.DECA =>  "da",
				SiPrefix.NONE => "",
				SiPrefix.DECI =>  "d",
				SiPrefix.CENTI => "c",
				SiPrefix.MILLI => "m",
				SiPrefix.MICRO => "μ",
				SiPrefix.NANO =>  "n",
				SiPrefix.PICO =>  "p",
				SiPrefix.FEMTO => "f",
				SiPrefix.ATTO => "a",
				_ => ""
			};
		}

		/// <summary>
		/// Converts the given SI prefix string to its corresponding enumeration (e.g. k to KILO)
		/// </summary>
		/// <param name="prefix">the SI prefix string</param>
		/// <returns>the associated enum</returns>
		public static SiPrefix PrefixFromShortString(string prefix)
		{
			return prefix switch
			{
				"E" => SiPrefix.EXA,
				"P" => SiPrefix.PETA,
				"T" => SiPrefix.TERA,
				"G" => SiPrefix.GIGA,
				"M" => SiPrefix.MEGA,
				"k" => SiPrefix.KILO,
				"h" => SiPrefix.HECTO,
				"da" => SiPrefix.DECA,
				"" => SiPrefix.NONE,
				"d" => SiPrefix.DECI,
				"c" => SiPrefix.CENTI,
				"m" => SiPrefix.MILLI,
				"μ" => SiPrefix.MICRO,
				"n" => SiPrefix.NANO,
				"p" => SiPrefix.PICO,
				"f" => SiPrefix.FEMTO,
				"a" => SiPrefix.ATTO,
				_ => SiPrefix.NONE
			};
		}

		/// <summary>
		/// Converts the given SI prefix to its corresponding long string (e.g. kilo for KILO)
		/// </summary>
		/// <param name="prefix">the SI prefix</param>
		/// <returns>the associated short string prefix (e.g. kilo, mimmi, deka)</returns>
		public static string LongStringPrefix(SiPrefix prefix)
		{
			return prefix switch
			{
				SiPrefix.EXA => "exa",
				SiPrefix.PETA => "peta",
				SiPrefix.TERA => "tera",
				SiPrefix.GIGA => "giga",
				SiPrefix.MEGA => "mega",
				SiPrefix.KILO => "kilo",
				SiPrefix.HECTO => "hecto",
				SiPrefix.DECA => "deca",
				SiPrefix.NONE => "",
				SiPrefix.DECI => "deci",
				SiPrefix.CENTI => "centi",
				SiPrefix.MILLI => "milli",
				SiPrefix.MICRO => "micro",
				SiPrefix.NANO => "nano",
				SiPrefix.PICO => "pico",
				SiPrefix.FEMTO => "femto",
				SiPrefix.ATTO => "atto",
				_ => ""
			};
		}

		/// <summary>
		/// Defines multiples and submultiples of the SI units.
		/// </summary>
		public enum SiPrefix
		{
			/// <summary>
			/// 1e+18
			/// </summary>
			EXA,
			/// <summary>
			/// 1e+15
			/// </summary>
			PETA,
			/// <summary>
			/// 1e+12
			/// </summary>
			TERA,
			/// <summary>
			/// 1e+9
			/// </summary>
			GIGA,
			/// <summary>
			/// 1e+6
			/// </summary>
			MEGA,
			/// <summary>
			/// 1e+3
			/// </summary>
			KILO,
			/// <summary>
			/// 1e+2
			/// </summary>
			HECTO,
			/// <summary>
			/// 1e+1
			/// </summary>
			DECA,
			/// <summary>
			/// 1e+0
			/// </summary>
			NONE,
			/// <summary>
			/// 1e-1
			/// </summary>		
			DECI,
			/// <summary>
			/// 1e-2
			/// </summary>
			CENTI,
			/// <summary>
			/// 1e-3
			/// </summary>
			MILLI,
			/// <summary>
			/// 1e-6
			/// </summary>
			MICRO,
			/// <summary>
			/// 1e-9
			/// </summary>
			NANO,
			/// <summary>
			/// 1e-12
			/// </summary>
			PICO,
			/// <summary>
			/// 1e-15
			/// </summary>
			FEMTO,
			/// <summary>
			/// 1e-18
			/// </summary>
			ATTO,
		}

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
