using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsPartOf : BaseContext, IIdsCardinalityFacet, IIfcTypeConstraintProvider
{
	private readonly MinMaxOccur minMaxOccurr;
	private readonly string relation;
    
    public IIfcTypeConstraint? TypesFilter { get; private set; }

    public bool IsRequired => minMaxOccurr.IsRequired;

    public IdsPartOf(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
	{
        minMaxOccurr = new MinMaxOccur(reader);
		relation = reader.GetAttribute("relation") ?? string.Empty;
	}

    public bool IsValid { get; private set; } = true;

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        IsValid = false;
        if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
            return retStatus;
        var ret = Audit.Status.Ok;
        var requiredSchemaVersions = spec.SchemaVersions;
        if (string.IsNullOrEmpty(relation))
        {
            ret |= IdsMessage.ReportInvalidEmtpyValue(logger, this, "relation");
            return ret;
        }

        // relation child is always a valid string matcher
        var relMatcher = new StringListMatcher(relation, this);
        var possibleRelationNames = SchemaInfo.AllPartOfRelations
            .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
            .Select(y => y.IfcName);
        // this triggers a log error if there's anything but a single match
        ret |= relMatcher.HasSingleMatch(possibleRelationNames, false, logger, out var matchedRelationName, "relation names", requiredSchemaVersions);

        // if the facet is not required we don't check if it makes sense semantically
        if (!IsRequired)
        {
            IsValid = true;
            TypesFilter = null;
        }

        // if we have a match then there are other constraints we can evaluate on the
        // types of both sides of the relation
        //
        if (matchedRelationName is null)
            return SetInvalid();
        var relationInfo = SchemaInfo.AllPartOfRelations.FirstOrDefault(x => x.IfcName == matchedRelationName);
        if (relationInfo is null)
        {
            ret |= IdsToolMessages.ReportUnexpectedScenario(logger, $"no valid relation found for {matchedRelationName}", this);
            return SetInvalid(ret);
        }

        // Entities of the partOf need to be of type of relationInfo.ManySideIfcType
        TypesFilter = new IfcInheritanceTypeConstraint(relationInfo.ManySideIfcType, requiredSchemaVersions);
        if (IfcTypeConstraint.IsNotNullAndEmpty(TypesFilter))
        {
            ret |= IdsToolMessages.ReportUnexpectedScenario(logger, $"no valid types found for {relationInfo.ManySideIfcType}", this);
            return SetInvalid(ret);
        }

        // The entity needs to be of type of relationInfo.OneSideIfcType
        if (GetChildNodes("entity").FirstOrDefault() is not IIfcTypeConstraintProvider childEntity)
        {
            ret |= IdsMessage.ReportInvalidEmtpyValue(logger, this, "entity");
            return SetInvalid(ret);
        }

        IsValid = true;
        var validChildEntityType = new IfcInheritanceTypeConstraint(relationInfo.OneSideIfcType, requiredSchemaVersions);
        var possible = validChildEntityType.Intersect(childEntity.TypesFilter);
        if (possible.IsEmpty)
        {
            ret |= IdsMessage.ReportIncompatibleClauses(logger, this, "relation not compatible with provided child entity");
            return SetInvalid(ret);
        }
        return ret;
    }

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
    private Audit.Status SetInvalid(Audit.Status status = Audit.Status.IdsContentError)
    {
        TypesFilter = null;
        IsValid = false;
        return status;
    }

}
