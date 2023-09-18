using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;

namespace IdsLib.SchemaProviders
{
    public abstract class SchemaProvider
    {
        protected static Audit.Status GetResourceSchemasByVersion(IdsVersion vrs, ILogger? logger, out IEnumerable<XmlSchema> schemas)
        {
            var ret = Audit.Status.Ok;
            IEnumerable<string> tmpResources = Enumerable.Empty<string>();
            switch (vrs)
            {
                case IdsVersion.Ids0_9:
                case IdsVersion.Ids1_0:
                    tmpResources = new List<string> { "xsdschema.xsd", "xml.xsd", "ids.xsd" };
                    break;
                case IdsVersion.Invalid:
                default:
                    ret |= IdsToolMessages.ReportInvalidSchemaVersion(logger, vrs);
                    break;
            }
            schemas = tmpResources.Select(x => GetSchema(x)).ToList();
            return ret;
        }

        // todo: change to return status and out parameter for schemas

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="imports"></param>
        /// <returns></returns>
        protected static IEnumerable<XmlSchema> GetResourceSchemasFromImports(ILogger? logger, IEnumerable<string> imports)
        {
            var distinct = imports.Distinct();
            foreach (var schema in distinct)
            {
                switch (schema)
                {
                    case "http://www.w3.org/2001/xml.xsd":
                    case "https://www.w3.org/2001/xml.xsd":
                        yield return GetSchema("xml.xsd")!;
                        yield return GetSchema("xsdschema.xsd")!;
                        break;
                    case "http://www.w3.org/2001/XMLSchema.xsd":
                    case "https://www.w3.org/2001/XMLSchema.xsd":
                        break;
                    case "http://www.w3.org/2001/XMLSchema-instance":
                    case "https://www.w3.org/2001/XMLSchema-instance":
                        break;
                    default:
                        XsdMessages.ReportUnexpectedSchema(logger, schema);
                        break;
                }
            }
        }

        protected static XmlSchema GetSchema(string name)
        {
            var fullName = "IdsLib.Resources.XsdSchemas." + name;
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName)
                    ?? throw new NotImplementedException("Null resource stream.");
            var schema = XmlSchema.Read(stream, null)
                ?? throw new NotImplementedException("Invalid resource stream.");
            return schema;
        }
    }

}
