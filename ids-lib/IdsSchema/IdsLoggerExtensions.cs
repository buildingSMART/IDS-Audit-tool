using IdsLib.IdsSchema.IdsNodes;
using Microsoft.Extensions.Logging;
using System;

namespace IdsLib.IdsSchema;

internal static class IdsLoggerExtensions
{
    internal static Audit.Status ReportUnexpectedScenario(this ILogger? logger, string scenarioMessage, BaseContext context)
    {
        logger?.LogCritical("Unhandled scenario: {message} on `{tp}` at line {line}, position {pos}.", scenarioMessage, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.NotImplementedError;
    }
    internal static Audit.Status ReportInvalidOccurr(this ILogger? logger, BaseContext context, MinMaxOccur minMax) 
    {
        minMax.Audit(out var occurError);
        logger?.LogError("Invalid occurrence on `{tp}` at line {line}, position {pos}. {occurError}.", context.type, context.StartLineNumber, context.StartLinePosition, occurError);
        return Audit.Status.IdsContentError;
    }

    /// <param name="alternative">alternative string with no punctuation at the end</param>
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
        logger?.LogError("Invalid type `{tp}` to match `{expected}` at line {line}, position {pos}.", context.type, expected, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidListMatcher(BaseContext xmlContext, string value, ILogger? logger, string listToMatchName, IfcSchema.IfcSchemaVersions? schemaContext)
    {
        if (schemaContext.HasValue)
            logger?.LogError("Invalid value `{val}` in {tp} to match `{expected}` in the context of {schemaContext} at line {line}, position {pos}.", value, xmlContext.type, listToMatchName, schemaContext.Value, xmlContext.StartLineNumber, xmlContext.StartLinePosition);
        else
            logger?.LogError("Invalid value `{val}` in {tp} to match `{expected}` at line {line}, position {pos}.", value, xmlContext.type, listToMatchName, xmlContext.StartLineNumber, xmlContext.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidDataConfiguration(ILogger? logger, BaseContext context, string message)
    {
        logger?.LogError("{message}, node `{tp}` at line {line}, position {pos}.", message, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }
}
