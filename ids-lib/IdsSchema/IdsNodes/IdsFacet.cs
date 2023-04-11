using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacet : BaseContext, IIdsRequirementFacet
{
    private readonly MinMaxOccur minMaxOccurr;
    public IdsFacet(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
        minMaxOccurr = new MinMaxOccur(reader);
    }

    public bool IsValid => true;

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
