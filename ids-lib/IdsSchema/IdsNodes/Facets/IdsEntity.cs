using IdsLib.IdsSchema.XsNodes;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsEntity : IdsXmlNode, IIfcTypeConstraintProvider, IIdsFacet
{
    /// <summary>
    /// prepared typefilters per schema version
    /// </summary>
    private readonly Dictionary<SchemaInfo, IIfcTypeConstraint> typeFilters = new();

	/// <inheritdoc />
	public IIfcTypeConstraint? GetTypesFilter(SchemaInfo schema)
    {
        if (typeFilters.TryGetValue(schema, out var typeFilter)) 
            return typeFilter; 
        return null;
    }

	public IdsEntity(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        IsValid = false;
    }

    private const string PRED_TYPE = "predefinedType";

    public bool IsValid {get; private set;}

    internal protected override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
        if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
            return retStatus;        
        var requiredSchemaVersions = spec.IfcSchemaVersions;
        var name = GetChildNodes("name").FirstOrDefault();

        // name must be a valid string matcher
        var entityNameMatcher = name?.GetListMatcher();
        if (entityNameMatcher is null)
            return IdsErrorMessages.Report102NoStringMatcher(logger, this, "name");

		// we introduce a schema-by-schema evaluation of the valid classes
		//
		requiredSchemaVersions.TryGetSchemaInformation(out var schemas);
        Audit.Status ret = Audit.Status.Ok;
		IsValid = true;

		// preload subtype for efficiency
		var predefinedType = GetChildNodes(PRED_TYPE).FirstOrDefault();
		IStringListMatcher? predefinedTypeMatcher = null;
		if (predefinedType is not null)
		{
			predefinedTypeMatcher = predefinedType.GetListMatcher();

			if (predefinedTypeMatcher is null)
			{
				return IdsErrorMessages.Report102NoStringMatcher(logger, this, PRED_TYPE);
			}
		}

        // we want to make sure that no values in the list can never match (i.e. they have at least one match against the union of schema).
        List<string>? allSchemaClasses = null;
		if (entityNameMatcher is XsRestriction rest)
        {
			allSchemaClasses = SchemaInfo.GetAllClassesFor(schemas).Select(x => x.ToUpperInvariant()).ToList();
			ret |= rest.EachEnumMeaningfulAgainstCandidates(allSchemaClasses, false, logger, "entity name", requiredSchemaVersions);
        }


		if (predefinedTypeMatcher is XsRestriction predRest)
		{
			allSchemaClasses ??= SchemaInfo.GetAllClassesFor(schemas).Select(x => x.ToUpperInvariant()).ToList();
			var allSchemaPredefined = SchemaInfo.GetAllPredefinedTypesFor(schemas, allSchemaClasses).ToList();
			ret |= predRest.EachEnumMeaningfulAgainstCandidates(allSchemaPredefined, false, logger, PRED_TYPE, requiredSchemaVersions);
		}

		foreach (var schema in schemas)
        {
			// we want to make sure that at least one match exist in the name list for each schema	
			var ValidClassNames = schema.Select(y => y.Name.ToUpperInvariant());
			ret |= entityNameMatcher.MustMatchAgainstCandidates(ValidClassNames, false, logger, out var possibleClasses, "entity name", schema.Version);
            if (ret != Audit.Status.Ok)
                continue;
            typeFilters.Add(schema, new IfcConcreteTypeList(possibleClasses));

            // now check predefined types 
            
            if (predefinedType is null || predefinedTypeMatcher is null)
                continue;            
            
            List<string>? possiblePredefined = null;
            foreach (var ifcClass in possibleClasses)
            {
                var c = schema[ifcClass];
                if (c is null)
                {
                    ret |= IdsErrorMessages.Report501UnexpectedScenario(logger, $"class metadata for {ifcClass} not found in schema {schema.Version}.", this);
                    continue;
                }
                if (possiblePredefined == null)
                    possiblePredefined = new List<string>(c.PredefinedTypeValues);
                else
                    possiblePredefined = possiblePredefined.Intersect(c.PredefinedTypeValues).ToList(); // using intersect because it has got to work for all classes matched
            }

            if (possiblePredefined == null)
                ret |= IdsErrorMessages.Report105InvalidDataConfiguration(logger, this, PRED_TYPE);
            else if (possiblePredefined.Contains("USERDEFINED")) // if a user defined option is available then any value is acceptable
                continue;
            else
                // todo: ensure that this notifies an error and that error cases are added for multiple enumeration values
                ret |= predefinedTypeMatcher.MustMatchAgainstCandidates(possiblePredefined, false, logger, out var matches, PRED_TYPE, schema.Version);
        }
        if (ret != Status.Ok)
        {
            typeFilters.Clear();
            IsValid = false;
        }
        return ret;
    }

   
}
