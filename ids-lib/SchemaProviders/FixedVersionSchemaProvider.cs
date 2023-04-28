using IdsLib.IdsSchema.IdsNodes;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;

namespace IdsLib.SchemaProviders
{
    internal class FixedVersionSchemaProvider : SchemaProvider, Audit.ISchemaProvider
    {
        private readonly IdsVersion fixedVersion;
        public FixedVersionSchemaProvider(IdsVersion vrs)
        {
            fixedVersion = vrs;
        }

        public Audit.Status GetSchemas(Stream vrs, ILogger? logger, out IEnumerable<XmlSchema> schemas)
        {
            return GetResourceSchemasByVersion(fixedVersion, logger, out schemas);
        }
    }

}
