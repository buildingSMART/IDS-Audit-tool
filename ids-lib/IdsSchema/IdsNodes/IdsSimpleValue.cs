using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal partial class IdsSimpleValue : BaseContext, IStringListMatcher, IStringPrefixMatcher, IFiniteStringMatcher
{
    internal string Content = string.Empty;

    public IdsSimpleValue(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
    }

    protected internal override void SetContent(string contentString)
    {
        Content = contentString ?? string.Empty;
    }     

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        if (!TryMatch(candidateStrings, ignoreCase, out matches))
            return IdsMessage.ReportInvalidListMatcher(this, Content, logger, listToMatchName, schemaContext, candidateStrings);
        return Audit.Status.Ok;
    }

    public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
    {
        var compCase = ignoreCase
            ? System.StringComparison.OrdinalIgnoreCase
            : System.StringComparison.Ordinal;
        matches = candidateStrings.Where(x => x.Equals(Content, compCase)).ToList();
        return matches.Any();
    }

    public bool MatchesPrefix(string prefixString)
    {
        return Content.StartsWith(prefixString);
    }

    public IEnumerable<string> GetDicreteValues()
    {
        yield return Content;
    }
}
