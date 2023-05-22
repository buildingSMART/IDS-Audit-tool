using IdsLib.IdsSchema;
using IdsLib.IdsSchema.XsNodes;
using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.Messages;

internal static class IdsMessages
{

	// todo: create resource for internationalization
	// todo: add error code 
	// todo: add configuration to return :p style location
	//       e.g. ("Inconsistent clauses: {message} on {location:p}.", scenarioMessage, context.GetNodeIdentification());

	internal static Audit.Status ReportInvalidApplicability(ILogger? logger, IdsXmlNode context, string scenarioMessage)
    {
        logger?.LogError("Error {errorCode}: Invalid applicability. {message} on {location}.", 101, scenarioMessage, context.GetNodeIdentification());
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportIncompatibleClauses(ILogger? logger, IdsXmlNode context, string scenarioMessage)
    {
		logger?.LogError("Error {errorCode}: Inconsistent clauses: {message} on {location}.", 201, scenarioMessage, context.GetNodeIdentification());
        return Audit.Status.IdsContentError;
    }

	internal static Audit.Status ReportInvalidCardinality(ILogger? logger, IdsXmlNode context, MinMaxCardinality minMax)
	{
		minMax.Audit(out var occurError);
		logger?.LogError("Error {errorCode}: Invalid cardinality on {location}. {occurError}.", 301, context.GetNodeIdentification(), occurError);
		return MinMaxCardinality.ErrorStatus;
	}

    internal static Audit.Status ReportUnexpectedScenario(ILogger? logger, string scenarioMessage, IdsXmlNode context)
    {
        logger?.LogCritical("Error {errorCode}: Unhandled scenario: {message} on {location}.", 501, scenarioMessage, context.GetNodeIdentification());
        return Audit.Status.NotImplementedError;
    }

	/// <summary>
	/// Report an invalid XML structure within a facet
	/// </summary>
	/// <param name="logger">optional logging destination</param>
	/// <param name="context">Provides indication of the position of the error in the file</param>
	/// <param name="alternative">if an alternative solution is preferred, it can be suggested here with no punctuation at the end</param>
	/// <returns>The error status associated with this problem</returns>
	internal static Audit.Status ReportInvalidXsFacet(ILogger? logger, IdsXmlNode context, string alternative)
    {
        if (string.IsNullOrWhiteSpace(alternative))
            logger?.LogError("Error {errorCode}: Invalid context on {location}.", 302, context.GetNodeIdentification());
        else
            logger?.LogError("Error {errorCode}: Invalid context on {location}; {alternative}.", 302, context.GetNodeIdentification(), alternative);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportReservedStringMatched(ILogger? logger, IdsXmlNode context, string keyword, string field)
    {
        logger?.LogError("Error {errorCode}: Reserved keyword '{part}' for {field} on {location}.", 401, keyword, field, context.GetNodeIdentification());
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportNoStringMatcher(ILogger? logger, IdsXmlNode context, string field)
    {
        logger?.LogError("Error {errorCode}: Empty string matcher for `{field}` on {location}.", 102, field, context.GetNodeIdentification());
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportRestrictionBadType(ILogger? logger, IdsXmlNode context, string baseAsString)
    {
        logger?.LogError("Error {errorCode}: Invalid type `{base}` on {location}.", 303, baseAsString, context.GetNodeIdentification());
        return Audit.Status.IdsContentError;
    }

	internal static Audit.Status ReportRestrictionEmptyContent(ILogger? logger, IdsXmlNode context)
	{
		logger?.LogError("Error {errorCode}: No details on {location}.", 304, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}

	internal static Audit.Status ReportInvalidListMatcher(IdsXmlNode xmlContext, string value, ILogger? logger, string nameOflistToMatch, IfcSchema.IfcSchemaVersions schemaContext, IEnumerable<string> candidateStrings)
    {
        if (!candidateStrings.Any())
            logger?.LogError("Error {errorCode}: Invalid value `{value}` to match `{nameOflistToMatch}` (no valid values exist) in the context of {schemaContext} on {location}.", 103, value, nameOflistToMatch, schemaContext, xmlContext.GetNodeIdentification());
        else
        {
            var count = candidateStrings.Count();
            if (count == 1)
                logger?.LogError("Error {errorCode}: Invalid value `{value}` to match `{nameOflistToMatch}` (the only accepted value is `{acceptedValue}`) in the context of {schemaContext} on {location}.", 103, value, nameOflistToMatch, candidateStrings.First(), schemaContext, xmlContext.GetPositionalIdentifier());
            else if (count < 6)
                logger?.LogError("Error {errorCode}: Invalid value `{value}` to match `{nameOflistToMatch}` (accepted values are {acceptedValues}) in the context of {schemaContext} on {location}.", 103, value, nameOflistToMatch, string.Join(",", candidateStrings), schemaContext, xmlContext.GetPositionalIdentifier());
            else
                logger?.LogError("Error {errorCode}: Invalid value `{value}` to match `{nameOflistToMatch}` ({acceptedValuesCount} accepted values exist, starting with {acceptedValues}...) in the context of {schemaContext} on {location}.", 103, value, nameOflistToMatch, count, candidateStrings.Take(5), schemaContext, xmlContext.GetPositionalIdentifier());
        }
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidListMatcherCount(IdsXmlNode xmlContext, string value, ILogger? logger, string listToMatchName, int numberOfMatches, IfcSchema.IfcSchemaVersions schemaContext)
    {
        logger?.LogError("Error {errorCode}: Invalid number of matches ({count}) for `{val}` in {tp} to match `{expected}` in the context of {schemaContext} on {location}.", 104, numberOfMatches, value, xmlContext.type, listToMatchName, schemaContext, xmlContext.GetPositionalIdentifier());
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidDataConfiguration(ILogger? logger, IdsXmlNode context, string field)
    {
		// no valid configuration option exists for field given the context
		logger?.LogError("Error {errorCode}: {message}, on {location}.", 105, field, context.GetNodeIdentification());
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidEmtpyValue(ILogger? logger, IdsXmlNode context, string emptyAttributeName)
    {
        logger?.LogError("Error {errorCode}: Invalid empty attribute {attributeName} on {location}.", 106, emptyAttributeName, context.GetNodeIdentification());
        return Audit.Status.IdsContentError;
    }

	internal static void ReportSchemaComplianceWarning(ILogger? logger, LogLevel level, string location, string message)
	{
        logger?.Log(level, "Error {errorCode}: Schema compliance warning on {location}; {message}", location, message);
	}

	internal static void ReportSchemaComplianceError(ILogger? logger, string location, string message)
	{
		logger?.LogError("Error {errorCode}: Schema compliance error on {location}; {message}", location, message);
	}

	internal static Audit.Status ReportInvalidSchemaVersion(ILogger? logger, IfcSchemaVersions version, IdsXmlNode context)
	{
		logger?.LogError("Error {errorCode}: Invalid schema version '{vers}' on {location}.", 107, version, context.GetNodeIdentification());
		return Audit.Status.IdsContentError;
	}
	internal static IfcSchemaVersions ReportInvalidSchemaString(ILogger? logger, string version, IdsXmlNode context)
	{
		logger?.LogError("Error {errorCode}: Invalid schema version '{vers}' on {location}.", 107, version, context.GetNodeIdentification());
		return IfcSchemaVersions.IfcNoVersion;
	}

	internal static Audit.Status ReportBadConstraintValue(ILogger? logger, IdsXmlNode context, string value, XsRestriction.BaseTypes baseType)
	{
		logger?.LogError("Error {errorCode}: Invalid value '{vers}' for base type '{baseType}' on {location}.", 305, value, baseType, context.GetNodeIdentification());
        return Audit.Status.IdsContentError;
	}


}
