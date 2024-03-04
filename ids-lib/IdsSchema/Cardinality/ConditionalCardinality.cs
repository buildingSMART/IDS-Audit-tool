using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IdsLib.IdsSchema.Cardinality
{
    internal class ConditionalCardinality : ICardinality
    {
        internal string enumerationValue { init;  get; } 

        public ConditionalCardinality(XmlReader reader)
        {
            enumerationValue = reader.GetAttribute("cardinality") ?? "required";
        }

        public bool IsRequired => enumerationValue == "required";
        public bool IsProhibited => enumerationValue == "prohibited";

        public Audit.Status Audit(out string errorMessage)
        {
            switch (enumerationValue)
            {
                case "required":
                case "prohibited":
                case "optional":
                    errorMessage = string.Empty;
                    return IdsLib.Audit.Status.Ok;
                default:
                    errorMessage = $"Invalid cardinality '{enumerationValue}'";
                    return CardinalityConstants.CardinalityErrorStatus;
            }
        }
    }
}
