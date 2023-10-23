using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsRootElement : IdsXmlNode
{
	public IdsVersion SchemaVersion { get; private set; }

	public IdsRootElement(System.Xml.XmlReader reader, ILogger? logger) : base(reader, null)
    {
		string locationAttribute = IdsXmlHelpers.GetSchemaLocation(reader);
		SchemaVersion = IdsFacts.GetVersionFromLocation(locationAttribute, logger);
	}
}
