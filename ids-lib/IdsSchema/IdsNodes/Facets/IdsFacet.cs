using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes;

/// <summary>
/// This is used for Classification and Material
/// </summary>
internal class IdsFacet : IdsXmlNode, IIdsCardinalityFacet, IIfcTypeConstraintProvider
{
    private readonly MinMaxOccur minMaxOccurr;
    

    public IdsFacet(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        minMaxOccurr = new MinMaxOccur(reader);
    }

    public bool IsValid { get; private set; } = true;

    public bool IsRequired => minMaxOccurr.IsRequired;

    private IIfcTypeConstraint? typesFilter;
    public IIfcTypeConstraint? TypesFilter
    {
        get
        {
            if (!IsRequired)
                return null;
            return typesFilter;
        }

        private set => typesFilter = value;
    }

    protected internal override Audit.Status PerformAudit(ILogger? logger)
    {
        if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
            return retStatus;
        var requiredSchemaVersions = spec.SchemaVersions;
        if (IsRequired)
            TypesFilter = new IfcConcreteTypeList(SchemaInfo.GetRelAsssignClasses(requiredSchemaVersions));
        return base.PerformAudit(logger);
    }

    public Audit.Status PerformCardinalityAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (minMaxOccurr.Audit(out var _) != Audit.Status.Ok)
        {
			IdsMessage.ReportInvalidOccurr(logger, this, minMaxOccurr);
            ret |= MinMaxOccur.ErrorStatus;
            IsValid = false;
        }
        return ret;
    }
}
