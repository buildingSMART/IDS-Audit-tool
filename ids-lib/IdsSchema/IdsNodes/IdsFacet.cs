using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacet : BaseContext, IIdsCardinalityFacet
{
    private readonly MinMaxOccur minMaxOccurr;
    public IdsFacet(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
        minMaxOccurr = new MinMaxOccur(reader);
    }

    public bool IsValid => true;

    public bool IsRequired => minMaxOccurr.IsRequired;

    public Audit.Status PerformCardinalityAudit(ILogger? logger)
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
