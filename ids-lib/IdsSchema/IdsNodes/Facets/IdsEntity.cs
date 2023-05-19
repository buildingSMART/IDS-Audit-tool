using IdsLib.IdsSchema.XsNodes;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsEntity : BaseContext, IIfcTypeConstraintProvider, IIdsFacet
{
    public IIfcTypeConstraint? TypesFilter { get; private set; } = null;

    public IdsEntity(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
        IsValid = false;
    }

    public bool IsValid {get; private set;}

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
            return retStatus;        
        var requiredSchemaVersions = spec.SchemaVersions;
        var name = GetChildNodes("name").FirstOrDefault();

        // one child must be a valid string matcher
        var sm = name?.GetListMatcher();
        if (sm is null)
            return IdsMessage.ReportNoStringMatcher(logger, this, "name");
        var ValidClassNames = SchemaInfo.AllClasses
            .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
            .Select(y => y.UpperCaseName);
        var ret = sm.DoesMatch(ValidClassNames, false, logger, out var possibleClasses, "entity names", requiredSchemaVersions);
        if (ret != Audit.Status.Ok)
            return SetInvalid();

        IsValid = true;
        TypesFilter = new IfcConcreteTypeList(possibleClasses);

        // predefined types that are common for the possibleClasses across defined schemas
        var type = GetChildNodes("predefinedType").FirstOrDefault();
        if (type is null)
            return ret;

        var predefinedTypeMatcher = type.GetListMatcher();
        if (predefinedTypeMatcher is null)
            return IdsMessage.ReportNoStringMatcher(logger, this, "predefinedType");

        var schemas = SchemaInfo.GetSchemas(spec.SchemaVersions);
        List<string>? possiblePredefined = null;
        foreach (var s in schemas)
        {
            foreach (var ifcClass in possibleClasses)
            {
                var c = s[ifcClass];
                if (c is null)
                {
                    ret |= IdsToolMessages.ReportUnexpectedScenario(logger, $"class metadata for {ifcClass} not found required schema.", this);
                    continue;
                }
                if (possiblePredefined == null)
                    possiblePredefined = new List<string>(c.PredefinedTypeValues);
                else 
                    possiblePredefined = possiblePredefined.Intersect(c.PredefinedTypeValues).ToList();
            }
        }
        if (possiblePredefined == null)
            ret |= IdsMessage.ReportInvalidDataConfiguration(logger, this, "No valid predefinedType configuration for Entity");
        else
            ret |= predefinedTypeMatcher.DoesMatch(possiblePredefined, false, logger, out var matches, "PredefinedTypes", requiredSchemaVersions);
        
        return ret;
    }

    private Audit.Status SetInvalid()
    {
        TypesFilter = null;
        IsValid = false;
        return Audit.Status.IdsContentError;
    }
}
