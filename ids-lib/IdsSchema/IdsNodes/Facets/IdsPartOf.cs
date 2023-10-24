using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsPartOf : IdsXmlNode, IIdsCardinalityFacet, IIfcTypeConstraintProvider
{
	private const string relationXmlAttributeName = "relation";
	private readonly RequirementCardinality cardinality;
	private readonly string relationValue;
	
    /// <summary>
	/// prepared typefilters per schema version
	/// </summary>
	private readonly Dictionary<SchemaInfo, IIfcTypeConstraint> typeFilters = new();

	/// <inheritdoc />
	public IIfcTypeConstraint? GetTypesFilter(SchemaInfo schema)
	{
		if (typeFilters.TryGetValue(schema, out var typeFilter))
			return typeFilter;
		return null;
	}

	public bool IsRequired => cardinality.IsRequired;

    public IdsPartOf(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
	{
        cardinality = new RequirementCardinality(reader);
		relationValue = reader.GetAttribute(relationXmlAttributeName) ?? string.Empty;
	}

    public bool IsValid { get; private set; } = true;

    internal protected override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
        IsValid = false;
        if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
            return retStatus;
        var ret = Audit.Status.Ok;
        var requiredSchemaVersions = spec.IfcSchemaVersions;
        // relation child is always a valid string matcher

        // if the facet is not required we don't check if it makes sense semantically
        if (!IsRequired)
        {
            IsValid = true;
            typeFilters.Clear();
            return ret;
        }

		requiredSchemaVersions.TryGetSchemaInformation(out var schemas);
        foreach (var schema in schemas)
        {
			if (!string.IsNullOrEmpty(relationValue))
			{
				var relMatcher = new StringListMatcher(relationValue, this);
				var possibleRelationNames = schema.AllPartOfRelations.Select(y => y.RelationIfcName);
				// this triggers a log error if there's anything but a single match
				ret |= relMatcher.HasSingleMatch(possibleRelationNames, false, logger, out var matchedRelationName, "relation name", schema.Version);

				// if we have a match then there are other constraints we can evaluate on the
				// types of both sides of the relation
				//
				if (matchedRelationName is null)
					return SetInvalid();
				var relationInfo = schema.AllPartOfRelations.FirstOrDefault(x => x.RelationIfcName == matchedRelationName);
				if (relationInfo is null)
				{
					ret |= IdsErrorMessages.Report501UnexpectedScenario(logger, $"no valid relation found for {matchedRelationName}", this);
					return SetInvalid(ret);
				}

				// Entities of the partOf need to be of type of relationInfo.PartIfcType
				// 
				var filter = new IfcInheritanceTypeConstraint(relationInfo.PartIfcType, schema.Version);
				if (IfcTypeConstraint.IsNotNullAndEmpty(filter))
				{
					ret |= IdsErrorMessages.Report501UnexpectedScenario(logger, $"no valid types found for {relationInfo.PartIfcType}", this);
					return SetInvalid(ret);
				}
				typeFilters.Add(schema, filter);

				// The entity needs to be of type of relationInfo.OwnerIfcType
				//
				if (GetChildNodes("entity").FirstOrDefault() is not IIfcTypeConstraintProvider childEntity)
				{
					ret |= IdsErrorMessages.Report106InvalidEmtpyValue(logger, this, "entity");
					return SetInvalid(ret);
				}

				var validChildEntityType = new IfcInheritanceTypeConstraint(relationInfo.OwnerIfcType, schema.Version);
				var possible = validChildEntityType.Intersect(childEntity.GetTypesFilter(schema));
				if (possible.IsEmpty)
				{
					ret |= IdsErrorMessages.Report201IncompatibleClauses(logger, this, schema, "relation not compatible with provided child entity");
					return SetInvalid(ret);
				}
			}
        }
		IsValid = true;
		return ret;
	}

	public Audit.Status PerformCardinalityAudit(ILogger? logger)
	{
		var ret = Audit.Status.Ok;
		if (cardinality.Audit(out var _) != Audit.Status.Ok)
		{
			IdsErrorMessages.Report301InvalidCardinality(logger, this, cardinality);
			ret |= MinMaxCardinality.ErrorStatus;
		}
		return ret;
	}
    private Audit.Status SetInvalid(Audit.Status status = Audit.Status.IdsContentError)
    {
		typeFilters.Clear();
		IsValid = false;
        return status;
    }

}
