using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsPartOf : BaseContext, IIdsRequirementFacet, IIfcTypeConstraintProvider
{
	private static readonly string[] SpecificationArray = { "specification" };
	private readonly MinMaxOccur minMaxOccurr;
	private readonly string relation;
	private IIfcTypeConstraint validTypes;

	public IdsPartOf(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
	{
        minMaxOccurr = new MinMaxOccur(reader);
		relation = reader.GetAttribute("relation") ?? string.Empty;
		validTypes = IfcConcreteTypeList.Empty;
	}

    public bool IsValid { get; private set; } = true;

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        IsValid = false;
        if (!TryGetUpperNode<IdsSpecification>(logger, this, SpecificationArray, out var spec, out var retStatus))
            return retStatus;
        var ret = Audit.Status.Ok;
        var requiredSchemaVersions = spec.SchemaVersions;
        if (string.IsNullOrEmpty(relation))
        {
            ret |= IdsLoggerExtensions.ReportInvalidEmtpyValue(logger, this, "relation");
            return ret;
        }

        // relation child is always a valid string matcher
        var relMatcher = new StringListMatcher(relation, this);
        var possibleRelationNames = SchemaInfo.AllPartOfRelations
            .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
            .Select(y => y.IfcName);
        // this triggers a log error if there's anything but a single match
        ret |= relMatcher.HasSingleMatch(possibleRelationNames, false, logger, out var matchedRelationName, "relation names", requiredSchemaVersions);

        // if we have a match then there are other constraints we can evaluate on the
        // types of both sides of the relation
        //
        if (matchedRelationName is null)
        {
            return ret;
        }
        var relationInfo = SchemaInfo.AllPartOfRelations.FirstOrDefault(x => x.IfcName == matchedRelationName);
        if (relationInfo is null)
        {
            ret |= IdsLoggerExtensions.ReportUnexpectedScenario(logger, $"no valid relation found for {matchedRelationName}", this);
            return ret;
        }

        // Entities of the partOf need to be of type of relationInfo.ManySideIfcType
        validTypes = new IfcInheritanceTypeConstraint(relationInfo.ManySideIfcType, requiredSchemaVersions);
        if (validTypes.IsEmpty)
        {
            ret |= IdsLoggerExtensions.ReportUnexpectedScenario(logger, $"no valid types found for {relationInfo.ManySideIfcType}", this);
            return ret;
        }

        // The entity needs to be of type of relationInfo.OneSideIfcType
        if (GetChildNodes("entity").FirstOrDefault() is not IIfcTypeConstraintProvider childEntity)
        {
            ret |= IdsLoggerExtensions.ReportInvalidEmtpyValue(logger, this, "entity");
            return ret;
        }

        IsValid = true;
        var validChildEntityType = new IfcInheritanceTypeConstraint(relationInfo.OneSideIfcType, requiredSchemaVersions);
        var possible = validChildEntityType.Intersect(childEntity.ValidTypes);
        if (possible.IsEmpty)
            ret |= IdsLoggerExtensions.ReportIncompatibleClauses(logger, this, "relation not compatible with provided child entity");

        return ret;
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

	public IIfcTypeConstraint ValidTypes => validTypes;
}
