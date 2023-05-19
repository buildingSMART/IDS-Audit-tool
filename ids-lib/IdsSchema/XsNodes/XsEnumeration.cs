using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsEnumeration : BaseContext, IStringListMatcher, IStringPrefixMatcher, IFiniteStringMatcher
{
    private readonly string value;
    public XsEnumeration(XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
        value = reader.GetAttribute("value") ?? string.Empty;
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        if (!TryMatch(candidateStrings, ignoreCase, out matches))
            return IdsMessage.ReportInvalidListMatcher(this, value, logger, listToMatchName, schemaContext, candidateStrings);
        return Audit.Status.Ok;
    }

    public IEnumerable<string> GetDicreteValues()
    {
        yield return value;
    }

    public bool MatchesPrefix(string prefixString)
    {
        return value.StartsWith(prefixString);
    }

    public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
    {
        var compCase = ignoreCase
                    ? System.StringComparison.OrdinalIgnoreCase
                    : System.StringComparison.Ordinal;
        matches = candidateStrings.Where(x => x.Equals(value, compCase)).ToList();
        return matches.Any();
    }
}
