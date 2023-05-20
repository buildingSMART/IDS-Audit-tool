using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsSpecification : IdsXmlNode
{
	internal static readonly string[] SpecificationIdentificationArray = { "specification" };

	private readonly MinMaxOccur minMaxOccurr;
    internal readonly IfcSchemaVersions SchemaVersions = IfcSchemaVersions.IfcNoVersion;

    private readonly IdsXmlNode? parent;
    protected override internal IdsXmlNode? Parent => parent;
    
    public IdsSpecification(System.Xml.XmlReader reader, IdsXmlNode? parent, ILogger? logger) : base(reader, null)
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
            ret |= IdsMessage.ReportInvalidOccurr(logger, this, minMaxOccurr);
        if (SchemaVersions == IfcSchemaVersions.IfcNoVersion)
            ret |= IdsMessage.ReportInvalidSchemaVersion(logger, SchemaVersions, this);
        var applic = GetChildNode<IdsFacetCollection>("applicability");
        if (applic is null)
        {
            ret |= IdsMessage.ReportInvalidApplicability(logger, this, "not found");
            return ret;
        }
        if (!applic.ChildFacets.Any())
        {
            ret |= IdsMessage.ReportInvalidApplicability(logger, this, "one condition is required at minimum");
            return ret;
        }

        var reqs = GetChildNode<IdsFacetCollection>("requirements");
        if (applic is not null && reqs is not null
            && !IfcTypeConstraint.IsNotNullAndEmpty(applic.TypesFilter) // if they are empty an error would already be notified
            && !IfcTypeConstraint.IsNotNullAndEmpty(reqs.TypesFilter)
            )
        {
            var totalFilters = IfcTypeConstraint.Intersect(applic.TypesFilter, reqs.TypesFilter);
            if (IfcTypeConstraint.IsNotNullAndEmpty(totalFilters))
                ret |= IdsMessage.ReportIncompatibleClauses(logger, this, "impossible match of applicability and requirements");
        }

        return base.PerformAudit(logger) | ret;
    }
}
