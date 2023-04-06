using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsSpecification : BaseContext
{
    private readonly MinMaxOccur minMaxOccurr;
    internal readonly IfcSchemaVersions SchemaVersions = IfcSchemaVersions.IfcNoVersion;

    private BaseContext? parent;
    protected override internal BaseContext? Parent
    {
        get => parent;
        set
        {
            // we are not storing a specification node in the parent's children to reduce memory footprint,
            // becasue this allows the GC to collect a specification, when dereferenced
            // 
            parent = value;
        }
    }


    public IdsSpecification(System.Xml.XmlReader reader, ILogger? logger) : base(reader)
    {
        minMaxOccurr = new MinMaxOccur(reader);
        var vrs = reader.GetAttribute("ifcVersion") ?? string.Empty;
        SchemaVersions = vrs.GetSchemaVersions(this, logger);
    }

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (minMaxOccurr.Audit(out var _) != Audit.Status.Ok)
            ret |= logger.ReportInvalidOccurr(this, minMaxOccurr);
        if (SchemaVersions == IfcSchemaVersions.IfcNoVersion)
            ret |= logger.ReportInvalidSchemaVersion(SchemaVersions, this);
        return base.PerformAudit(logger) | ret;
    }
}
