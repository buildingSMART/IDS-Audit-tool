namespace IdsLib.IfcSchema;

/// <summary>
/// Metadata about measure conversion behaviours.
/// </summary>
public readonly struct IfcMeasureInformation
{
	/// <summary>
	/// basic constructor
	/// </summary>
	public IfcMeasureInformation(string measure, string description, string unit, string symbol, string defDisplayUnit, string exponents, string unitTypeEnum)
	{
		Id = measure;
		IfcMeasure = measure;
		Description = description;
		Unit = unit;
		UnitSymbol = symbol;
		DefaultDisplay = symbol;
		Exponents = DimensionalExponents.FromString(exponents) ?? new DimensionalExponents();
		UnitTypeEnum = unitTypeEnum;
	}

	/// <summary>
	/// The string ID found in the XML persistence
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
	/// Symbol used to present the unit, e.g. Hz
	/// </summary>
	public string UnitSymbol { get; }
	/// <summary>
	/// Preferred representation unit, e.g. 1 / s
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
}
