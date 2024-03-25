using IdsLib.IdsSchema.Cardinality;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsAttribute : IdsXmlNode, IIdsFacet, IIfcTypeConstraintProvider, IIdsCardinalityFacet
{

    private readonly ICardinality cardinality;

    /// <summary>
    /// value is used when evaluating cardinality for requirements
    /// </summary>
    private IdsXmlNode? value { get; set; } = null;

    public IdsAttribute(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        IsValid = false;
        cardinality = new ConditionalCardinality(reader);
    }

    public bool IsValid { get; private set; }

    /// <inheritdoc />
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
        var name = GetChildNodes("name").FirstOrDefault(); // name must exist
        var sm = name?.GetListMatcher();
        // the first child must be a valid string matcher
        if (sm is null)
            return IdsErrorMessages.Report102NoStringMatcher(logger, this, "name");

        value = GetChildNodes("value").FirstOrDefault(); 
        
        var ret = Audit.Status.Ok;
		requiredSchemaVersions.TryGetSchemaInformation(out var schemas);
		IsValid = true;
		foreach (var schema in schemas)
        {
            var validAttributeNames = (value is null)
                ? SchemaInfo.AllAttributes // this one includes attributes that have entity types (e.g. UnitsInContext -> IFCUNITASSIGNMENT)
                    .Where(x => (x.ValidSchemaVersions & schema.Version) == schema.Version)
                    .Select(y => y.IfcAttributeName)
                : schema.GetAttributeNames(); // this only considers attributes that have value type (e.g. LongName -> IFCLABEL)
			var thisRet = sm.DoesMatch(validAttributeNames, false, logger, out var matchingAttributeNames, "attribute name", schema.Version);
            if (thisRet != Audit.Status.Ok)
            {
                ret |= thisRet;
                continue;
            }

            if (value != null)
            {
                // if a value is defined then the type must be value type
                // we can also check that any value constraint matches the expected type
                var possibleTypes = schema.GetAttributesTypes(matchingAttributeNames).ToList();
                if (!possibleTypes.Any())
                {
					ret |= IdsErrorMessages.Report303RestrictionBadType(logger, value, $"no valid base type exists", schema);
				}
                else if (possibleTypes.Count == 1)
                {
                    var expected = possibleTypes.First();
					if (value.HasXmlBaseType(out var baseType))
					{
						if (!possibleTypes.Contains(baseType))
						{
							if (string.IsNullOrEmpty(baseType))
								ret |= IdsErrorMessages.Report303RestrictionBadType(logger, value, $"found empty but expected `{expected}`", schema);
							else
								ret |= IdsErrorMessages.Report303RestrictionBadType(logger, value, $"found `{baseType}` but expected `{expected}`", schema);
						}
					}
				}
				else
                {
					if (value.HasXmlBaseType(out var baseType))
					{
						if (!possibleTypes.Contains(baseType))
						{
                            string expected = string.Join(", ", possibleTypes);
							if (string.IsNullOrEmpty(baseType))
								ret |= IdsErrorMessages.Report303RestrictionBadType(logger, value, $"found empty but expected a close list ({expected})", schema);
							else
								ret |= IdsErrorMessages.Report303RestrictionBadType(logger, value, $"found `{baseType}` but expected a close list ({expected})", schema);
						}
					}
				}
                
			}
			// if we have valid attributes we can restrict the valid types depending on them
			typeFilters.Add(schema, new IfcConcreteTypeList(SchemaInfo.SharedClassesForAttributes(schema.Version, matchingAttributeNames)));
		}
		
        if (ret != Audit.Status.Ok)
        {
			typeFilters.Clear();
			IsValid = false;
		}

		return ret;
    }

    /// <inheritdoc />
    public Audit.Status PerformCardinalityAudit(ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (cardinality.Audit(out var _) != Audit.Status.Ok) // this evaluates if the string in the xml is within the valid range
        {
            IdsErrorMessages.Report301InvalidCardinality(logger, this, cardinality);
            ret |= CardinalityConstants.CardinalityErrorStatus;
            IsValid = false;
        }
        else
        {
            if (cardinality is ConditionalCardinality crd && crd.enumerationValue == "optional" && IdsSimpleValue.IsNullOrEmpty(value))
            {
                IdsErrorMessages.Report202InvalidCardinalityContext(logger, this, cardinality, crd.enumerationValue, "it requires the specification of the 'value' constraint"); 
                ret |= CardinalityConstants.CardinalityErrorStatus;
                IsValid = false;
            }
        }
        return ret;
    }
}
