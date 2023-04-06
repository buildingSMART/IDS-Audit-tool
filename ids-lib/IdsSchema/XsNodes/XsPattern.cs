using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Xbim.InformationSpecifications;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsPattern : BaseContext, IStringListMatcher
{
    private readonly string pattern;
    public XsPattern(XmlReader reader) : base(reader)
    {
        pattern = reader.GetAttribute("value") ?? string.Empty;
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        if (!EnsureRegex(out var _, ignoreCase))
        {
            matches = new List<string>();
            return IdsLoggerExtensions.ReportInvalidListMatcher(this, pattern, logger, listToMatchName, schemaContext);
        }
        if (ignoreCase)
            matches = candidateStrings.Where(x => compiledCaseInsensitiveRegex!.IsMatch(x)).ToList();
        else
            matches = candidateStrings.Where(x => compiledCaseSensitiveRegex!.IsMatch(x)).ToList();
        return !matches.Any()
            ? IdsLoggerExtensions.ReportInvalidListMatcher(this, pattern, logger, listToMatchName, schemaContext)
            : Audit.Status.Ok;
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
            var preProcess = XmlRegex.Preprocess(pattern);
            if (!ignoreCase)
                compiledCaseSensitiveRegex = new Regex(preProcess, RegexOptions.Compiled | RegexOptions.Singleline);
            else
                compiledCaseInsensitiveRegex = new Regex(preProcess, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            // logger?.LogError("Invalid pattern constraint: {pattern}", pattern);
            return false;
        }
    }
}
