using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacetCollection : IdsXmlNode, IIfcTypeConstraintProvider
{
    public IdsFacetCollection(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
    }

    private IIfcTypeConstraint? typeFilter = null;
    internal IEnumerable<IIdsFacet> ChildFacets => Children.OfType<IIdsFacet>();

    private bool typesFilterInitialized = false;

    public IIfcTypeConstraint? GetTypesFilter(SchemaInfo schema)
	{   
        if (!typesFilterInitialized) 
        {
            typeFilter = null;
            foreach (var provider in Children.OfType<IIfcTypeConstraintProvider>())
            {
                if (provider is IIdsCardinalityFacet card && !card.IsRequired)
                    continue;
                typeFilter = IfcTypeConstraint.Intersect(typeFilter, provider.GetTypesFilter(schema));
                if (IfcTypeConstraint.IsNotNullAndEmpty(typeFilter))
                    break;                    
            }
            typesFilterInitialized = true;
        }
        return typeFilter;
        
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
		if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
			return retStatus;
		var requiredSchemaVersions = spec.SchemaVersions;
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
