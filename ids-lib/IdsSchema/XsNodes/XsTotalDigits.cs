using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsTotalDigits : BaseContext, IStringListMatcher
{
    // private readonly string value;
    public XsTotalDigits(XmlReader reader) : base(reader)
    {
        // value might be useful for future implementations
        // value = reader.GetAttribute("value") ?? string.Empty; 
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        // totalDigits in invalid when it comes to string
        matches = Enumerable.Empty<string>();
        return IdsLoggerExtensions.ReportInvalidXsFacet(logger, this, "use `length` instead");
    }
}
