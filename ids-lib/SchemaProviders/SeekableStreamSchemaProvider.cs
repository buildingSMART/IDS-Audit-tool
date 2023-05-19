using IdsLib.IdsSchema;
using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace IdsLib.SchemaProviders
{
    internal class SeekableStreamSchemaProvider : SchemaProvider, Audit.ISchemaProvider
    {
        public Audit.Status GetSchemas(Stream source, ILogger? logger, out IEnumerable<XmlSchema> schemas)
        {
            if (!source.CanSeek)
            {
                schemas = Enumerable.Empty<XmlSchema>();
				return IdsToolMessages.ReportUnseekableStream(logger);
                
            }
            var originalPosition = source.Position;
            source.Seek(0, SeekOrigin.Begin);
            var info = IdsXmlHelpers.GetIdsInformationAsync(source).Result;
            source.Position = originalPosition;
            if (!info.IsIds)
            {
                schemas = Enumerable.Empty<XmlSchema>();
                return IdsToolMessages.ReportUnexpectedScenario(logger, !string.IsNullOrWhiteSpace(info.StatusMessage)
                        ? info.StatusMessage
                        : "The stream provided does not contain a recognised IDS."
                    );

            }
            if (info.Version == IdsVersion.Invalid)
            {
                schemas = Enumerable.Empty<XmlSchema>();
                return IdsToolMessages.ReportInvalidVersion(logger);
            }
            return GetResourceSchemasByVersion(info.Version, logger, out schemas);
        }
    }

}
