using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace IdsLib.IfcSchema;

/// <summary>
/// One of the core SI units of measure
/// </summary>
public enum DimensionType
{
	/// <summary>
	/// Length
	/// </summary>
	Length,
	/// <summary>
	/// Mass
	/// </summary>
	Mass,
	/// <summary>
	/// Time
	/// </summary>
	Time,
	/// <summary>
	/// Electric Current
	/// </summary>
	ElectricCurrent,
	/// <summary>
	/// Temperature
	/// </summary>
	Temperature,
	/// <summary>
	/// Amount Of Substance
	/// </summary>
	AmountOfSubstance,
	/// <summary>
	/// Luminous Intensity
	/// </summary>
	LuminousIntensity,
}

/// <summary>
/// Supports conversion of measures from different forms of unit expression
/// </summary>
public class DimensionalExponents : IEquatable<DimensionalExponents>
{
	/// <summary>
	/// Dimensional exponent for the Length core SI unit of measure
	/// </summary>
	public int Length { get; set; } = 0;
	/// <summary>
	/// Dimensional exponent for the Mass core SI unit of measure
	/// </summary>
	public int Mass { get; set; } = 0;
	/// <summary>
	/// Dimensional exponent for the Time core SI unit of measure
	/// </summary>
	public int Time { get; set; } = 0;
	/// <summary>
	/// Dimensional exponent for the ElectricCurrent core SI unit of measure
	/// </summary>
	public int ElectricCurrent { get; set; } = 0;
	/// <summary>
	/// Dimensional exponent for the Temperature core SI unit of measure
	/// </summary>
	public int Temperature { get; set; } = 0;
	/// <summary>
	/// Dimensional exponent for the AmountOfSubstance core SI unit of measure
	/// </summary>
	public int AmountOfSubstance { get; set; } = 0;
	/// <summary>
	/// Dimensional exponent for the LuminousIntensity core SI unit of measure
	/// </summary>
	public int LuminousIntensity { get; set; } = 0;

	/// <summary>
	/// Provides the DimensionalExponents of the base SI units.
	/// </summary>
	/// <param name="tp">Enum expression of the unit being sought</param>
	/// <returns>the relevant exponent sequence.</returns>
	/// <exception cref="NotImplementedException">should never occurr</exception>
	public static DimensionalExponents GetUnit(DimensionType tp)
	{
		return tp switch
		{
			DimensionType.Length => new DimensionalExponents(1, 0, 0, 0, 0, 0, 0),
			DimensionType.Mass => new DimensionalExponents(0, 1, 0, 0, 0, 0, 0),
			DimensionType.Time => new DimensionalExponents(0, 0, 1, 0, 0, 0, 0),
			DimensionType.ElectricCurrent => new DimensionalExponents(0, 0, 0, 1, 0, 0, 0),
			DimensionType.Temperature => new DimensionalExponents(0, 0, 0, 0, 1, 0, 0),
			DimensionType.AmountOfSubstance => new DimensionalExponents(0, 0, 0, 0, 0, 1, 0),
			DimensionType.LuminousIntensity => new DimensionalExponents(0, 0, 0, 0, 0, 0, 1),
			_ => throw new NotImplementedException(),
		};
	}

	/// <summary>
	/// Finds the exponent of one of the base SI units.
	/// </summary>
	/// <param name="tp">The relevant unit to search for</param>
	/// <returns>an integer</returns>
	/// <exception cref="NotImplementedException">Should never occurr</exception>
	public int GetExponent(DimensionType tp)
	{
		return tp switch
		{
			DimensionType.Length => Length,
			DimensionType.Mass => Mass,
			DimensionType.Time => Time,
			DimensionType.ElectricCurrent => ElectricCurrent,
			DimensionType.Temperature => Temperature,
			DimensionType.AmountOfSubstance => AmountOfSubstance,
			DimensionType.LuminousIntensity => LuminousIntensity,
			_ => throw new NotImplementedException(),
		};
	}

	/// <inheritdoc />
	public override string ToString()
	{
		var asStringArray = ValuesAsArray().Select(x => x.ToString()).ToArray();
		return "(" + string.Join(", ", asStringArray) + ")";
	}

	/// <summary>
	/// Generates a new <see cref="DimensionalExponents"/> instance, elevating a starting one to the <paramref name="exponent"/> power.
	/// </summary>
	/// <param name="val">The base unit</param>
	/// <param name="exponent">the power to raise the base unit to</param>
	/// <returns>A new <see cref="DimensionalExponents"/> instance</returns>
	public static DimensionalExponents Elevated(DimensionalExponents val, int exponent)
	{
		return new DimensionalExponents
		(
			val.Length * exponent,
			val.Mass * exponent,
			val.Time * exponent,
			val.ElectricCurrent * exponent,
			val.Temperature * exponent,
			val.AmountOfSubstance * exponent,
			val.LuminousIntensity * exponent
		);
	}


	/// <summary>
	/// elevates  the current instance to the <paramref name="exponent"/> power.
	/// </summary>
	/// <param name="exponent">the power to raise the current instance to</param>
	public void Elevate(int exponent)
	{
		Length *= exponent;
		Mass *= exponent;
		Time *= exponent;
		ElectricCurrent *= exponent;
		Temperature *= exponent;
		AmountOfSubstance *= exponent;
		LuminousIntensity *= exponent;
	}

	/// <summary>
	/// Generates a new <see cref="DimensionalExponents"/> instance, resulting from the multiplication of the current instance with the <paramref name="other"/>.
	/// </summary>
	/// <returns>A new instance, not altering the current.</returns>
	public DimensionalExponents Multiply(DimensionalExponents other)
	{
		return new DimensionalExponents
			(
			Length + other.Length,
			Mass + other.Mass,
			Time + other.Time,
			ElectricCurrent + other.ElectricCurrent,
			Temperature + other.Temperature,
			AmountOfSubstance + other.AmountOfSubstance,
			LuminousIntensity + other.LuminousIntensity
			);
	}

	/// <summary>
	/// New dimensional exponent from a string.
	/// </summary>
	/// <param name="str">
	/// the string as a list of integers, separated by commas, optionally surrounded by round brackets
	/// order of integers in the string is:
	/// 1) length, 2)mass, 3) time, 4) electricCurrent, 5) temperature, 6) amountOfSsubstance, 7) luminousIntensity
	/// </param>
	/// <returns>Null if the string is invalid</returns>
	public static DimensionalExponents? FromString(string str)
	{
		str = str.Trim();
		str = str.Trim('(');
		str = str.Trim(')');
#if NETSTANDARD2_0
		var arr = str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
#else
		var arr = str.Split(new string[] { "," }, StringSplitOptions.TrimEntries);
#endif
		if (arr.Length != 7)
			return null;
		var res = new int[arr.Length];
		for (int i = 0; i < arr.Length; i++)
		{
			if (!int.TryParse(arr[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out res[i]))
				return null;
		}
		return new DimensionalExponents(
			res[0],
			res[1],
			res[2],
			res[3],
			res[4],
			res[5],
			res[6]
			);
	}

	/// <summary>
	/// Specifies exponent for all the base SI units involved; order of parameters is:
	/// 1) length, 2)mass, 3) time, 4) electricCurrent, 5) temperature, 6) amountOfSsubstance, 7) luminousIntensity
	/// </summary>
	public DimensionalExponents(
		int length,
		int mass,
		int time,
		int electricCurrent,
		int temperature,
		int amountOfSsubstance,
		int luminousIntensity
		)
	{
		Length = length;
		Mass = mass;
		Time = time;
		ElectricCurrent = electricCurrent;
		Temperature = temperature;
		AmountOfSubstance = amountOfSsubstance;
		LuminousIntensity = luminousIntensity;
	}

	/// <summary>
	/// Unitless exponents, all values are 0.
	/// </summary>
	public DimensionalExponents()
	{
	}

	/// <summary>
	/// short form of the SI Units referenced by the exponents
	/// </summary>
	static string[] Units { get; } = new string[]
	{
		"m", "kg", "s", "A", "K", "mol", "cd"
	};

	/// <summary>
	/// String expression of the combination of exponets in the SI <see cref="Units"/>.
	/// </summary>
	/// <returns></returns>
	public string ToUnitSymbol()
	{
		int[] asArray = ValuesAsArray();

		var numerator = GetMultiplier(asArray, true);
		var denominator = GetMultiplier(asArray, false);
		if (denominator == "1")
			return numerator;
		else return numerator + " / " + denominator;
	}

	private int[] ValuesAsArray()
	{
		return new[]
		{
			Length, Mass, Time, ElectricCurrent, Temperature, AmountOfSubstance, LuminousIntensity
		};
	}

	static private string GetMultiplier(int[] asArray, bool numerator)
	{
		List<string> vals = new();
		for (int i = 0; i < asArray.Length; i++)
		{
			if (asArray[i] == 0)
				continue;
			if (asArray[i] > 0 == numerator)
			{
				var abs = Math.Abs(asArray[i]);
				if (abs == 1)
					vals.Add(Units[i]);
				else
					vals.Add(Units[i] + abs);
			}
		}
		if (vals.Any())
			return string.Join(" ", vals.ToArray());
		else
			return "1";
	}

	/// <inheritdoc />
	public bool Equals(DimensionalExponents? other)
	{
		if (other is null)
			return false;
		return Length == other.Length
			&& Mass == other.Mass
			&& Time == other.Time
			&& ElectricCurrent == other.ElectricCurrent
			&& Temperature == other.Temperature
			&& AmountOfSubstance == other.AmountOfSubstance
			&& LuminousIntensity == other.LuminousIntensity;
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return Equals(obj as DimensionalExponents);
	}

#pragma warning disable IDE0070 // Use 'System.HashCode'
	/// <inheritdoc />
	public override int GetHashCode()
	{
		// system hashcode is not available in net20
		return (
			Length,
			Mass,
			Time,
			ElectricCurrent,
			Temperature,
			AmountOfSubstance,
			LuminousIntensity
			).GetHashCode();
	}
#pragma warning restore IDE0070 // Use 'System.HashCode'
}
