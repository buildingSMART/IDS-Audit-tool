using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal partial class IdsSimpleValue : BaseContext, IStringListMatcher
{
    internal string Content = string.Empty;

    public IdsSimpleValue(System.Xml.XmlReader reader) : base(reader)
    {
    }

    protected internal override void SetContent(string contentString)
    {
        Content = contentString ?? string.Empty;
    }     

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        var compCase = ignoreCase
            ? System.StringComparison.OrdinalIgnoreCase
            : System.StringComparison.Ordinal;
        matches = candidateStrings.Where(x => x.Equals(Content, compCase)).ToList();
        if (!matches.Any())
            return IdsLoggerExtensions.ReportInvalidListMatcher(this, Content, logger, listToMatchName, schemaContext);
        return Audit.Status.Ok;
    }
}
