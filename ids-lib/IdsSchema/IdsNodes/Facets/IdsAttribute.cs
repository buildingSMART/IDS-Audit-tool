using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsAttribute : IdsXmlNode, IIdsFacet, IIfcTypeConstraintProvider
{   
    public IdsAttribute(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        IsValid = false;
    }

    public bool IsValid { get; private set; }

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

	internal protected override Audit.Status PerformAudit(ILogger? logger)
    {
		if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
			return retStatus;
		var requiredSchemaVersions = spec.SchemaVersions;
        var name = GetChildNodes("name").FirstOrDefault(); // name must exist
        var sm = name?.GetListMatcher();
        // the first child must be a valid string matcher
        if (sm is null)
            return IdsErrorMessages.Report102NoStringMatcher(logger, this, "name");

        Audit.Status ret = Audit.Status.Ok;
		requiredSchemaVersions.TryGetSchemaInformation(out var schemas);
        foreach (var schema in schemas)
        {
			var validAttributeNames = SchemaInfo.AllAttributes
			.Where(x => (x.ValidSchemaVersions & schema.Version) == schema.Version)
			.Select(y => y.IfcAttributeName);
			ret |= sm.DoesMatch(validAttributeNames, false, logger, out var matches, "attribute names", schema.Version);
			if (ret != Audit.Status.Ok)
				return SetInvalid();
			// if we have valid attributes we can restrict the valid types depending on them
			typeFilters.Add(schema, new IfcConcreteTypeList(SchemaInfo.SharedClassesForAttributes(schema.Version, matches)));
		}
		IsValid = true;


		return ret;
    }
    private Audit.Status SetInvalid()
    {
		typeFilters.Clear();
        IsValid = false;
        return Audit.Status.IdsContentError;
    }
}
