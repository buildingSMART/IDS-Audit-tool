using IdsLib.IdsSchema.Cardinality;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacetCollection : IdsXmlNode, IIfcTypeConstraintProvider
{
	internal readonly ICardinality Cardinality;

	public IdsFacetCollection(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        // this is only relevant for applicability, but we can attempt to read it in any case.
		Cardinality = new MinMaxCardinality(reader);
	}

    private Dictionary<IfcSchemaVersions, IIfcTypeConstraint?> typeFilterDic = new();
    internal IEnumerable<IIdsFacet> ChildFacets => Children.OfType<IIdsFacet>();

    public IIfcTypeConstraint? GetTypesFilter(SchemaInfo schema)
	{   
        if (typeFilterDic.TryGetValue(schema.Version, out var val))
        {
            return val;
        }

        IIfcTypeConstraint? typeFilter = null;
        foreach (var provider in Children.OfType<IIfcTypeConstraintProvider>())
        {
            if (provider is IIdsCardinalityFacet card && !card.IsRequired)
                continue;
            typeFilter = IfcTypeConstraint.Intersect(typeFilter, provider.GetTypesFilter(schema));
            if (IfcTypeConstraint.IsNotNullAndEmpty(typeFilter))
                break;
        }
        typeFilterDic.Add(schema.Version, typeFilter);
        return typeFilter;
    }

    protected internal override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (type == "requirements")
        {
            foreach (var extendedRequirement in Children.OfType<IIdsCardinalityFacet>())
            {
                ret |= extendedRequirement.PerformCardinalityAudit(logger);
            }
        }
        else if (type == "applicability")
        {
            if (Cardinality.Audit(out var _) != Audit.Status.Ok)
            {
                ret |= IdsErrorMessages.Report301InvalidCardinality(logger, this, Cardinality);
            }
        }

		if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
			return retStatus;
		var requiredSchemaVersions = spec.IfcSchemaVersions;
        if (requiredSchemaVersions.TryGetSchemaInformation(out var schemaInfos))
        {
            foreach (var schemaInfo in schemaInfos)
            {
				if (!ChildFacets.Any(x => !x.IsValid) && IfcTypeConstraint.IsNotNullAndEmpty(GetTypesFilter(schemaInfo)))
					ret |= IdsErrorMessages.Report201IncompatibleClauses(logger, this, schemaInfo, "impossible match of constraints in set");
			}
        }
        return ret;
    }
}
