using IdsLib.IdsSchema;
using Microsoft.Extensions.Logging;

namespace IdsLib.IfcSchema;

internal static class IfcLoggerExtensions
{
    internal static Audit.Status ReportInvalidSchemaVersion(this ILogger? logger, IfcSchemaVersions version, BaseContext context)
    {
        logger?.LogError("Invalid schema version '{vers}' in {tp} at line {line}, position {pos}.", version, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }
    internal static IfcSchemaVersions ReportInvalidSchemaString(this ILogger? logger, string version, BaseContext context)
    {
        logger?.LogError("Invalid schema version '{vers}' in {tp} at line {line}, position {pos}.", version, context.type, context.StartLineNumber, context.StartLinePosition);
        return IfcSchemaVersions.IfcNoVersion;
    }
}
