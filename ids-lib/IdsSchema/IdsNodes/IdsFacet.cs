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
        var ret = Audit.Status.Ok;
        if (minMaxOccurr.Audit(out var _) != Audit.Status.Ok)
        {
            logger.ReportInvalidOccurr(this, minMaxOccurr);
            ret |= MinMaxOccur.ErrorStatus;
        }
        return ret;
    }
}
