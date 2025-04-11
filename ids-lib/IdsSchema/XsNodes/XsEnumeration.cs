using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsEnumeration : IdsXmlNode, IStringListMatcher, IStringPrefixMatcher, IFiniteStringMatcher
{
    private readonly string value;
    public XsEnumeration(XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        value = reader.GetAttribute("value") ?? string.Empty;
    }

    public Audit.Status MustMatchAgainstCandidates(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string variableName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        if (!TryMatch(candidateStrings, ignoreCase, out matches))
            return IdsErrorMessages.Report103InvalidListMatcher(this, value, logger, variableName, schemaContext, candidateStrings);
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

    public string Value => value;

	protected internal override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
	{
		Audit.Status ret = Audit.Status.Ok;
		if (!TryGetUpperNode<XsRestriction>(logger, this, XsRestriction.RestrictionIdentificationArray, out var restriction, out var retStatus))
			return retStatus;
        ret |= XsTypes.AuditStringValue(logger, restriction.Base, value, this);
		return ret;
	}
}
