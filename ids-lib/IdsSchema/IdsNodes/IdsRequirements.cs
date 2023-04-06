using Microsoft.Extensions.Logging;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsRequirements : BaseContext
{
    /// <summary>
    /// This class is used for Auditing facets as requirements:
    /// </summary>
    /// <param name="reader"></param>
    public IdsRequirements(System.Xml.XmlReader reader) : base(reader)
    {
        // no specific fields
    }

    protected internal override Audit.Status PerformAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        foreach (var item in Children.OfType<IIdsRequirementFacet>())
        {
            ret |= item.PerformAuditAsRequirement(logger);
        }
        return ret;
    }
}
