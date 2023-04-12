using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsLength : BaseContext, IStringListMatcher
{
    private readonly string value;
    public XsLength(XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
        value = reader.GetAttribute("value") ?? string.Empty;
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        if (!int.TryParse(value, out var len)) 
        {
            matches = Enumerable.Empty<string>();   
            return IdsLoggerExtensions.ReportInvalidListMatcher(this, value, logger, listToMatchName, schemaContext);
        }
        matches = candidateStrings.Where(x=>x.Length == len).ToList();
        return matches.Any()
           ? Audit.Status.Ok
           : Audit.Status.IdsContentError;
    }

    public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
    {
        if (!int.TryParse(value, out var len))
        {
            matches = Enumerable.Empty<string>();
            return false;
        }
        matches = candidateStrings.Where(x => x.Length == len).ToList();
        return matches.Any();
    }

    protected internal override Audit.Status PerformAudit(ILogger? logger)
    {
        // Debug.WriteLine($"Children: {Children.Count}");
        return base.PerformAudit(logger);
    }
}
