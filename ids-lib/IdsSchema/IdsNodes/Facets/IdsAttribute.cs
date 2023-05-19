using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsAttribute : BaseContext, IIdsFacet, IIfcTypeConstraintProvider
{   
    public IdsAttribute(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
        IsValid = false;
    }

    public bool IsValid { get; private set; }

    public IIfcTypeConstraint? TypesFilter { get; private set; } = null;

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        if (!TryGetUpperNodes(this, IdsSpecification.SpecificationIdentificationArray, out var nodes))
        {
			IdsToolMessages.ReportUnexpectedScenario(logger, "Missing specification for attribute.", this);
            return Audit.Status.IdsStructureError;
        }
        if (nodes[0] is not IdsSpecification spec)
        {
			IdsToolMessages.ReportUnexpectedScenario(logger, "Invalid specification for attribute.", this);
            return Audit.Status.IdsContentError;
        }
        var requiredSchemaVersions = spec.SchemaVersions;
        var name = GetChildNodes("name").FirstOrDefault(); // name must exist
        var sm = name?.GetListMatcher();

        // the first child must be a valid string matcher
        if (sm is null)
            return IdsMessage.ReportNoStringMatcher(logger, this, "name");
        
        var validAttributeNames = SchemaInfo.AllAttributes
            .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
            .Select(y => y.IfcAttributeName);
        var ret = sm.DoesMatch(validAttributeNames, false, logger, out var matches, "attribute names", requiredSchemaVersions);
        if (ret != Audit.Status.Ok)
            return SetInvalid();
        IsValid = true;
        // if we have valid attributes we can restrict the valid types depending on them
        TypesFilter = new IfcConcreteTypeList(SchemaInfo.SharedClassesForAttributes(requiredSchemaVersions, matches));
        return ret;
    }
    private Audit.Status SetInvalid()
    {
        TypesFilter = null;
        IsValid = false;
        return Audit.Status.IdsContentError;
    }
}
