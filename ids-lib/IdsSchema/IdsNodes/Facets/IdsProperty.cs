using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsProperty : BaseContext, IIdsCardinalityFacet, IIfcTypeConstraintProvider
{
    private readonly MinMaxOccur minMaxOccurr;
    private readonly IStringListMatcher? measureMatcher;
    public IdsProperty(System.Xml.XmlReader reader, BaseContext? parent) : base(reader, parent)
    {
        minMaxOccurr = new MinMaxOccur(reader);
        var measure = reader.GetAttribute("measure") ?? string.Empty;
        if (!string.IsNullOrEmpty(measure))
            measureMatcher = new StringListMatcher(measure, this);
        else
            measureMatcher = null;
    }

    public bool IsValid { get; private set; } = false;

    public bool IsRequired => minMaxOccurr.IsRequired; 

    public IIfcTypeConstraint? TypesFilter {get; private set;}

    internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
        if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
            return retStatus;
        var requiredSchemaVersions = spec.SchemaVersions;
        IsValid = false;

        // property set and name are compulsory
        var pset = GetChildNodes("propertySet").FirstOrDefault();
        var psetMatcher = pset?.GetListMatcher();
        if (psetMatcher is null)
            return IdsMessage.ReportNoStringMatcher(logger, this, "propertySet");

        var name = GetChildNodes("name").FirstOrDefault();
        var nameMatcher = name?.GetListMatcher();
        if (nameMatcher is null)
            return IdsMessage.ReportNoStringMatcher(logger, this, "name");

        // we are keeping the stricter type to ensure that it is valid across multiple schemas
        // depending on the schema version of IfcRelDefinesByProperties the filter needs to be
        // - IfcObject in ifc2x3 
        // - IfcObjectDefinition in Ifc4 
        // - IfcObjectDefinition in ifc4x3
        // 
        TypesFilter = requiredSchemaVersions.HasFlag(IfcSchemaVersions.Ifc2x3)
            ? new IfcInheritanceTypeConstraint("IFCOBJECT", requiredSchemaVersions)
            : new IfcInheritanceTypeConstraint("IFCOBJECTDEFINITION", requiredSchemaVersions);

        // initiate valid measures, will constrain later if there's a known property
        var validMeasureNames = SchemaInfo.AllMeasures
                .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
                .Select(y => y.IfcMeasureClassName);
        IsValid = true;


        if (IsRequired)
        {
            var validPsetNames = SchemaInfo.SharedPropertySetNames(requiredSchemaVersions);
            if (psetMatcher.TryMatch(validPsetNames, false, out var possiblePsetNames))
            {
                // see if there's a match with standard property sets
                var validPropNames = SchemaInfo.SharedPropertyNames(requiredSchemaVersions, possiblePsetNames);
                var nameMatch = nameMatcher.DoesMatch(validPropNames, false, logger, out var possiblePropertyNames, "property names", requiredSchemaVersions);
                if (nameMatch != Audit.Status.Ok)
                    return SetInvalid();

                // limit the validity of the IfcMeasure to the value coming from the metadata for the property
                var limit = SchemaInfo.ValidMeasureForAllProperties(requiredSchemaVersions, possiblePsetNames, possiblePropertyNames);
                if (limit is null)
                    validMeasureNames = Enumerable.Empty<string>();                
                else
                    validMeasureNames = new string[] { limit };
                
                // limit the validity of the type
                var validTypes = SchemaInfo.PossibleTypesForPropertySets(requiredSchemaVersions, possiblePsetNames);
                TypesFilter = new IfcConcreteTypeList(validTypes);
            }
            else if (psetMatcher is IStringPrefixMatcher ssm && ssm.MatchesPrefix("Pset_"))
            {
                IsValid = false;
                TypesFilter = null;
                return IdsMessage.ReportReservedStringMatched(logger, this, "prefix 'Pset_'", "property set name");
            }
            else
            {
                // todo: optional strict rule to implement on property name
                // we can check if the property name is one of the standard and suggest to move it to the standard pset
                // this could be done via the IFiniteStringMatcher
            }
        }
        else
        {
            TypesFilter = null;
            IsValid = true;
        }

        var ret = Audit.Status.Ok;
        if (measureMatcher is not null)
        {
            ret |= measureMatcher.DoesMatch(validMeasureNames, false, logger, out var matches, "measure names", requiredSchemaVersions);
        }
        if (ret != Audit.Status.Ok)
            IsValid = false;
        return ret;
    }

    private Audit.Status SetInvalid()
    {
        TypesFilter = null;
        IsValid = false;
        return Audit.Status.IdsContentError;
    }

    public Audit.Status PerformCardinalityAudit(ILogger? logger)
    {
        if (minMaxOccurr.Audit(out var _) != Audit.Status.Ok)
            return logger.ReportInvalidOccurr(this, minMaxOccurr);
        return Audit.Status.Ok;
    }
}
