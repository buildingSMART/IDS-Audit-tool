using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsTotalDigits : IdsXmlNode, IStringListMatcher
{
    public XsTotalDigits(XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {        
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        // totalDigits in invalid when it comes to string
        matches = Enumerable.Empty<string>();
        return IdsErrorMessages.Report302InvalidXsFacet(logger, this, "use `length` instead");
    }

    public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
    {
        // totalDigits in invalid when it comes to string
        matches = Enumerable.Empty<string>();
        return false;
    }
}
