using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema;

internal static class IdsLoggerExtensions
{
    internal static Audit.Status ReportInvalidApplicability(this ILogger? logger, BaseContext context, string scenarioMessage)
    {
        logger?.LogError("Invalid applicability: {message} on `{tp}` at line {line}, position {pos}.", scenarioMessage, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }
    internal static Audit.Status ReportIncompatibleClauses(this ILogger? logger, BaseContext context, string scenarioMessage)
    {
        logger?.LogError("Inconsistent clauses: {message} on `{tp}` at line {line}, position {pos}.", scenarioMessage, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportUnexpectedScenario(this ILogger? logger, string scenarioMessage)
    {
        logger?.LogCritical("Unhandled scenario: {message}", scenarioMessage);
        return Audit.Status.NotImplementedError;
    }

    internal static Audit.Status ReportLocatedUnexpectedScenario(this ILogger? logger, string scenarioMessage, BaseContext context)
    {
        logger?.LogCritical("Unhandled scenario: {message} on `{tp}` at line {line}, position {pos}.", scenarioMessage, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.NotImplementedError;
    }
    internal static Audit.Status ReportInvalidOccurr(this ILogger? logger, BaseContext context, MinMaxOccur minMax) 
    {
        minMax.Audit(out var occurError);
        logger?.LogError("Invalid occurrence on `{tp}` at line {line}, position {pos}. {occurError}.", context.type, context.StartLineNumber, context.StartLinePosition, occurError);
        return MinMaxOccur.ErrorStatus;
    }

    /// <summary>
    /// Report an invalid XML structure within a facet
    /// </summary>
    /// <param name="logger">optional logging destination</param>
    /// <param name="context">Provides indication of the position of the error in the file</param>
    /// <param name="alternative">if an alternative solution is preferred, it can be suggested here with no punctuation at the end</param>
    /// <returns>The error status associated with this problem</returns>
    internal static Audit.Status ReportInvalidXsFacet(this ILogger? logger, BaseContext context, string alternative)
    {
        if (string.IsNullOrWhiteSpace(alternative))
            logger?.LogError("Invalid context for `{tp}` at line {line}, position {pos}.", context.type, context.StartLineNumber, context.StartLinePosition);
        else
            logger?.LogError("Invalid context for `{tp}` at line {line}, position {pos}; {alternative}.", context.type, context.StartLineNumber, context.StartLinePosition, alternative);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidStringMatcher(this ILogger? logger, BaseContext context, string field)
    {
        logger?.LogError("Invalid string matcher for `{field}` at `{tp}`, line {line}, position {pos}.", field, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportReservedStringMatched(this ILogger? logger, BaseContext context, string part, string field)
    {
        logger?.LogError("Reserved {part} for {field} at `{tp}`, line {line}, position {pos}.", part, field, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }


    internal static Audit.Status ReportNoStringMatcher(this ILogger? logger, BaseContext context, string field)
    {
        logger?.LogError("Empty string matcher for `{field}` on `{tp}` at line {line}, position {pos}.", field, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportBadType(ILogger? logger, BaseContext context, string baseAsString)
    {
        logger?.LogError("Invalid type `{base}` on `{tp}` at line {line}, position {pos}.", baseAsString, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportBadMatcher(ILogger? logger, BaseContext context, string expected)
    {
        logger?.LogError("Invalid type `{elementType}` to match `{expected}` at line {line}, position {pos}.", context.type, expected, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidListMatcher(BaseContext xmlContext, string value, ILogger? logger, string nameOflistToMatch, IfcSchema.IfcSchemaVersions schemaContext, IEnumerable<string> candidateStrings)
    {
        if (!candidateStrings.Any())
            logger?.LogError("Invalid value `{value}` in {elementType} to match `{nameOflistToMatch}` (no valid values exist) in the context of {schemaContext} at line {line}, position {pos}.", value, xmlContext.type, nameOflistToMatch, schemaContext, xmlContext.StartLineNumber, xmlContext.StartLinePosition);
        else
        {
            var count = candidateStrings.Count();
            if (count == 1)
                logger?.LogError("Invalid value `{value}` in {elementType} to match `{nameOflistToMatch}` (the only accepted value is `{acceptedValue}`) in the context of {schemaContext} at line {line}, position {pos}.", value, xmlContext.type, nameOflistToMatch, candidateStrings.First(), schemaContext, xmlContext.StartLineNumber, xmlContext.StartLinePosition);
            else if (count < 6)
                logger?.LogError("Invalid value `{value}` in {elementType} to match `{nameOflistToMatch}` (accepted values are {acceptedValues}) in the context of {schemaContext} at line {line}, position {pos}.", value, xmlContext.type, nameOflistToMatch, string.Join(",", candidateStrings), schemaContext, xmlContext.StartLineNumber, xmlContext.StartLinePosition);
            else
                logger?.LogError("Invalid value `{value}` in {elementType} to match `{nameOflistToMatch}` ({acceptedValuesCount} accepted values exist) in the context of {schemaContext} at line {line}, position {pos}.", value, xmlContext.type, nameOflistToMatch, count, schemaContext, xmlContext.StartLineNumber, xmlContext.StartLinePosition);
        }
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidListMatcherCount(BaseContext xmlContext, string value, ILogger? logger, string listToMatchName, int numberOfMatches, IfcSchema.IfcSchemaVersions? schemaContext)
    {
        if (schemaContext.HasValue)
            logger?.LogError("Invalid number of matches ({count}) for `{val}` in {tp} to match `{expected}` in the context of {schemaContext} at line {line}, position {pos}.", numberOfMatches, value, xmlContext.type, listToMatchName, schemaContext.Value, xmlContext.StartLineNumber, xmlContext.StartLinePosition);
        else
            logger?.LogError("Invalid number of matches ({count}) for `{val}` in {tp} to match `{expected}` at line {line}, position {pos}.", numberOfMatches, value, xmlContext.type, listToMatchName, xmlContext.StartLineNumber, xmlContext.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidDataConfiguration(ILogger? logger, BaseContext context, string message)
    {
        logger?.LogError("{message}, node `{tp}` at line {line}, position {pos}.", message, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidEmtpyValue(ILogger? logger, BaseContext context, string emptyFieldName)
    {
        logger?.LogError("Invalid empty field {message} in node `{tp}` at line {line}, position {pos}.", emptyFieldName, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }


}
