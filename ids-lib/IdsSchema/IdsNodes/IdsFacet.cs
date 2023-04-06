using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacet : BaseContext, IIdsRequirementFacet
{
    private readonly MinMaxOccur minMaxOccurr;
    public IdsFacet(System.Xml.XmlReader reader) : base(reader)
    {
        minMaxOccurr = new MinMaxOccur(reader);
    }

    public Audit.Status PerformAuditAsRequirement(ILogger? logger)
    {
        if (minMaxOccurr.Audit(out var _) != Audit.Status.Ok)
            return logger.ReportInvalidOccurr(this, minMaxOccurr);
        return Audit.Status.Ok;
    }
}
