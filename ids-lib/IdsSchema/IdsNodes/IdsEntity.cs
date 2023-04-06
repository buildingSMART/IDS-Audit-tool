using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsEntity : BaseContext
{
    private static readonly string[] SpecificationArray = { "specification" };

    public IdsEntity(System.Xml.XmlReader reader) : base(reader)
    {
    }

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        if (!TryGetUpperNodes(this, SpecificationArray, out var nodes))
        {
            IdsLoggerExtensions.ReportUnexpectedScenario(logger, "Missing specification for entity.", this);
            return Audit.Status.IdsStructureError;
        }
        if (nodes[0] is not IdsSpecification spec)
        {
            IdsLoggerExtensions.ReportUnexpectedScenario(logger, "Invalid specification for entity.", this);
            return Audit.Status.IdsContentError;
        }
        var requiredSchemaVersions = spec.SchemaVersions;
        var name = GetChildNodes("name").FirstOrDefault();

        // one child must be a valid string matcher
        var sm = name?.GetListMatcher();
        if (sm is null)
            return IdsLoggerExtensions.ReportNoStringMatcher(logger, this, "name");
        var ValidClassNames = SchemaInfo.AllClasses
            .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
            .Select(y => y.IfcClassName.ToUpperInvariant());
        var ret = sm.DoesMatch(ValidClassNames, false, logger, out var possibleClasses, "entity names", requiredSchemaVersions);
        if (ret != Audit.Status.Ok)   
            return ret;
        

        // predefined types that are common for the possibleClasses across defined schemas
        var type = GetChildNodes("predefinedType").FirstOrDefault();
        if (type is null)
            return ret;

        var predefinedTypeMatcher = type.GetListMatcher();
        if (predefinedTypeMatcher is null)
            return IdsLoggerExtensions.ReportNoStringMatcher(logger, this, "predefinedType");

        var schemas = SchemaInfo.GetSchemas(spec.SchemaVersions);
        List<string>? possiblePredefined = null;
        foreach (var s in schemas)
        {
            foreach (var ifcClass in possibleClasses)
            {
                var c = s[ifcClass];
                if (c is null)
                {
                    ret |= IdsLoggerExtensions.ReportUnexpectedScenario(logger, $"class metadata for {ifcClass} not found required schema.", this);
                    continue;
                }
                if (possiblePredefined == null)
                    possiblePredefined = new List<string>(c.PredefinedTypeValues);
                else 
                    possiblePredefined = possiblePredefined.Intersect(c.PredefinedTypeValues).ToList();
            }
        }
        if (possiblePredefined == null)
            ret |= IdsLoggerExtensions.ReportInvalidDataConfiguration(logger, this, "No valid predefinedType configuration for Entity");
        else
            ret |= predefinedTypeMatcher.DoesMatch(possiblePredefined, false, logger, out var matches, "PredefinedTypes", requiredSchemaVersions);
        
        return ret;
    }
}
