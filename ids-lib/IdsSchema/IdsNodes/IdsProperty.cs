using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsProperty : BaseContext, IIdsRequirementFacet
{
    private static readonly string[] SpecificationArray = { "specification" };
    private readonly MinMaxOccur minMaxOccurr;
    private readonly IStringListMatcher? measureMatcher;
    public IdsProperty(System.Xml.XmlReader reader) : base(reader)
    {
        minMaxOccurr = new MinMaxOccur(reader);
        var measure = reader.GetAttribute("measure") ?? string.Empty;
        if (!string.IsNullOrEmpty(measure))
            measureMatcher = new StringListMatcher(measure, this);
        else
            measureMatcher = null;
    }

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        if (!TryGetUpperNodes(this, SpecificationArray, out var nodes))
        {
            IdsLoggerExtensions.ReportUnexpectedScenario(logger, "Missing specification for property.", this);
            return Audit.Status.IdsStructureError;
        }
        if (nodes[0] is not IdsSpecification spec)
        {
            IdsLoggerExtensions.ReportUnexpectedScenario(logger, "Invalid specification for property.", this);
            return Audit.Status.IdsContentError;
        }
        var requiredSchemaVersions = spec.SchemaVersions;
        var names = GetChildNodes("name");
        
        var ret = Audit.Status.Ok;
        if (measureMatcher is not null)
        {
            var validMeasureNames = SchemaInfo.AllMeasures
                .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
                .Select(y => y.IfcMeasureClassName);
            var result = measureMatcher.DoesMatch(validMeasureNames, false, logger, out var matches, "measure names", requiredSchemaVersions);
            ret |= result;
        }
        return ret;
    }

    public Audit.Status PerformAuditAsRequirement(ILogger? logger)
    {
        if (minMaxOccurr.Audit(out var _) != Audit.Status.Ok)
            return logger.ReportInvalidOccurr(this, minMaxOccurr);
        return Audit.Status.Ok;
    }
}
