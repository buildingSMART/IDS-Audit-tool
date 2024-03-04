using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace IdsLib.SchemaProviders;

internal class FileBasedSchemaProvider : SchemaProvider, Audit.ISchemaProvider
{
    private readonly List<string> schemaFiles;
    private Audit.Status validationStatus = Audit.Status.Ok;
    readonly ILogger? validationLogger;

    public FileBasedSchemaProvider(IEnumerable<string> schemaFiles, ILogger? logger)
    {
        validationLogger = logger;
        this.schemaFiles = schemaFiles.ToList();
    }

    void ValidationCallback(object? sender, ValidationEventArgs args)
    {
        if (args.Severity == XmlSeverityType.Warning)
            XsdMessages.ReportSchemaIssue(validationLogger, LogLevel.Warning, args.Message);
        else
            XsdMessages.ReportSchemaIssue(validationLogger, LogLevel.Error, args.Message);
        validationStatus = Audit.Status.XsdSchemaError;
    }

    public Audit.Status GetSchemas(Stream _, ILogger? logger, out IEnumerable<XmlSchema> schemas)
    {
        var ret = Audit.Status.Ok;
        var imports = new List<string>();
        var retSchemas = new List<XmlSchema>();
        foreach (var diskSchema in schemaFiles)
        {
            if (!File.Exists(diskSchema))
            {
                ret |= IdsToolMessages.ReportSourceNotFound(logger, diskSchema);
                continue;
            }
            try
            {
                using var reader = File.OpenText(diskSchema);
                validationStatus = Audit.Status.Ok;
                var schema = XmlSchema.Read(reader, ValidationCallback);
                if (validationStatus != Audit.Status.Ok)
                {
                    ret |= validationStatus;
                    continue;
                }
                if (schema is null)
                {
                    ret |= IdsToolMessages.ReportInvalidXsdSource(logger, diskSchema);
                    continue;
                }
                foreach (var location in schema.Includes.OfType<XmlSchemaImport>().Select(x => x.SchemaLocation))
                {
                    if (location is null)
                        continue;
                    imports.Add(location);
                }
                retSchemas.Add(schema);
            }
            catch (Exception)
            {
                ret |= IdsToolMessages.ReportInvalidXsdSource(logger, diskSchema);
            }

        }
        // also get required reference schemas
        foreach (var schema in GetResourceSchemasFromImports(logger, imports))
        {
            retSchemas.Add(schema);
        }
        schemas = retSchemas.ToArray();
        return ret;
    }
}
