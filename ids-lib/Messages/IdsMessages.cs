using IdsLib.IdsSchema;
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

internal static class IdsMessages
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

	internal static Audit.Status Report103InvalidListMatcher(IdsXmlNode xmlContext, string value, ILogger? logger, string nameOflistToMatch, IfcSchema.IfcSchemaVersions schemaContext, IEnumerable<string> candidateStrings)
	{
		if (!candidateStrings.Any())
			logger?.LogError("Error {errorCode}: Invalid value `{value}` to match `{nameOflistToMatch}` (no valid values exist) in the context of {schemaContext} on {location}.", 103, value, nameOflistToMatch, schemaContext, xmlContext.GetNodeIdentification());
		else
		{
			var count = candidateStrings.Count();
			if (count == 1)
				logger?.LogError("Error {errorCode}: Invalid value `{value}` to match `{nameOflistToMatch}` (the only accepted value is `{acceptedValue}`) in the context of {schemaContext} on {location}.", 103, value, nameOflistToMatch, candidateStrings.First(), schemaContext, xmlContext.GetNodeIdentification());
			else if (count < 6)
				logger?.LogError("Error {errorCode}: Invalid value `{value}` to match `{nameOflistToMatch}` (accepted values are {acceptedValues}) in the context of {schemaContext} on {location}.", 103, value, nameOflistToMatch, string.Join(",", candidateStrings), schemaContext, xmlContext.GetNodeIdentification());
			else
				logger?.LogError("Error {errorCode}: Invalid value `{value}` to match `{nameOflistToMatch}` ({acceptedValuesCount} accepted values exist, starting with {acceptedValues}...) in the context of {schemaContext} on {location}.", 103, value, nameOflistToMatch, count, candidateStrings.Take(5), schemaContext, xmlContext.GetNodeIdentification());
		}
		return Audit.Status.IdsContentError;
	}


	internal static Audit.Status Report104InvalidListMatcherCount(IdsXmlNode xmlContext, string value, ILogger? logger, string listToMatchName, int numberOfMatches, IfcSchema.IfcSchemaVersions schemaContext)
	{
		logger?.LogError("Error {errorCode}: Invalid number of matches ({count}) for `{val}` in {tp} to match `{expected}` in the context of {schemaContext} on {location}.", 104, numberOfMatches, value, xmlContext.type, listToMatchName, schemaContext, xmlContext.GetNodeIdentification());
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
		logger?.LogError("Error {errorCode}: Invalid empty attribute {attributeName} on {location}.", 106, emptyAttributeName, context.GetNodeIdentification());
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

	internal static Audit.Status Report201IncompatibleClauses(ILogger? logger, IdsXmlNode context, string scenarioMessage)
	{
		logger?.LogError("Error {errorCode}: Inconsistent clauses: {message} on {location}.", 201, scenarioMessage, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report301InvalidCardinality(ILogger? logger, IdsXmlNode context, MinMaxCardinality minMax)
	{
		minMax.Audit(out var occurError);
		logger?.LogError("Error {errorCode}: Invalid cardinality on {location}. {occurError}.", 301, context.GetNodeIdentification(), occurError);
		return MinMaxCardinality.ErrorStatus;
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

	internal static Audit.Status Report303RestrictionBadType(ILogger? logger, IdsXmlNode context, string baseAsString)
	{
		logger?.LogError("Error {errorCode}: Invalid type `{base}` on {location}.", 303, baseAsString, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report304RestrictionEmptyContent(ILogger? logger, IdsXmlNode context)
	{
		logger?.LogError("Error {errorCode}: No details on {location}.", 304, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report305BadConstraintValue(ILogger? logger, IdsXmlNode context, string value, XsRestriction.BaseTypes baseType)
	{
		logger?.LogError("Error {errorCode}: Invalid value '{vers}' for base type '{baseType}' on {location}.", 305, value, baseType, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static void ReportSchema306ComplianceError(ILogger? logger, NodeIdentification location, string message)
	{
		logger?.LogError("Error {errorCode}: Schema compliance error on {location}; {message}", 306, location, message);
	}

	internal static void ReportSchema307ComplianceWarning(ILogger? logger, LogLevel level, NodeIdentification location, string message)
	{
		logger?.Log(level, "Error {errorCode}: Schema compliance warning on {location}; {message}", 307, location, message);
	}

	internal static Audit.Status Report401ReservedStringMatched(ILogger? logger, IdsXmlNode context, string keyword, string field)
	{
		logger?.LogError("Error {errorCode}: Reserved keyword '{part}' for {field} on {location}.", 401, keyword, field, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status Report501UnexpectedScenario(ILogger? logger, string scenarioMessage, IdsXmlNode context)
	{
		logger?.LogCritical("Error {errorCode}: Unhandled scenario: {message} on {location}.", 501, scenarioMessage, context.GetNodeIdentification());
		return Audit.Status.NotImplementedError;
	}
}
