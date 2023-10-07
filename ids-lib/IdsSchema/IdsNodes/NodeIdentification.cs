using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.IdsSchema.IdsNodes
{
	/// <summary>
	/// Provides a way to identify the element of the xml with line/poistion or relative index inside the IDS
	/// Instances of this class are passed as a parameter in the <see cref="ILogger"/> calls, and by default it presents the location by line and number.
	/// 
	/// A custom implementation of ILogger allows you to cast the received state parameter to IReadOnlyList&lt;KeyValuePair&lt;string, object&gt;&gt; and receive instances of NodeIdentification as parameters, to access the precise identifier.
	/// </summary>
	public class NodeIdentification : IFormattable
	{
		/// <summary>
		/// Provides positional identification of a node within and IDS via its hierarchy.
		/// Indices are 1-based to be user friendly. The hierarchy is separated by forward slash char.
		/// </summary>
		public string PositionalIdentifier { get; set; } = string.Empty;
		/// <summary>
		/// Line number in the xml file
		/// </summary>
		public int StartLineNumber { get; set; } = 0;
		/// <summary>
		/// Line position in the xml file
		/// </summary>
		public int StartLinePosition { get; set; } = 0;
		/// <summary>
		/// The localname of the XML node type
		/// </summary>
		public string NodeType { get; set; } = string.Empty;
		/// <inheritdoc />
		public override string ToString()
		{
			return $"'{NodeType}' element at line {StartLineNumber}, position {StartLinePosition}";
		}
		/// <summary>
		/// Formats the value of the current instance using the required format.
		/// </summary>
		/// <param name="format">the format required</param>
		/// <param name="formatProvider">an optional provider</param>
		/// <returns>a non null string</returns>
		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			return format switch
			{
				"p" => PositionalIdentifier,
				_ => ToString(),
			};
		}
	}
}
