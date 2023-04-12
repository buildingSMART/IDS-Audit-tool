using IdsLib.IdsSchema.IdsNodes;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsRestriction : BaseContext, IStringListMatcher, IStringPrefixMatcher, IFiniteStringMatcher
{
    private readonly string baseAsString;
    public XsRestriction(XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
        baseAsString = reader.GetAttribute("base") ?? string.Empty;
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        matches =  Enumerable.Empty<string>();
        if ( // todo: discuss with group: do we want to force the base type requirement for strings?
            !string.IsNullOrWhiteSpace(baseAsString) &&
            baseAsString != "xs:string"
            )
            return IdsLoggerExtensions.ReportBadType(logger, this, baseAsString);
        var ret = Audit.Status.Ok;
        foreach (var child in Children)
        {
            // only matcher values are reported
            if (child is not IStringListMatcher ismv)
            {
                // ret |= IdsLoggerExtensions.ReportBadMatcher(logger, child, "string");
                // this would let xs:annotation pass with no issues
                continue;
            }
            ret |= ismv.DoesMatch(candidateStrings, ignoreCase, logger, out var thisM, listToMatchName, schemaContext);
            if (thisM.Any())
                matches = matches.Union(thisM);   
        }
        return ret;
    }

    public IEnumerable<string> GetDicreteValues()
    {
        foreach (var child in Children.OfType<IFiniteStringMatcher>())
            foreach (var item in child.GetDicreteValues())
                yield return item;
    }

    public bool MatchesPrefix(string prefixString)
    {
        foreach (var child in Children.OfType<IStringPrefixMatcher>())
        {
            if (child.MatchesPrefix(prefixString)) 
                return true;
        }
        return false;
    }

    public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
    {
        matches = Enumerable.Empty<string>();
        if ( // todo: discuss with group: do we want to force the base type requirement for strings?
            !string.IsNullOrWhiteSpace(baseAsString) &&
            baseAsString != "xs:string"
            )
            return false;
        foreach (var child in Children)
        {
            // only matcher values are reported
            if (child is not IStringListMatcher ismv)
            {
                continue;
            }
            if (ismv.TryMatch(candidateStrings, ignoreCase, out var thisM))
                matches = matches.Union(thisM);
        }
        return matches.Any();
    }

    protected internal override Audit.Status PerformAudit(ILogger? logger)
    {
        // todo: are there constraints to enforce between the types of the children in XS:Restriction?
        return base.PerformAudit(logger);
    }
}
