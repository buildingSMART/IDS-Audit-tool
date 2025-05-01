using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata about measure conversion behaviours.
/// </summary>
[DebuggerDisplay("{Id}, {Description}")]
public readonly struct IfcMeasureInformation
{
	/// <summary>
	/// Empty measure information, used for default values.
	/// </summary>
	public static readonly IfcMeasureInformation Empty = new("", "", "", "", "", "", "", []);

	/// <summary>
	/// Complete constructor
	/// </summary>
	public IfcMeasureInformation(string measureId, string description, string unit, string unitSymbol, string defDisplayUnit, string exponents, string unitTypeEnum, string[] siUnitNameEnum)
		: this(measureId, description, unit, unitSymbol, defDisplayUnit, exponents, unitTypeEnum)
	{
		SiUnitNameEnums = siUnitNameEnum;
	}

	/// <summary>
	/// Checks if the measure information is empty.
	/// </summary>
	public bool IsEmpty
	{
		get
		{
			if (string.IsNullOrEmpty(Id))
				return true;
			if (string.IsNullOrEmpty(IfcMeasure))
				return true;
			return false;
		}
	}

	/// <summary>
	/// basic constructor
	/// </summary>
	public IfcMeasureInformation(string measureId, string description, string unit, string unitSymbol, string defDisplayUnit, string exponents, string unitTypeEnum)
	{
		Id = measureId;
		IfcMeasure = measureId;
		Description = description;
		Unit = unit;
		UnitSymbol = unitSymbol;
		DefaultDisplay = defDisplayUnit;
		Exponents = DimensionalExponents.FromString(exponents) ?? new DimensionalExponents();
		UnitTypeEnum = unitTypeEnum;
		IsBasicUnit = DimensionalExponents.UnitMeasures.Contains(Id);

	}

	/// <summary>
	/// Checks if the measure is a basic SI unit.
	/// </summary>
	public bool IsBasicUnit { get; }

	/// <summary>
	/// The string ID found in the XML persistence, currently identical to the <see cref="IfcMeasure"/>
	/// </summary>
	public string Id { get; }
	/// <summary>
	/// String of the Ifc type expected
	/// </summary>
	public string IfcMeasure { get; }
	/// <summary>
	/// A textual description, e.g. "Frequency"
	/// </summary>
	public string Description { get; }
	/// <summary>
	/// Full name of the unit, e.g. hertz
	/// </summary>
	public string Unit { get; }
	/// <summary>
	/// the string values of the SI unit name enums, if any are available
	/// </summary>
	public string[] SiUnitNameEnums { get; } = [];
	/// <summary>
	/// Symbol used to present the unit, e.g. Hz
	/// </summary>
	public string UnitSymbol { get; }
	/// <summary>
	/// Preferred representation unit. This could be either direct or derived, e.g. Ω, m4, or J / Kg K
	/// </summary>
	public string DefaultDisplay { get; }
	/// <summary>
	/// Dimensional exponents useful for conversion to other units.
	/// </summary>
	public DimensionalExponents Exponents { get; }
	/// <summary>
	/// The string value of the UnitType enum of a valid matching unit
	/// </summary>
	public string UnitTypeEnum { get; }

	/// <summary>
	/// Returns the SI preferred unit.
	/// </summary>
	/// <returns>empty string for measures that do not have expected measures</returns>
	public readonly string GetUnit()
	{
		if (!string.IsNullOrEmpty(Unit))
			return Unit;
		if (Exponents is not null)
			return Exponents.ToUnitSymbol();
		return "";
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="value">leave empty to search for any SI unit enum, otherwise specify a string like `IfcSIUnitName.KELVIN` or `KELVIN`</param>
	/// <returns></returns>
	public bool HasSiUnitEnum(string? value = null)
	{
		if (value is null)
			return SiUnitNameEnums.Any();
		return SiUnitNameEnums.Any(x => x.EndsWith(value));
	}
}
