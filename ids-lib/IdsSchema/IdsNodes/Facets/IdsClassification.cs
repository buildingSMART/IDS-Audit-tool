using IdsLib.IdsSchema.Cardinality;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.IdsNodes;

/// <summary>
/// This is used for Classification and Material
/// </summary>
internal class IdsClassification : IdsXmlNode, IIdsCardinalityFacet, IIfcTypeConstraintProvider
{
    private readonly ICardinality cardinality;

    /// <summary>
    /// value is used when evaluating cardinality for requirements
    /// </summary>
    private IdsXmlNode? value { get; set; } = null;

    public IdsClassification(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
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
		return new IfcConcreteTypeList(schema.GetRelAsssignClassificationClasses());		
	}

    protected internal override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        var system = GetChildNodes("system").FirstOrDefault();
        value = GetChildNodes("value").FirstOrDefault();

        if (IdsSimpleValue.IsNullOrEmpty(system))
        {
            ret |= IdsErrorMessages.Report106InvalidEmtpyValue(logger, this, nameof(system));
        }
        
        ret |= base.PerformAudit(stateInfo, logger);
        return ret;
    }

	/// <inheritdoc />
	public Audit.Status PerformCardinalityAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (cardinality.Audit(out var _) != Audit.Status.Ok)
        {
			IdsErrorMessages.Report301InvalidCardinality(logger, this, cardinality);
            ret |= CardinalityConstants.CardinalityErrorStatus;
            IsValid = false;
        }
        else
        {
            if (cardinality is ConditionalCardinality crd && crd.enumerationValue == "optional" && IdsSimpleValue.IsNullOrEmpty(value))
            {
                IdsErrorMessages.Report202InvalidCardinalityContext(logger, this, cardinality, crd.enumerationValue, "it requires the specification of the 'value' constraint");
                ret |= CardinalityConstants.CardinalityErrorStatus;
                IsValid = false;
            }
        }
        return ret;
    }
}
