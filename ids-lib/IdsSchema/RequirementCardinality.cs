using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IdsLib.IdsSchema
{
	internal class RequirementCardinality : ICardinality
	{
		public enum CardOptions
		{
			Invalid,
			Required,
			Prohibited,
			Optional
		}
		public bool IsRequired => cardOption == CardOptions.Required;

		CardOptions cardOption { get; set; } = CardOptions.Required;	

		public string OptionValue { get; private set; } = string.Empty;

		public RequirementCardinality(XmlReader reader)
		{
			OptionValue = reader.GetAttribute("cardinality") ?? "";
			// both default to "1" according to xml:xs specifications
			cardOption = OptionValue switch
			{
				"" => CardOptions.Required,
				"Required" => CardOptions.Required,
				"Prohibited" => CardOptions.Prohibited,
				"Optional" => CardOptions.Optional,
				_ => CardOptions.Invalid
			};
		}

		internal const Audit.Status ErrorStatus = IdsLib.Audit.Status.IdsContentError;

		/// <inheritdoc/>
		public Audit.Status Audit(out string errorMessage)
		{
			if (cardOption == CardOptions.Invalid)
			{
				errorMessage = $"Invalid value '{OptionValue}' for attrribute";
				return ErrorStatus;
			}
			errorMessage = string.Empty;
			return IdsLib.Audit.Status.Ok;
		}
	}
}
