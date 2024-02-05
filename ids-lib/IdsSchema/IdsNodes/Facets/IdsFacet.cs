using IdsLib.IdsSchema.Cardinality;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.IdsNodes;

/// <summary>
/// This is used for Classification and Material
/// </summary>
internal class IdsFacet : IdsXmlNode, IIdsCardinalityFacet, IIfcTypeConstraintProvider
{
    private readonly ICardinality cardinality;
    
    public IdsFacet(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        cardinality = new ConditionalCardinality(reader);
    }

	/// <inheritdoc />
	public bool IsValid { get; private set; } = true;

	/// <inheritdoc />
	public bool IsRequired => cardinality.IsRequired;

	/// <inheritdoc />
	public IIfcTypeConstraint? GetTypesFilter(SchemaInfo schema)
	{
		if (!IsRequired)
			return null;
		if (type == "classification")
			return new IfcConcreteTypeList(schema.GetRelAsssignClassificationClasses());
		else
			return new IfcConcreteTypeList(schema.GetRelAsssignClasses());
	}

    protected internal override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
        return base.PerformAudit(stateInfo, logger);
    }

	/// <inheritdoc />
	public Audit.Status PerformCardinalityAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (cardinality.Audit(out var _) != Audit.Status.Ok)
        {
			IdsErrorMessages.Report301InvalidCardinality(logger, this, cardinality);
            ret |= MinMaxCardinality.CardinalityErrorStatus;
            IsValid = false;
        }
        return ret;
    }
}
