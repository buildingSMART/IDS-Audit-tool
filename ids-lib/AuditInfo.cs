using System.Xml.Schema;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace IdsLib;

public static partial class Audit
{
    private class AuditInfo
    {
        public IAuditOptions Options { get; }

        internal ILogger? Logger;

        public AuditInfo(IAuditOptions opts, ILogger? logger)
        {
            Options = opts;
            Logger = logger;
        }

        public string? ValidatingFile { get; set; }

        public Status Status { get; internal set; }

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
                Status |= Status.IdsStructureError;
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                Logger?.LogError("XML ERROR at {location}; {message}", location, e.Message);
                Status |= Status.IdsStructureError;
            }
        }
    }
}
