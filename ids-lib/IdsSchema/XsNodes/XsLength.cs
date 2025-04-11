using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsLength : IdsXmlNode, IStringListMatcher
{
    private readonly string value;
    public XsLength(XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        value = reader.GetAttribute("value") ?? string.Empty;
    }

    public Audit.Status MustMatchAgainstCandidates(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string variableName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        if (!int.TryParse(value, out var len)) 
        {
            matches = Enumerable.Empty<string>();   
            return IdsErrorMessages.Report103InvalidListMatcher(this, value, logger, variableName, schemaContext, candidateStrings);
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

    protected internal override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
        // Debug.WriteLine($"Children: {Children.Count}");
        return base.PerformAudit(stateInfo, logger);
    }
}
