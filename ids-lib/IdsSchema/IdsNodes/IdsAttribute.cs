using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsAttribute : BaseContext, IIdsRequirementFacet
{
    private static readonly string[] SpecificationArray = { "specification" };
    
    public IdsAttribute(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
    }

    public bool IsValid { get; private set; }

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
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
        var names = GetChildNodes("name");
        
        var ret = Audit.Status.Ok;
        foreach (var name in names)
        {
            // the first child must be a valid string matcher
            if (!name.Children.Any())
                return IdsLoggerExtensions.ReportNoStringMatcher(logger, this, "name");
            if (name.Children[0] is not IStringListMatcher sm)
                return IdsLoggerExtensions.ReportInvalidStringMatcher(logger, name.Children[0], "name");
            var ValidClassNames = SchemaInfo.AllAttributes
                .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
                .Select(y => y.IfcAttributeName);
            var result = sm.DoesMatch(ValidClassNames, false, logger, out var matches, "attribute names", requiredSchemaVersions);
            ret |= result;
        }
        return ret;
    }

    public Audit.Status PerformAuditAsRequirement(ILogger? logger)
    {
        return Audit.Status.Ok;
    }
}
