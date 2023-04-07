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

	public IdsPartOf(System.Xml.XmlReader reader) : base(reader)
	{
		minMaxOccurr = new MinMaxOccur(reader);
		relation = reader.GetAttribute("relation") ?? string.Empty;
		validTypes = IfcConcreteTypeList.Empty;
	}

	internal protected override Audit.Status PerformAudit(ILogger? logger)
	{
		var ret = Audit.Status.Ok;
		if (!TryGetUpperNodes(this, SpecificationArray, out var nodes))
		{
			IdsLoggerExtensions.ReportUnexpectedScenario(logger, "Missing specification for attribute.", this);
			return Audit.Status.IdsStructureError;
		}
		if (nodes[0] is not IdsSpecification spec)
		{
			IdsLoggerExtensions.ReportUnexpectedScenario(logger, "Invalid specification for attribute.", this);
			return Audit.Status.IdsContentError;
		}
		var requiredSchemaVersions = spec.SchemaVersions;
		if (string.IsNullOrEmpty(relation))
			ret |= IdsLoggerExtensions.ReportInvalidEmtpyValue(logger, this, "relation");
		else
		{
			// relation child is always a valid string matcher
			var relMatcher = new StringListMatcher(relation, this);
			var possibleRelationNames = SchemaInfo.AllPartOfRelations
				.Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
				.Select(y => y.IfcName.ToUpperInvariant());
			// this triggers a log error if there's anything but a single match
			ret |= relMatcher.HasSingleMatch(possibleRelationNames, false, logger, out var matchedRelationName, "relation names", requiredSchemaVersions);

			// if we have a match then there are other constraints we can evaluate on the
			// types of both sides of the relation
			//
			if (matchedRelationName is not null)
			{
				var relationInfo = SchemaInfo.AllPartOfRelations.First(x => x.IfcName == matchedRelationName);

				// Entities of the partOf need to be of type of relationInfo.ManySideIfcType
				validTypes = new IfcInheritanceTypeConstraint(relationInfo.ManySideIfcType, requiredSchemaVersions);

                // The entity needs to be of type of relationInfo.OneSideIfcType
                var validChildEnityType = new IfcInheritanceTypeConstraint(relationInfo.ManySideIfcType, requiredSchemaVersions);
				// todo: this is incomplete and also needs to add testing
				ret |= Audit.Status.NotImplementedError;
            }
		}
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
