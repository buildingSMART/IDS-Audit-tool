using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IdsLib.IdsSchema.Cardinality
{
    internal class SimpleCardinality : ICardinality
    {
        private readonly string enumerationValue;

        public SimpleCardinality(XmlReader reader)
        {
            enumerationValue = reader.GetAttribute("cardinality") ?? "Required";
        }

        public bool IsRequired => enumerationValue == "Required";

        public Audit.Status Audit(out string errorMessage)
        {
            switch (enumerationValue)
            {
                case "Required":
                case "Prohibited":
                    errorMessage = string.Empty;
                    return IdsLib.Audit.Status.Ok;
                default:
                    errorMessage = $"Invalid cardinality '{enumerationValue}'";
                    return MinMaxCardinality.CardinalityErrorStatus;
            }
        }
    }
}
