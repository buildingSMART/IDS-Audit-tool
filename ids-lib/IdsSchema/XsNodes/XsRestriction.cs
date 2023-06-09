﻿using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsRestriction : IdsXmlNode, IStringListMatcher, IStringPrefixMatcher, IFiniteStringMatcher
{
    internal enum BaseTypes
    {
        Invalid,
        Undefined,
        XsString,
		XsBoolean,
        XsInteger,
        XsDouble,
        XsFloat,
        XsDecimal,
		XsDuration,
		XsDateTime,
		XsDate,
		XsAnyUri,
	}

    private string BaseAsString { get; init; }
    internal BaseTypes Base { get; init; }

	internal static readonly string[] RestrictionIdentificationArray = { "restriction" };

	public XsRestriction(XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        BaseAsString = reader.GetAttribute("base") ?? string.Empty;
        Base = GetBaseFrom(BaseAsString);
    }

	private static BaseTypes GetBaseFrom(string baseAsString)
	{
        return baseAsString switch
        {
            "" => BaseTypes.Undefined,
            "xs:string" => BaseTypes.XsString,
			"xs:boolean" => BaseTypes.XsBoolean,
			"xs:integer" => BaseTypes.XsInteger,
			"xs:double" => BaseTypes.XsDouble,
			"xs:float" => BaseTypes.XsFloat,
			"xs:decimal" => BaseTypes.XsDecimal,
			"xs:duration" => BaseTypes.XsDuration,
			"xs:dateTime" => BaseTypes.XsDateTime,
			"xs:date" => BaseTypes.XsDate,
			"xs:anyUri" => BaseTypes.XsAnyUri,
			_ => BaseTypes.Invalid
        };
	}

	public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
		var ret = Audit.Status.Ok;
        matches =  Enumerable.Empty<string>();
        if ( // todo: discuss with group: do we want to force the base type requirement for strings?
            !string.IsNullOrWhiteSpace(BaseAsString) &&
            BaseAsString != "xs:string"
            )
            return IdsMessages.Report303RestrictionBadType(logger, this, BaseAsString);
        
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
            !string.IsNullOrWhiteSpace(BaseAsString) &&
            BaseAsString != "xs:string"
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
		var ret = Audit.Status.Ok;
		if (Base == BaseTypes.Invalid)
			ret |= IdsMessages.Report303RestrictionBadType(logger, this, BaseAsString);
        if (!Children.Any())
            IdsMessages.Report304RestrictionEmptyContent(logger, this);
		return ret;
    }
}
