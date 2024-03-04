using IdsLib.IdsSchema.IdsNodes;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;

namespace IdsLib.SchemaProviders;

/// <summary>
/// A schema provider based on a specific recognised version
/// </summary>
public class FixedVersionSchemaProvider : SchemaProvider, Audit.ISchemaProvider
{
    private readonly IdsVersion fixedVersion;

    /// <summary>
    /// Public constructor
    /// </summary>
    /// <param name="vrs">The fixed schema version</param>
    public FixedVersionSchemaProvider(IdsVersion vrs)
    {
        fixedVersion = vrs;
    }

    /// <summary>
    /// standard Implementation of the resolver.
    /// </summary>
    public Audit.Status GetSchemas(Stream vrs, ILogger? logger, out IEnumerable<XmlSchema> schemas)
    {
        return GetResourceSchemasByVersion(fixedVersion, logger, out schemas);
    }
}
