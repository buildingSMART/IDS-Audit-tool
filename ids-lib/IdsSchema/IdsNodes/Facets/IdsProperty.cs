using IdsLib.IdsSchema.Cardinality;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsProperty : IdsXmlNode, IIdsCardinalityFacet, IIfcTypeConstraintProvider
{
    private readonly ICardinality cardinality;
    private readonly IStringListMatcher? dataTypeMatcher;
    private readonly string dataType;
    public IdsProperty(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        cardinality = new ConditionalCardinality(reader);
        dataType = reader.GetAttribute("dataType") ?? string.Empty;
        if (!string.IsNullOrEmpty(dataType))
            dataTypeMatcher = new StringListMatcher(dataType, this);
        else
            dataTypeMatcher = null;
    }

    public bool IsValid { get; private set; } = false;

    /// <summary>
    /// value is used when evaluating cardinality for requirements
    /// </summary>
    private IdsXmlNode? value { get; set; } = null;

    public bool IsRequired => cardinality.IsRequired;

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

	internal protected override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
        if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
            return retStatus;
        var requiredSchemaVersions = spec.IfcSchemaVersions;
        IsValid = false;

        // property set and name are compulsory
        var pset = GetChildNodes("propertySet").FirstOrDefault();
        var psetMatcher = pset?.GetListMatcher();
        if (psetMatcher is null)
            return IdsErrorMessages.Report102NoStringMatcher(logger, this, "propertySet");

        var name = GetChildNodes("name").FirstOrDefault();
        var nameMatcher = name?.GetListMatcher();
        if (nameMatcher is null)
            return IdsErrorMessages.Report102NoStringMatcher(logger, this, "name");

        value = GetChildNodes("value").FirstOrDefault();
        if (string.IsNullOrEmpty(dataType) && !IdsSimpleValue.IsNullOrEmpty(value))
        {
            return IdsErrorMessages.Report203IncompatibleConstraints(logger, this, "specifiying a 'value' requires a 'dataType'");
        }


        // we are keeping the stricter type to ensure that it is valid across multiple schemas
        // depending on the schema version of IfcRelDefinesByProperties the filter needs to be
        //
        // - IfcObject in ifc2x3 
        // - IfcObjectDefinition in Ifc4 
        // - IfcObjectDefinition in ifc4x3
        // 
        // todo: we need to add material properties (at least)...
        // start from the following documentation to work out what other types are enabled:
        // in ifc2x3 see https://standards.buildingsmart.org/IFC/RELEASE/IFC2x3/TC1/HTML/ifcmaterialpropertyresource/lexical/ifcextendedmaterialproperties.htm 
        //     in ifc2x3 the connection does not allow for a pset in the pset
        // in ifc4 see https://standards.buildingsmart.org/MVD/RELEASE/IFC4/ADD2_TC1/RV1_2/HTML/schema/ifcpropertyresource/lexical/ifcextendedproperties.htm (new in 4)
        //     this is abstract and only subclass is ifcmaterialproperties pointing to IfcMaterialDefinition
        // in ifc4x3 see https://ifc43-docs.standards.buildingsmart.org/IFC/RELEASE/IFC4x3/HTML/lexical/IfcExtendedProperties.htm
        //     two subclasses for materials (IfcMaterialDefinition) and profiles (IfcProfileDef)	
        var ret = Audit.Status.Ok;

		requiredSchemaVersions.TryGetSchemaInformation(out var schemas);
        foreach (var schema in schemas)
        {
            // we start from the basic types in IfcRelDefinesByProperties 
            IIfcTypeConstraint filter = schema.GetRelAssignPropertyClasses!; // banged, because we should be guaranteed to have it.

            // initiate valid measures, will constrain later if there's a known property
            var validMeasureNames = SchemaInfo.AllMeasures
                    .Where(x => (x.ValidSchemaVersions & schema.Version) == schema.Version)
                    .Select(y => y.IfcMeasureClassName.ToUpperInvariant());
            IsValid = true;

            if (IsRequired)
            {
                var validPsetNames = SchemaInfo.SharedPropertySetNames(schema.Version);
                if (psetMatcher.TryMatch(validPsetNames, false, out var possiblePsetNames))
                {
                    // see if there's a match with standard property sets
                    var validPropNames = SchemaInfo.SharedPropertyNames(schema.Version, possiblePsetNames);
                    var nameMatch = nameMatcher.DoesMatch(validPropNames, false, logger, out var possiblePropertyNames, "property name", schema.Version);
                    if (nameMatch != Audit.Status.Ok)
                        return SetInvalid();

                    // limit the validity of the IfcMeasure to the value coming from the metadata for the property
                    var limit = SchemaInfo.ValidMeasureForAllProperties(schema.Version, possiblePsetNames, possiblePropertyNames);
                    if (limit is null)
                        validMeasureNames = Enumerable.Empty<string>();
                    else
                        validMeasureNames = new string[] { limit };

                    // limit the validity of the type
                    var validTypes = SchemaInfo.PossibleTypesForPropertySets(schema.Version, possiblePsetNames);
					typeFilters.Add(schema, new IfcConcreteTypeList(validTypes));
                }
                else if (psetMatcher is IStringPrefixMatcher ssm && ssm.MatchesPrefix("Pset_"))
                {
                    IsValid = false;
                    return IdsErrorMessages.Report401ReservedPrefix(logger, this, "Pset_", "property set name", schema, ssm.Value);
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
				typeFilters.Add(schema, filter);
				IsValid = true;
            }
			if (dataTypeMatcher is not null)
			{
				ret |= dataTypeMatcher.DoesMatch(validMeasureNames, false, logger, out var matches, "datatype", schema.Version);
			}
			if (ret != Audit.Status.Ok)
				IsValid = false;
		}
       
        return ret;
    }

    private Audit.Status SetInvalid()
    {
        typeFilters.Clear();
        IsValid = false;
        return Audit.Status.IdsContentError;
    }

    public Audit.Status PerformCardinalityAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (cardinality.Audit(out var _) != Audit.Status.Ok)
            ret |= IdsErrorMessages.Report301InvalidCardinality(logger, this, cardinality);
        else if (cardinality is ConditionalCardinality crd)
        {
            if (crd.enumerationValue == "optional" && string.IsNullOrEmpty(dataType))
            {
                IdsErrorMessages.Report202InvalidCardinalityContext(logger, this, cardinality, crd.enumerationValue, "it requires the specification of the 'dataType' constraint");
                ret |= CardinalityConstants.CardinalityErrorStatus;
                IsValid = false;
            }
            else if (crd.enumerationValue == "prohibited" && !string.IsNullOrEmpty(dataType))
            {
                IdsErrorMessages.Report202InvalidCardinalityContext(logger, this, cardinality, crd.enumerationValue, "it is not compatible with the specification of the 'dataType' constraint");
                ret |= CardinalityConstants.CardinalityErrorStatus;
                IsValid = false;
            }
        }
        return ret;
    }
}
