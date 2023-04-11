using IdsLib.IfcSchema.TypeFilters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacetCollection : BaseContext, IIfcTypeConstraintProvider
{
    private readonly IfcSchema.IfcSchemaVersions schemaVersions = IfcSchema.IfcSchemaVersions.IfcNoVersion;

    private static readonly string[] SpecificationArray = { "specification" };
    /// <summary>
    /// This class is used for Auditing facets as requirements
    /// </summary>
    public IdsFacetCollection(System.Xml.XmlReader reader, BaseContext? parent, ILogger? logger) : base(reader, parent)
    {
        if (TryGetUpperNode<IdsSpecification>(logger, this, SpecificationArray, out var spec, out var _))
            schemaVersions = spec.SchemaVersions;
    }

    private IIfcTypeConstraint? validTypes = null;
    internal IEnumerable<IIdsFacet> ChildFacets => Children.OfType<IIdsFacet>();

    public IIfcTypeConstraint ValidTypes
    {
        get
        {
            if (validTypes is null) 
            {
                IIfcTypeConstraint? flt = null;
                foreach (var provider in Children.OfType<IIfcTypeConstraintProvider>())
                {
                    if (provider is IIdsFacet facet)
                    {
                        if (!facet.IsValid)
                            continue;
                    }
                    if (flt is null)
                        flt = provider.ValidTypes;
                    else
                        flt = flt.Intersect(provider.ValidTypes);
                    if (flt.IsEmpty)
                    {
                        break;
                    }
                }

                validTypes = flt ?? new IfcInheritanceTypeConstraint(
                    IfcConcreteTypeList.SpecialTopClassName, 
                    schemaVersions 
                    );
            }
            return validTypes!;
        }
    }

    protected internal override Audit.Status PerformAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (type == "requirements")
        {
            foreach (var extendedRequirement in Children.OfType<IIdsRequirementFacet>())
            {
                ret |= extendedRequirement.PerformAuditAsRequirement(logger);
            }
        }
        if (!ChildFacets.Any(x=>!x.IsValid) && ValidTypes.IsEmpty)
            ret |= IdsLoggerExtensions.ReportIncompatibleClauses(logger, this, "impossible match of constraints in requirements list");
        return ret;
    }
}
