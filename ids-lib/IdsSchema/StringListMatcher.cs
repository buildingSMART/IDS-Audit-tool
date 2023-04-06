using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema;

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
}
