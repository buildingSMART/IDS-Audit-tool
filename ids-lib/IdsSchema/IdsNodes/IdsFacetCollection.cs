using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacetCollection : BaseContext, IIfcTypeConstraintProvider
{
    public IdsFacetCollection(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
    }

    private IIfcTypeConstraint? typeFilter = null;
    internal IEnumerable<IIdsFacet> ChildFacets => Children.OfType<IIdsFacet>();

    private bool typesFilterInitialized = false;

    public IIfcTypeConstraint? TypesFilter
    {
        get
        {
            if (!typesFilterInitialized) 
            {
                typeFilter = null;
                foreach (var provider in Children.OfType<IIfcTypeConstraintProvider>())
                {
                    if (provider is IIdsCardinalityFacet card && !card.IsRequired)
                        continue;
                    typeFilter = IfcTypeConstraint.Intersect(typeFilter, provider.TypesFilter);
                    if (IfcTypeConstraint.IsNotNullAndEmpty(typeFilter))
                        break;                    
                }
                typesFilterInitialized = true;
            }
            return typeFilter;
        }
    }

    protected internal override Audit.Status PerformAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (type == "requirements")
        {
            foreach (var extendedRequirement in Children.OfType<IIdsCardinalityFacet>())
            {
                ret |= extendedRequirement.PerformCardinalityAudit(logger);
            }
        }
        if (!ChildFacets.Any(x=>!x.IsValid) && IfcTypeConstraint.IsNotNullAndEmpty(TypesFilter))
            ret |= IdsMessage.ReportIncompatibleClauses(logger, this, "impossible match of constraints in set");
        return ret;
    }
}
