using System.Xml.Schema;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace IdsLib;

public static partial class Audit
{
    private class AuditHelper
    {
        internal ILogger? Logger;

        public AuditProcessOptions Options { get; }

        public AuditHelper(ILogger? logger, AuditProcessOptions options)
        {
            Logger = logger;
            Options = options;
        }

        public Status SchemaStatus { get; internal set; }
        
        public void ValidationReporter(object? sender, ValidationEventArgs e)
        {
            var location = "position unknown";
            if (sender is IXmlLineInfo rdr)
            {
                location = $"line {rdr.LineNumber}, position {rdr.LinePosition}";
            }
            if (e.Severity == XmlSeverityType.Warning)
            {
                Logger?.LogWarning("XML WARNING at {location}; {message}", location, e.Message);
                SchemaStatus |= Status.IdsStructureError;
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                Logger?.LogError("XML ERROR at {location}; {message}", location, e.Message);
                SchemaStatus |= Status.IdsStructureError;
            }
        }
    }
}
