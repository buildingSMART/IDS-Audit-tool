using IdsLib.IdsSchema.XsNodes;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            return IdsMessages.Report102NoStringMatcher(logger, this, "name");

        // we introduce a schema-by-schema evaluation of the valid classes
        //
        requiredSchemaVersions.TryGetSchemaInformation(out var schemas);
        Audit.Status ret = Audit.Status.Ok;
		IsValid = true;
		foreach (var schema in schemas)
        {
            var ValidClassNames = schema
                .Select(y => y.Name.ToUpperInvariant());
			ret |= sm.DoesMatch(ValidClassNames, false, logger, out var possibleClasses, "entity names", schema.Version);
            if (ret != Audit.Status.Ok)
                return SetInvalid();
            typeFilters.Add(schema, new IfcConcreteTypeList(possibleClasses));

            // predefined types that are common for the possibleClasses across defined schemas
            var predefinedType = GetChildNodes("predefinedType").FirstOrDefault();
            if (predefinedType is null)
                continue;

            var predefinedTypeMatcher = predefinedType.GetListMatcher();
            if (predefinedTypeMatcher is null)
                return IdsMessages.Report102NoStringMatcher(logger, this, "predefinedType");

            
            List<string>? possiblePredefined = null;
            foreach (var ifcClass in possibleClasses)
            {
                var c = schema[ifcClass];
                if (c is null)
                {
                    ret |= IdsMessages.Report501UnexpectedScenario(logger, $"class metadata for {ifcClass} not found in schema {schema.Version}.", this);
                    continue;
                }
                if (possiblePredefined == null)
                    possiblePredefined = new List<string>(c.PredefinedTypeValues);
                else
                    possiblePredefined = possiblePredefined.Intersect(c.PredefinedTypeValues).ToList();
            }
            
            if (possiblePredefined == null)
                ret |= IdsMessages.Report105InvalidDataConfiguration(logger, this, "predefinedType");
            else if (possiblePredefined.Contains("USERDEFINED")) // if a user defined option is available then any value is acceptable
                return ret;
            else
                // todo: ensure that this notifies an error and that error cases are added for multiple enumeration values
                ret |= predefinedTypeMatcher.DoesMatch(possiblePredefined, false, logger, out var matches, "PredefinedTypes", schema.Version);
        }
        return ret;
    }

    private Audit.Status SetInvalid()
    {
        typeFilters.Clear();
        IsValid = false;
        return Audit.Status.IdsContentError;
    }
}
