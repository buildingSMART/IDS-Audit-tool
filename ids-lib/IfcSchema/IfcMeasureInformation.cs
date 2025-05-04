using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata about measure conversion behaviours.
/// </summary>
[DebuggerDisplay("{Id}, {Description}")]
public record IfcMeasureInformation : IUnitInformation
{
	/// <summary>
	/// Complete constructor
	/// </summary>
	public IfcMeasureInformation(string measureId, string description, string unit, string unitSymbol, string defDisplayUnit, string exponents, string unitTypeEnum, string[] siUnitNameEnum)
		: this(measureId, description, unit, unitSymbol, defDisplayUnit, exponents, unitTypeEnum)
	{
		SiUnitNameEnums = siUnitNameEnum;
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
	/// Checks if the measure has no unit components.
	/// </summary>
	public bool IsPureNumber => Exponents.IsPureNumber;

	/// <summary>
	/// Checks if the measure is a named SI unit which is not a pure number (e.g. excludes rad, db, m2).
	/// </summary>
	public bool IsDirectSIUnit => !string.IsNullOrEmpty(UnitSymbol) && !Exponents.IsPureNumber;

	/// <summary>
	/// Checks if the measure is one of the 7 basic SI unit.
	/// </summary>
	public bool IsBasicUnit { get; }

	/// <summary>
	/// The string ID found in the XML persistence, currently identical to the <see cref="IfcMeasure"/>
	/// </summary>
	public string Id { get; }
	/// <summary>
	/// String of the Ifc type expected, e.g. IFCAREAMEASURE
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
	public string GetUnit()
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

	/// <inheritdoc/>
	public IUnitInformation? GetParentUnit()
	{
		return null;
	}

	/// <summary>
	/// The range of characters that are valid in SI unit names. This includes the Greek letters mu and omega, and the degree symbol.
	/// </summary>
	public static string SiUnitNameCharactersRange = "a-zA-ZμΩ°";

	/// <summary>
	/// The range of characters that reasonable in unit names. This includes the SI unit names expanded with double and single quotes for feet and inch.
	/// </summary>
	public static string BroadUnitNameCharactersRange = """a-zA-ZμΩ°"'""";

	/// <summary>
	/// Standard regex catcher for SI unit components, such as mm, m2, mm-2 or Gy 
	/// </summary>
	public static Regex SiUnitComponentMatcher => new(@"^[\s]*(?<chars>[a-zA-ZμΩ°]+)[\s]*(?<pow>[+-]?[\d]*)[\s]*$", RegexOptions.Compiled);

	/// <summary>
	/// Standard regex catcher for unit components, such as mm, m2, or Gy, but expanded to match " and ' for inch and feet
	/// </summary>
	public static Regex BroadUnitComponentMatcher => new("""^[\s]*(?<chars>[a-zA-ZμΩ°"']+)[\s]*(?<pow>[+-]?[\d]*)[\s]*$""", RegexOptions.Compiled);

	/// <summary>
	/// Tries to parse a string into a SI unit name, and returns the relevant exponents and SI prefix multiplier.
	/// </summary>
	public static bool TryGetSIUnitFromString(string val, [NotNullWhen(true)] out DimensionalExponents? exponents, out IfcConversionUnitInformation.SiPrefix siPrefix, out int pow)
	{
		var m = SiUnitComponentMatcher.Match(val);
		if (!m.Success)
		{
			exponents = null;
			siPrefix = IfcConversionUnitInformation.SiPrefix.NONE;
			pow = 0;
			return false;
		}
		var tUnit = GetUnit(m.Groups["chars"].Value);
		exponents = DimensionalExponents.FromString(tUnit.exponents);
		siPrefix = tUnit.siPrefix;
		var tPow = m.Groups["pow"].Value;
		if (string.IsNullOrEmpty(tPow))
			pow = 1;
		else if (!int.TryParse(tPow, out pow))
			pow = 0;
		return exponents is not null;
	}

	private static (string exponents, IfcConversionUnitInformation.SiPrefix siPrefix) GetUnit(string unitName)
	{
		return unitName switch
		{
			"EGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"Gy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aGy" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"Emol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"Pmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"Tmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"Gmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"Mmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"damol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"mol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fmol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"amol" => ("(0, 0, 0, 0, 0, 1, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"ESv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"Sv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aSv" => ("(2, 0, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"F" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aF" => ("(-2, 1, 4, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"C" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aC" => ("(0, 0, 1, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"ES" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"S" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aS" => ("(-2, -1, 3, 2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"A" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aA" => ("(0, 0, 0, 1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"Ω" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aΩ" => ("(2, 1, -3, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"V" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aV" => ("(2, 1, -3, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"J" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aJ" => ("(2, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"N" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aN" => ("(1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"THz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"Hz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aHz" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"Elx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.EXA),
			"Plx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.PETA),
			"Tlx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.TERA),
			"Glx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"Mlx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"klx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hlx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"dalx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.DECA),
			"lx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dlx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.DECI),
			"clx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mlx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μlx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nlx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.NANO),
			"plx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.PICO),
			"flx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"alx" => ("(-2, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"H" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aH" => ("(2, 1, -2, -2, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"Em" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"Pm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"Tm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"Gm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"Mm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"km" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"dam" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"m" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fm" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"am" => ("(1, 0, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"Elm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.EXA),
			"Plm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.PETA),
			"Tlm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.TERA),
			"Glm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"Mlm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"klm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hlm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"dalm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.DECA),
			"lm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dlm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.DECI),
			"clm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mlm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μlm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nlm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.NANO),
			"plm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.PICO),
			"flm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"alm" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"Ecd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.EXA),
			"Pcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.PETA),
			"Tcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.TERA),
			"Gcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"Mcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"dacd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.DECA),
			"cd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.DECI),
			"ccd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"ncd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fcd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"acd" => ("(0, 0, 0, 0, 0, 0, 1)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"ET" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"T" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aT" => ("(0, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"Wb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aWb" => ("(2, 1, -2, -1, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"Ekg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"Pkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"Tkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"Gkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"Mkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"dakg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"kg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"ckg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fkg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"akg" => ("(0, 1, 0, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"Pa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aPa" => ("(-1, 1, -2, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"W" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aW" => ("(2, 1, -3, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"Bq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aBq" => ("(0, 0, -1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"EK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"E°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"PK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"P°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"TK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"T°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"GK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"G°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"MK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"M°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"kK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"k°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"h°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"daK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"da°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"dK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"d°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"c°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"mK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"m°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"μ°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"nK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"n°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"pK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"p°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"f°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"aK" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"a°K" => ("(0, 0, 0, 0, 1, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),
			"Es" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.EXA),
			"Ps" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PETA),
			"Ts" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.TERA),
			"Gs" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.GIGA),
			"Ms" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MEGA),
			"ks" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.KILO),
			"hs" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.HECTO),
			"das" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECA),
			"s" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NONE),
			"ds" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.DECI),
			"cs" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.CENTI),
			"ms" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MILLI),
			"μs" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.MICRO),
			"ns" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.NANO),
			"ps" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.PICO),
			"fs" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.FEMTO),
			"as" => ("(0, 0, 1, 0, 0, 0, 0)", IfcConversionUnitInformation.SiPrefix.ATTO),

			_ => ("", IfcConversionUnitInformation.SiPrefix.NONE)
		};
	}
}
