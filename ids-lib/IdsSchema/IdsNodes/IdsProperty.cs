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
    public IdsProperty(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
        minMaxOccurr = new MinMaxOccur(reader);
        var measure = reader.GetAttribute("measure") ?? string.Empty;
        if (!string.IsNullOrEmpty(measure))
            measureMatcher = new StringListMatcher(measure, this);
        else
            measureMatcher = null;
    }

    public bool IsValid => true;

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        if (!TryGetUpperNode<IdsSpecification>(logger, this, SpecificationArray, out var spec, out var retStatus))
            return retStatus;
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
