using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsSpecification : BaseContext
{
    private readonly MinMaxOccur minMaxOccurr;
    internal readonly IfcSchemaVersions SchemaVersions = IfcSchemaVersions.IfcNoVersion;

    private readonly BaseContext? parent;
    protected override internal BaseContext? Parent => parent;
    
    public IdsSpecification(System.Xml.XmlReader reader, BaseContext? parent, ILogger? logger) : base(reader, null)
    {
        this.parent = parent;
        minMaxOccurr = new MinMaxOccur(reader);
        var vrs = reader.GetAttribute("ifcVersion") ?? string.Empty;
        SchemaVersions = vrs.GetSchemaVersions(this, logger);
    }

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (minMaxOccurr.Audit(out var _) != Audit.Status.Ok)
            ret |= logger.ReportInvalidOccurr(this, minMaxOccurr);
        if (SchemaVersions == IfcSchemaVersions.IfcNoVersion)
            ret |= logger.ReportInvalidSchemaVersion(SchemaVersions, this);
        var applic = GetChildNode<IdsFacetCollection>("applicability");
        if (applic is null)
        {
            ret |= IdsLoggerExtensions.ReportInvalidApplicability(logger, this, "not found");
            return ret;
        }
        if (!applic.ChildFacets.Any())
        {
            ret |= IdsLoggerExtensions.ReportInvalidApplicability(logger, this, "one condition is required at minimum");
            return ret;
        }

        var reqs = GetChildNode<IdsFacetCollection>("requirements");
        if (applic is not null && reqs is not null
            && !applic.ValidTypes.IsEmpty // if they are empty an error would already be notified
            && !reqs.ValidTypes.IsEmpty
            )
        {
            var totalFilters = applic.ValidTypes.Intersect(reqs.ValidTypes);
            if (totalFilters.IsEmpty)
                ret |= IdsLoggerExtensions.ReportIncompatibleClauses(logger, this, "impossible match of applicability and requirements");
        }

        return base.PerformAudit(logger) | ret;
    }
}
