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
				case XsTypes.BaseTypes.XsAnyUri: // todo: implement Uri value filter
				case XsTypes.BaseTypes.Invalid: // notified in the the restriction already, do nothing here
				case XsTypes.BaseTypes.Undefined: // todo: ensure this is notified somewhere
				case XsTypes.BaseTypes.XsString:
					break;                          // nothing to do
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
				case BaseTypes.XsAnyUri: // todo: what is the regex for an URI?
					break;
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
