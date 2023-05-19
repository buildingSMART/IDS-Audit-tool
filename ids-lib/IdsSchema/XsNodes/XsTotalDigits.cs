using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsTotalDigits : BaseContext, IStringListMatcher
{
    public XsTotalDigits(XmlReader reader, BaseContext? parent) : base(reader, parent)
    {        
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        // totalDigits in invalid when it comes to string
        matches = Enumerable.Empty<string>();
        return IdsMessage.ReportInvalidXsFacet(logger, this, "use `length` instead");
    }

    public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
    {
        // totalDigits in invalid when it comes to string
        matches = Enumerable.Empty<string>();
        return false;
    }
}
