using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IdsLib.IdsSchema.XsNodes
{
	/// <summary>
	/// Utility class for XSD type management
	/// </summary>
	public static class XsTypes
	{
		private readonly static Regex regexInteger = new(@"^[+-]?(\d+)$", RegexOptions.Compiled);
		private readonly static Regex regexFloating = new(@"^[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?$", RegexOptions.Compiled);
		private readonly static Regex regexDuration = new(@"^[-+]?P(\d+Y)?(\d+M)?(\d+D)?(T(\d+H)?(\d+M)?(\d+S)?)?$", RegexOptions.Compiled);
		private readonly static Regex regexDate = new(@"^\d{4}-\d{2}-\d{2}(Z|([+-]\d{2}:\d{2}))?$", RegexOptions.Compiled);
		private readonly static Regex regexTime = new(@"^\d{2}:\d{2}:\d{2}(\.\d+)?(Z|([+-]\d{2}:\d{2}))?$", RegexOptions.Compiled);
		private readonly static Regex regexDateTime = new(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|([+-]\d{2}:\d{2}))?$", RegexOptions.Compiled);

		/// <summary>
		/// Determines if a string value is compatible with a given type
		/// </summary>
		/// <param name="valueString">the string value to parse</param>
		/// <param name="base">The expected base type</param>
		/// <returns>TRUE if compatible, false otherwise</returns>
		public static bool IsValid(string valueString, BaseTypes @base)
		{ 
			switch(@base)
			{
				case BaseTypes.XsInteger:
					return regexInteger.IsMatch(valueString);
				case BaseTypes.XsDouble:
				case BaseTypes.XsFloat:
				case BaseTypes.XsDecimal:
					return regexFloating.IsMatch(valueString);
				case BaseTypes.XsDate:
					return regexDate.IsMatch(valueString);
				case BaseTypes.XsTime:
					return regexTime.IsMatch(valueString);
				case BaseTypes.XsDateTime:
					return regexDateTime.IsMatch(valueString);
				case BaseTypes.XsDuration:
					return regexDuration.IsMatch(valueString);
			}
			return false;
		}

		/// <summary>
		/// Utility function to evaluate the type enumeration from a string 
		/// </summary>
		/// <param name="baseAsString">the string value to parse</param>
		/// <returns>the resolved enumeration result.</returns>
		public static BaseTypes GetBaseFrom(string baseAsString)
		{
			return baseAsString switch
			{
				"" => BaseTypes.Undefined,
				"xs:string" => BaseTypes.XsString,
				"xs:boolean" => BaseTypes.XsBoolean,
				"xs:integer" => BaseTypes.XsInteger,
				"xs:double" => BaseTypes.XsDouble,
				"xs:float" => BaseTypes.XsFloat,
				"xs:decimal" => BaseTypes.XsDecimal,
				"xs:duration" => BaseTypes.XsDuration,
				"xs:dateTime" => BaseTypes.XsDateTime,
				"xs:date" => BaseTypes.XsDate,
				"xs:anyUri" => BaseTypes.XsAnyUri,
				_ => BaseTypes.Invalid
			};
		}

		/// <summary>
		/// 
		/// </summary>
		public enum BaseTypes
		{
			/// <summary>
			/// A an attempted conversion from string was unsuccesful
			/// </summary>
			Invalid,
			/// <summary>
			/// The value has not been assigned yet
			/// </summary>
			Undefined,
			/// <summary>
			/// String value
			/// </summary>
			XsString,
			/// <summary>
			/// Boiolean value
			/// </summary>
			XsBoolean,
			/// <summary>
			/// Integer value
			/// </summary>
			XsInteger,
			/// <summary>
			/// Double precision floating point value
			/// </summary>
			XsDouble,
			/// <summary>
			/// Floating point value 
			/// </summary>
			XsFloat,
			/// <summary>
			/// Decimal precision value
			/// </summary>
			XsDecimal,
			/// <summary>
			/// Time duration value
			/// </summary>
			XsDuration,
			/// <summary>
			/// Date and time value (with optional time zone offset)
			/// </summary>
			XsDateTime,
			/// <summary>
			/// Date only value (with optional time zone offset)
			/// </summary>
			XsDate,
			/// <summary>
			/// Time only value (with optional time zone offset)
			/// </summary>
			XsTime,
			/// <summary>
			/// URI value
			/// </summary>
			XsAnyUri,
		}

	}
}
