﻿using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsPattern : IdsXmlNode, IStringListMatcher, IFiniteStringMatcher
{
    private readonly string pattern;
    public XsPattern(XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        pattern = reader.GetAttribute("value") ?? string.Empty;
    }

    public Audit.Status MustMatchAgainstCandidates(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string variableName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        if (!EnsureRegex(out var _, ignoreCase))
        {
            matches = new List<string>();
            return IdsErrorMessages.Report103InvalidListMatcher(this, pattern, logger, variableName, schemaContext, candidateStrings);
        }
        return (TryMatch(candidateStrings, ignoreCase, out matches))
            ? Audit.Status.Ok
            : IdsErrorMessages.Report103InvalidListMatcher(this, pattern, logger, variableName, schemaContext, candidateStrings);
    }

	protected internal override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
	{
        var ret = Audit.Status.Ok;
        if (!EnsureRegex(out var _, false))
        {
            ret = IdsErrorMessages.Report109InvalidRegex(this, pattern, logger);
		}
        return base.PerformAudit(stateInfo, logger) | ret;
	}

	public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
    {
        if (!EnsureRegex(out var _, ignoreCase))
        {
            matches = Enumerable.Empty<string>();   
            return false;
        }
        if (ignoreCase)
            matches = candidateStrings.Where(x => compiledCaseInsensitiveRegex!.IsMatch(x)).ToList();
        else
            matches = candidateStrings.Where(x => compiledCaseSensitiveRegex!.IsMatch(x)).ToList();
        return matches.Any();
    }

    private Regex? compiledCaseSensitiveRegex;
    private Regex? compiledCaseInsensitiveRegex;

    /// <returns>true if ok, false if error</returns>
    private bool EnsureRegex(out string errorMessage, bool ignoreCase)
    {
        errorMessage = "";
        if (
            (ignoreCase && compiledCaseInsensitiveRegex != null)
            ||
            (!ignoreCase && compiledCaseSensitiveRegex != null)
            )
            return true;
        try
        {
            var preProcess = XmlRegex.Preprocess(pattern, false);
            if (!ignoreCase)
                compiledCaseSensitiveRegex = new Regex(preProcess, RegexOptions.Compiled | RegexOptions.Singleline);
            else
                compiledCaseInsensitiveRegex = new Regex(preProcess, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    public IEnumerable<string> GetDicreteValues()
    {
        // todo: GetDicreteValues() for regexes is not implemented
        // we can parse the regex and see if there are simple token recognition that can be implemented
        // perhaps https://github.com/moodmosaic/Fare can help 
        yield break;
    }

    // todo: IStringPrefixMatcher.MatchesPrefix is not implemented for regexes, but it could be useful to fully enforce pset_ naming constraints
}
