using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib
{
	/// <summary>
	/// Provides an expressive way to present the collection of actions that the tool is performing
	/// </summary>
	public class ActionCollection: IFormattable
	{
		private List<Action> action = new List<Action>();

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Join(", ", action.Select(x=> ToFriendlyString(x)).ToArray());
		}

		/// <inheritdoc />
		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			return ToString();
		}

		internal void Add(Action idsStructure)
		{
			action.Add(idsStructure);
		}

		internal bool Any()
		{
			return action.Any();
		}

		/// <summary>
		/// Return the included actions
		/// </summary>
		public IEnumerable<Action> GetActions()
		{
			return action;
		}

		private string ToFriendlyString(Action x)
		{
			return x switch
			{
				Action.IdsStructure => "Ids structure",
				Action.XsdCorrectness => "Xsd schemas correctness",
				Action.IdsContent => "Ids content",
				Action.IdsContentWithOmissions => "Ids content (omitted on regex match)",
				_ => x.ToString(),
			};
		}
	}

	/// <summary>
	/// Types of controls that the tools performs, resulting from the evaluation of options provided
	/// </summary>
	public enum Action
	{
		/// <summary>
		/// Checks that the structure of the IDS conforms to the schema
		/// </summary>
		IdsStructure,
		/// <summary>
		/// Checks that any schema provided is valid
		/// </summary>
		XsdCorrectness,		
		/// <summary>
		/// Checks the implementation agreement of the IDS content
		/// </summary>
		IdsContent,
		/// <summary>
		/// Checks the implementation agreement of the IDS content, but skipping some files, due to an exclusion pattern
		/// </summary>
		IdsContentWithOmissions,
	}

}
