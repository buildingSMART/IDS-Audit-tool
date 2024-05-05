using IdsLib.IdsSchema;
using IdsLib.IdsSchema.Cardinality;
using IdsLib.IdsSchema.IdsNodes;
using IdsLib.IdsSchema.XsNodes;
using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.Messages;
/// <summary>
/// todo: create resource for internationalization
/// todo: add configuration to return :p style location <see cref="NodeIdentification.ToString(string?, IFormatProvider?)"/>, that defines the :p style
///       e.g. ("Inconsistent clauses: {message} on {location:p}.", scenarioMessage, context.GetNodeIdentification());
/// </summary>

/// Reminder: when adding an error code here also add to the ErrorCodes.md in the lib.

internal static class IdsErrorMessages
{
	internal static Audit.Status Report101InvalidApplicability(ILogger? logger, IdsXmlNode context, string scenarioMessage)
	{
		logger?.LogError("Error {errorCode}: Invalid applicability. {message} on {location}.", 101, scenarioMessage, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report102NoStringMatcher(ILogger? logger, IdsXmlNode context, string field)
	{
		logger?.LogError("Error {errorCode}: Empty string matcher for `{field}` on {location}.", 102, field, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report103InvalidListMatcher(IdsXmlNode xmlContext, string value, ILogger? logger, string variableName, IfcSchema.IfcSchemaVersions schemaContext, IEnumerable<string> candidateStrings)
	{
		if (!candidateStrings.Any())
			logger?.LogError("Error {errorCode}: Invalid {variableName} `{value}` (no valid values exist) in the context of {schemaContext} on {location}.", 103, variableName, value, schemaContext, xmlContext.GetNodeIdentification());
		else
		{
			var count = candidateStrings.Count();
			if (count == 1)
				logger?.LogError("Error {errorCode}: Invalid {variableName} `{value}` (the only accepted value is `{acceptedValue}`) in the context of {schemaContext} on {location}.", 103, variableName, value, candidateStrings.First(), schemaContext, xmlContext.GetNodeIdentification());
			else if (count < 6)
				logger?.LogError("Error {errorCode}: Invalid {variableName} `{value}` (accepted values are {acceptedValues}) in the context of {schemaContext} on {location}.", 103, variableName, value, string.Join(",", candidateStrings), schemaContext, xmlContext.GetNodeIdentification());
			else
				logger?.LogError("Error {errorCode}: Invalid {variableName} `{value}` ({acceptedValuesCount} accepted values exist, starting with {acceptedValues}...) in the context of {schemaContext} on {location}.", 103, variableName, value, count, candidateStrings.Take(5), schemaContext, xmlContext.GetNodeIdentification());
		}
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report104InvalidListMatcherCount(IdsXmlNode xmlContext, string value, ILogger? logger, string variableName, int numberOfMatches, IfcSchema.IfcSchemaVersions schemaContext)
	{
		logger?.LogError("Error {errorCode}: Invalid number of matches ({count}) for `{val}` in {tp} to match `{expected}` in the context of {schemaContext} on {location}.", 104, numberOfMatches, value, xmlContext.type, variableName, schemaContext, xmlContext.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report105InvalidDataConfiguration(ILogger? logger, IdsXmlNode context, string field)
	{
		// no valid configuration option exists for field given the context
		logger?.LogError("Error {errorCode}: {message}, on {location}.", 105, field, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report106InvalidEmtpyValue(ILogger? logger, IdsXmlNode context, string emptyAttributeName)
	{
		logger?.LogError("Error {errorCode}: Invalid empty value for {attributeName} on {location}.", 106, emptyAttributeName, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report107InvalidIfcSchemaVersion(ILogger? logger, IfcSchemaVersions version, IdsXmlNode context)
	{
		logger?.LogError("Error {errorCode}: Invalid schema version '{vers}' on {location}.", 107, version, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static IfcSchemaVersions Report107InvalidIfcSchemaString(ILogger? logger, string version, IdsXmlNode context)
	{
		logger?.LogError("Error {errorCode}: Invalid schema version '{vers}' on {location}.", 107, version, context.GetNodeIdentification());
		return IfcSchemaVersions.IfcNoVersion;
	}

	internal static void Report108UnsupportedIdsSchema(ILogger? logger, string version)
	{
		logger?.LogError("Error {errorCode}: Unsupported schema version '{vers}' on `ids` element, please use 'http://standards.buildingsmart.org/IDS/0.9.6/ids.xsd' instead.", 108, version);
	}

	internal static Audit.Status Report109InvalidRegex(IdsXmlNode locationContext, string pattern, ILogger? logger)
	{
		logger?.LogError("Error {errorCode}: Ivalid pattern string '{pattern}' on {location}.", 109, pattern, locationContext.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report201IncompatibleClauses(ILogger? logger, IdsXmlNode locationContext, SchemaInfo schemaInfo, string scenarioMessage)
	{
		logger?.LogError("Error {errorCode}: Inconsistent clauses for {ifcSchema}: {message} on {location}.", 201, schemaInfo.Version, scenarioMessage, locationContext.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

    internal static Audit.Status Report202InvalidCardinalityContext(ILogger? logger, IdsXmlNode context, ICardinality cardinality, string cardinalityValue, string errorMessage)
    {
        logger?.LogError("Error {errorCode}: Invalid cardinality '{cardinalityValue}' on {location}, {occurError}.", 202, cardinalityValue, context.GetNodeIdentification(), errorMessage);
        return CardinalityConstants.CardinalityErrorStatus;
    }

    internal static Audit.Status Report203IncompatibleConstraints(ILogger? logger, IdsXmlNode context, string errorMessage)
    {
        logger?.LogError("Error {errorCode}: Incompatible constraints on {location}, {errorMessage}.", 203, context.GetNodeIdentification(), errorMessage);
        return Audit.Status.IdsContentError;
    }
	internal static Audit.Status Report204IncompatibleRequirements(ILogger? logger, IdsXmlNode context, string errorMessage)
	{
		logger?.LogError("Error {errorCode}: Incompatible requirements on {location}, {errorMessage}.", 204, context.GetNodeIdentification(), errorMessage);
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report301InvalidCardinality(ILogger? logger, IdsXmlNode context, ICardinality minMax)
	{
		minMax.Audit(out var cardinalityError);
		logger?.LogError("Error {errorCode}: Invalid cardinality on {location}. {occurError}.", 301, context.GetNodeIdentification(), cardinalityError);
		return CardinalityConstants.CardinalityErrorStatus;
	}

    /// <summary>
    /// Report an invalid XML structure within a facet
    /// </summary>
    /// <param name="logger">optional logging destination</param>
    /// <param name="context">Provides indication of the position of the error in the file</param>
    /// <param name="alternative">if an alternative solution is preferred, it can be suggested here with no punctuation at the end</param>
    /// <returns>The error status associated with this problem</returns>
    internal static Audit.Status Report302InvalidXsFacet(ILogger? logger, IdsXmlNode context, string alternative)
	{
		if (string.IsNullOrWhiteSpace(alternative))
			logger?.LogError("Error {errorCode}: Invalid context on {location}.", 302, context.GetNodeIdentification());
		else
			logger?.LogError("Error {errorCode}: Invalid context on {location}; {alternative}.", 302, context.GetNodeIdentification(), alternative);
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report303RestrictionBadType(ILogger? logger, IdsXmlNode context, string stringValue)
	{
		if (string.IsNullOrEmpty(stringValue))
			logger?.LogError("Error {errorCode}: Invalid base type; empty string on {node}.", 303, context.GetNodeIdentification());
		else
			logger?.LogError("Error {errorCode}: Invalid base type; `{stringValue}` on {node}.", 303, stringValue, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report303RestrictionBadType(ILogger? logger, IdsXmlNode context, string description, SchemaInfo schema)
	{
		logger?.LogError("Error {errorCode}: Invalid base type for {schemaVersion}; {errorDescription} on {node}.", 303, schema.Version, description, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report304RestrictionEmptyContent(ILogger? logger, IdsXmlNode context)
	{
		logger?.LogError("Error {errorCode}: No details on {location}.", 304, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report305BadConstraintValue(ILogger? logger, IdsXmlNode context, string value, XsTypes.BaseTypes baseType)
	{
		logger?.LogError("Error {errorCode}: Invalid value `{invalidStringValue}` for base type `{baseType}` on {location}.", 305, value, baseType, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static void Report306SchemaComplianceError(ILogger? logger, NodeIdentification location, string message)
	{
		logger?.LogError("Error {errorCode}: Schema compliance error on {location}; {message}", 306, location, message);
	}

	internal static void Report307SchemaComplianceWarning(ILogger? logger, LogLevel level, NodeIdentification location, string message)
	{
		logger?.Log(level, "{LogLevel} {errorCode}: Schema compliance warning on {location}; {message}", level, 307, location, message);
	}

	internal static Audit.Status Report308RestrictionInvalidFacet(ILogger? logger, XsTypes.BaseTypes restrictionBase, IdsXmlNode invalidFacetType, IEnumerable<string> validFacetTypes)
	{
		logger?.LogError("Error {errorCode}: Invalid type `{invalidFacetType}` for restriction with base `{base}` (valid types are: {validTypes}) on {location}.", 308, invalidFacetType.type, restrictionBase, string.Join(", ", validFacetTypes), invalidFacetType.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report401ReservedPrefix(ILogger? logger, IdsXmlNode context, string prefix, string field, SchemaInfo schema, string value)
	{
		logger?.LogError("Error {errorCode}: Reserved prefix '{prefix}' for {field} ({matcherValue}) in the context of {schema} on {location}.", 401, prefix, field, value, schema.Version, context.GetNodeIdentification());        
        return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report501UnexpectedScenario(ILogger? logger, string scenarioMessage, IdsXmlNode context)
	{
		logger?.LogCritical("Error {errorCode}: Unhandled scenario: {message} on {location}.", 501, scenarioMessage, context.GetNodeIdentification());
		return Audit.Status.NotImplementedError;
	}

	
}
