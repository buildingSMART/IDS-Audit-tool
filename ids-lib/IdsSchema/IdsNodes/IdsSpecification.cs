using IdsLib.IdsSchema.Cardinality;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Linq;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsSpecification : IdsXmlNode
{
    internal const string NodeSignature = "specification";

	internal static readonly string[] SpecificationIdentificationArray = { NodeSignature };

    internal readonly IfcSchemaVersions IfcSchemaVersions = IfcSchemaVersions.IfcNoVersion;

    private readonly IdsXmlNode? parent;
    protected override internal IdsXmlNode? Parent => parent;
    
    public IdsSpecification(System.Xml.XmlReader reader, IdsXmlNode? parent, ILogger? logger) : base(reader, null)
    {
        this.parent = parent;

        var vrs = reader.GetAttribute("ifcVersion") ?? string.Empty;
        IfcSchemaVersions = vrs.GetSchemaVersions(this, logger);
    }

    internal protected override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
        var ret = Audit.Status.Ok;

        if (IfcSchemaVersions == IfcSchemaVersions.IfcNoVersion)
            ret |= IdsErrorMessages.Report107InvalidIfcSchemaVersion(logger, IfcSchemaVersions, this);
        var applic = GetChildNode<IdsFacetCollection>("applicability");
        if (applic is null)
        {
            ret |= IdsErrorMessages.Report101InvalidApplicability(logger, this, "applicability not found");
            return ret;
        }
        if (!applic.ChildFacets.Any())
        {
            ret |= IdsErrorMessages.Report101InvalidApplicability(logger, this, "One condition is required at minimum");
            return ret;
        }

        // Evaluation of compatibility between Requirement and Applicability
        //
        var reqs = GetChildNode<IdsFacetCollection>("requirements");
        if (applic.Cardinality.IsProhibited)
        {
            if (reqs is not null && reqs.ChildFacets.Any()) 
            {
				// trigger incompatible clauses
				ret |= IdsErrorMessages.Report204IncompatibleRequirements(logger, this, "requirements are not allowed when applicability is prohibited.");
			}
        }

        if (reqs is not null)
        {
            if (IfcSchemaVersions.TryGetSchemaInformation(out var schemaInfos))
            {
                foreach (var schemaInfo in schemaInfos)
                {
                    var aF = applic.GetTypesFilter(schemaInfo);
                    var rF = reqs.GetTypesFilter(schemaInfo);
					if (
                        !IfcTypeConstraint.IsNotNullAndEmpty(aF) // if they are empty an error would already be notified
                        && !IfcTypeConstraint.IsNotNullAndEmpty(rF)
                        )
                    {
                        var totalFilters = IfcTypeConstraint.Intersect(aF, rF);
                        if (IfcTypeConstraint.IsNotNullAndEmpty(totalFilters))
                        {
                            var appDesc = aF.ConcreteTypes.Count() > 5
                                ? $"{aF.ConcreteTypes.Count()} types"
                                : $"{string.Join(", ", aF.ConcreteTypes)}";
							var reqDesc = rF.ConcreteTypes.Count() > 5
								? $"{rF.ConcreteTypes.Count()} types"
								: $"{string.Join(", ", rF.ConcreteTypes)}";
							ret |= IdsErrorMessages.Report201IncompatibleClauses(logger, this, schemaInfo, $"impossible match of types between applicability ({appDesc}) and requirements ({reqDesc})");
                        }
                    }
                }
            }
        }

        return base.PerformAudit(stateInfo, logger) | ret;
    }
}
