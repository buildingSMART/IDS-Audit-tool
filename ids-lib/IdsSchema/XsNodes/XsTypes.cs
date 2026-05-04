using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.XsNodes
{
	/// <summary>
	/// Utility class for XSD type management
	/// </summary>
	public static partial class XsTypes
	{
		internal static Status AuditStringValue(ILogger? logger, XsTypes.BaseTypes baseType, string value, IdsXmlNode node)
		{
			var ret = Status.Ok;
			switch (baseType)
			{
				case XsTypes.BaseTypes.Undefined: // notified in the the restriction already, do nothing here
				case XsTypes.BaseTypes.Invalid: // notified in the the restriction already, do nothing here
				case XsTypes.BaseTypes.XsString:
					break;                          // nothing to do
				case XsTypes.BaseTypes.XsAnyUri:
				case XsTypes.BaseTypes.XsBoolean:
				case XsTypes.BaseTypes.XsInteger:
				case XsTypes.BaseTypes.XsDouble:
				case XsTypes.BaseTypes.XsFloat:
				case XsTypes.BaseTypes.XsDecimal:
				case XsTypes.BaseTypes.XsDuration:
				case XsTypes.BaseTypes.XsDateTime:
				case XsTypes.BaseTypes.XsDate:
				case XsTypes.BaseTypes.XsTime:
					if (!XsTypes.IsValid(value, baseType))
						ret = IdsErrorMessages.Report305BadConstraintValue(logger, node, value, baseType);
					break;
				default:
					ret = IdsErrorMessages.Report501UnexpectedScenario(logger, $"type evaluation not implemented for `{baseType}`", node);
					break;
			}
			return ret;
		}

		/// <summary>
		/// Determines if a string value is compatible with a given type
		/// </summary>
		/// <param name="valueString">the string value to parse</param>
		/// <param name="base">The expected base type</param>
		/// <returns>TRUE if compatible, FALSE otherwise</returns>
		public static bool IsValid(string valueString, BaseTypes @base)
		{
			switch (@base)
			{
				case BaseTypes.XsInteger:
					return regexInteger.IsMatch(valueString);
				case BaseTypes.XsDouble:
				case BaseTypes.XsFloat:
				case BaseTypes.XsDecimal:
					return regexDouble.IsMatch(valueString);
				case BaseTypes.XsDate:
					return regexDate.IsMatch(valueString);
				case BaseTypes.XsTime:
					return regexTime.IsMatch(valueString);
				case BaseTypes.XsDateTime:
					return regexDateTime.IsMatch(valueString);
				case BaseTypes.XsDuration:
					return regexDuration.IsMatch(valueString);
				case BaseTypes.XsString:
					return true;
				case BaseTypes.Invalid:
					return false;
				case BaseTypes.Undefined:
					return false;
				case BaseTypes.XsBoolean:
					return regexBoolean.IsMatch(valueString);
				case BaseTypes.XsAnyUri: 
					return regexAnyUri.IsMatch(valueString);
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
				"xs:time" => BaseTypes.XsTime,
				"xs:anyUri" => BaseTypes.XsAnyUri,
				_ => BaseTypes.Invalid
			};
		}

		/// <summary>
		/// Utility function to evaluate the string representation from the enum
		/// </summary>
		/// <param name="baseType">the base value to represent</param>
		/// <returns>the representation string prefixed with "xs:".</returns>
		public static string GetStringFromEnum(BaseTypes baseType)
		{
			return baseType switch
			{
				BaseTypes.XsString => "xs:string",
				BaseTypes.XsBoolean => "xs:boolean",
				BaseTypes.XsInteger => "xs:integer",
				BaseTypes.XsDouble => "xs:double",
				BaseTypes.XsFloat => "xs:float",
				BaseTypes.XsDecimal => "xs:decimal",
				BaseTypes.XsDuration => "xs:duration",
				BaseTypes.XsDateTime => "xs:dateTime",
				BaseTypes.XsDate => "xs:date",
				BaseTypes.XsTime => "xs:time",
				BaseTypes.XsAnyUri => "xs:anyUri",
				_ => ""
			};
		}

		/// <summary>
		/// Returns an array of base types that are considered valid for use in XML schema definitions.
		/// </summary>
		/// <remarks>
		/// The returned base types include common XML schema types such as string, boolean, numeric types,
		/// date and time types, and URI. Use this method to determine which base types are supported when defining or
		/// validating XML schema elements.
		/// </remarks>
		/// <returns>An array of <see cref="BaseTypes"/> values representing the valid XML schema base types. 
		/// The array will contain only supported types and will not be null.</returns>
		public static BaseTypes[] GetValidBaseTypes() => [
			BaseTypes.XsString,
			BaseTypes.XsBoolean,
			BaseTypes.XsInteger,
			BaseTypes.XsDouble,
			BaseTypes.XsFloat,
			BaseTypes.XsDecimal,
			BaseTypes.XsDuration,
			BaseTypes.XsDateTime,
			BaseTypes.XsDate,
			BaseTypes.XsTime,
			BaseTypes.XsAnyUri
		];


		/// <summary>
		/// Returns the default empty value as a string for the specified base type.
		/// </summary>
		/// <remarks>Use this method to obtain a consistent default value for supported base types when initializing
		/// or resetting values.</remarks>
		/// <param name="requiredType">The base type for which to retrieve the default empty value.</param>
		/// <returns>A string representing the default empty value for the specified base type. Returns an empty string if the type is
		/// not recognized.</returns>
		public static string GetDefaultEmptyValue(BaseTypes requiredType)
		{
			return requiredType switch
			{
				BaseTypes.XsAnyUri => "https://web",
				BaseTypes.XsBoolean => "false",
				BaseTypes.XsDate => DateTimeOffset.Now.ToString("yyyy-MM-ddzzz"),
				BaseTypes.XsDateTime => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss'Z'"),
				BaseTypes.XsDouble => "0.0",
				BaseTypes.XsDuration => "PT0S",
				BaseTypes.XsInteger => "0",
				BaseTypes.XsString => "",
				BaseTypes.XsTime => DateTimeOffset.Now.ToString("HH:mm:sszzz"),
				_ => ""
			};
		}

		// todo: we might consider the use of fractionDigtits for value checking:
		// compute → normalize (round/scale) → serialize → validate (fractionDigits)
		// this would require to store the fractionDigits value in the restriction, 

		/// <summary>
		/// Specifies the set of XML Schema definition (XSD) facets that can be applied to specify values by constraint of lexical space or values space, or their description
		/// </summary>
		/// <remarks>
		/// Use this enumeration to identify which facets are allowed for a given XSD simple type when
		/// defining or validating XML schemas. Each member corresponds to a specific constraint, such as limiting the range,
		/// length, or pattern of values. These facets are defined by the W3C XML Schema specification and are commonly used
		/// to enforce data integrity in XML documents.
		/// </remarks>
		public enum XsdAllowedFacets
		{
			/// <summary>
			/// An annotation is information for human and/or mechanical consumers. 
			/// </summary>
			Annotation,
			/// <summary>
			/// constrains the value space to a specified set of values.
			/// </summary>
			Enumeration,
			/// <summary>
			/// length is the number of units of length, where units of length varies depending on the type that is being derived from. The value of length must be a nonNegativeInteger.
			/// </summary>
			Length,
			/// <summary>
			/// maxExclusive is the exclusive upper bound of the value space for a datatype with the ordered property. The value of maxExclusive  must be in the value space of the base type or be equal to {value} in {base type definition}.
			/// </summary>
			MaxExclusive,
			/// <summary>
			///  maxInclusive is the inclusive upper bound of the value space for a datatype with the ordered property. The value of maxInclusive must be in the value space of the base type.
			/// </summary>
			MaxInclusive,
			/// <summary>
			/// maxLength is the maximum number of units of length, where units of length varies depending on the type that is being derived from. The value of maxLength  must be a nonNegativeInteger. 
			/// </summary>
			MaxLength,
			/// <summary>
			/// minExclusive is the exclusive lower bound of the value space for a datatype with the ordered property. The value of minExclusive must be in the value space of the base type or be equal to {value} in {base type definition}.
			/// </summary>
			MinExclusive,
			/// <summary>
			/// minInclusive is the inclusive lower bound of the value space for a datatype with the ordered property. The value of minInclusive  must be in the value space of the base type.
			/// </summary>
			MinInclusive,
			/// <summary>
			/// minLength is the minimum number of units of length, where units of length varies depending on the type that is being derived from. The value of minLength  must be a nonNegativeInteger.
			/// </summary>
			MinLength,
			/// <summary>
			/// pattern is a constraint on the value space of a datatype which is achieved by constraining the lexical space to literals which match a specific pattern. The value of pattern  must be a regular expression.
			/// </summary>
			Pattern,
			/// <summary>
			/// fractionDigits controls the size of the minimum difference between values in the value space of datatypes derived from decimal, by restricting the value space to numbers that are expressible as i × 10^-n where i and n are integers and 0 &lt;= n &lt;= fractionDigits. The value of fractionDigits must be a nonNegativeInteger.
			/// </summary>
			FractionDigits,
			/// <summary>
			/// totalDigits controls the maximum number of values in the value space of datatypes derived from decimal, by restricting it to numbers that are expressible as i × 10^-n where i and n are integers such that |i| &lt; 10^totalDigits and 0 &lt;= n &lt;= totalDigits. The value of totalDigits must be a positiveInteger.
			/// </summary>
			TotalDigits,
		}

		/// <summary>
		/// Specifies the set of supported base data types for value representation and conversion operations.
		/// </summary>
		/// <remarks>This enumeration is typically used to indicate the type of a value parsed from a string or to
		/// describe the expected data type in serialization, deserialization, or schema validation scenarios. The values
		/// correspond to common XML Schema (xs:) base types, including string, boolean, numeric, date/time, and URI types.
		/// The 'Invalid' member indicates a failed conversion, while 'Undefined' represents an uninitialized or unspecified
		/// type.</remarks>
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
