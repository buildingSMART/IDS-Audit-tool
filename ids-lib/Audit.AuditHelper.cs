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
            // preparing location
            var location = "position unknown";
            if (sender is IXmlLineInfo rdr)
            {
                location = $"line {rdr.LineNumber}, position {rdr.LinePosition}";
            }
            // reporting issues
            if (e.Severity == XmlSeverityType.Warning)
            {
                switch (Options.XmlWarningAction)
                {
                    case AuditProcessOptions.XmlWarningBehaviour.ReportAsInformation:
                        Logger?.LogInformation("Schema compliance warning at {location}; {message}", location, e.Message);
                        // status is not changed
                        break;
                    case AuditProcessOptions.XmlWarningBehaviour.ReportAsWarning:
                        Logger?.LogWarning("Schema compliance warning at {location}; {message}", location, e.Message);
                        SchemaStatus |= Status.IdsStructureWarning;
                        break;
                    case AuditProcessOptions.XmlWarningBehaviour.ReportAsError:
                        // the type reported is an error, but its original nature of warning is retained to help debug
                        Logger?.LogError("Schema compliance warning at {location}; {message}", location, e.Message);
                        SchemaStatus |= Status.IdsStructureError;
                        break;
                    default: // nothing to do
                        break;
                }
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                Logger?.LogError("Schema compliance error at {location}; {message}", location, e.Message);
                SchemaStatus |= Status.IdsStructureError;
            }
        }
    }
}
