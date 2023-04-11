using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;

namespace IdsLib.IdsSchema;

[DebuggerDisplay("{value}")]
internal class StringListMatcher : IStringListMatcher
{
    private readonly string value;
    private readonly BaseContext context;
    public StringListMatcher(string value, BaseContext context)
    {
        this.value = value;
        this.context = context;
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        var compCase = ignoreCase
                    ? System.StringComparison.OrdinalIgnoreCase
                    : System.StringComparison.Ordinal;
        matches = candidateStrings.Where(x => x.Equals(value, compCase)).ToList();
        if (!matches.Any())
            return IdsLoggerExtensions.ReportInvalidListMatcher(context, value, logger, listToMatchName, schemaContext);
        return Audit.Status.Ok;
    }

    /// <summary>
    /// triggers a log error if there's anything but a single match
    /// </summary>
    internal Audit.Status HasSingleMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out string? singleMatch, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        var ret = DoesMatch(candidateStrings, ignoreCase, logger, out var matches, listToMatchName, schemaContext);
        if (ret == Audit.Status.Ok)
        {
            try
            {
                singleMatch = matches.Single();
            }
            catch (Exception)
            {
                var count = matches.Count();
                ret |= IdsLoggerExtensions.ReportInvalidListMatcherCount(context, value, logger, listToMatchName, count, schemaContext);
                singleMatch = null;
                return ret;
            }
        }
        else
        {
            singleMatch = null; 
        }
        return ret;
    }
}
