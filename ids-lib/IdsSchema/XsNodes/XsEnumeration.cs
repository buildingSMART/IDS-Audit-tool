using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsEnumeration : IdsXmlNode, IStringListMatcher, IStringPrefixMatcher, IFiniteStringMatcher
{
    private readonly string value;
    public XsEnumeration(XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        value = reader.GetAttribute("value") ?? string.Empty;
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        if (!TryMatch(candidateStrings, ignoreCase, out matches))
            return IdsErrorMessages.Report103InvalidListMatcher(this, value, logger, listToMatchName, schemaContext, candidateStrings);
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

    readonly Regex regexInteger = new(@"^[+-]?(\d+)$", RegexOptions.Compiled);
	readonly Regex regexFloating = new(@"^[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?$", RegexOptions.Compiled);

	protected internal override Audit.Status PerformAudit(ILogger? logger)
	{
		Audit.Status ret = Audit.Status.Ok;
		if (!TryGetUpperNode<XsRestriction>(logger, this, XsRestriction.RestrictionIdentificationArray, out var restriction, out var retStatus))
			return retStatus;

        switch (restriction.Base)
        {
            case XsRestriction.BaseTypes.Invalid: // notified in the the restriction already, do nothing here
            case XsRestriction.BaseTypes.Undefined: // todo: to be discussed in group
            case XsRestriction.BaseTypes.XsString:  // nothing 
                break;
            case XsRestriction.BaseTypes.XsBoolean: 
                if (value != "false" && value != "true")
                    ret |= IdsErrorMessages.Report305BadConstraintValue(logger, this, value, restriction.Base);
                break;
            case XsRestriction.BaseTypes.XsInteger: 
				if (!regexInteger.IsMatch(value))
					ret |= IdsErrorMessages.Report305BadConstraintValue(logger, this, value, restriction.Base);
				break;
            case XsRestriction.BaseTypes.XsDouble: 
            case XsRestriction.BaseTypes.XsFloat:
            case XsRestriction.BaseTypes.XsDecimal:
				if (!regexFloating.IsMatch(value))
					ret |= IdsErrorMessages.Report305BadConstraintValue(logger, this, value, restriction.Base);
				break;
            case XsRestriction.BaseTypes.XsDuration: // todo: implement duration, discuss 
            case XsRestriction.BaseTypes.XsDateTime: // todo: implement date time value filter
            case XsRestriction.BaseTypes.XsDate: // todo: implement date value filter
            case XsRestriction.BaseTypes.XsAnyUri: // todo: implement Uri value filter
            default:
                ret |= IdsErrorMessages.Report501UnexpectedScenario(logger, "base type evaluation not implemented.", this);
                break;
        }
        return ret;
	}
}
