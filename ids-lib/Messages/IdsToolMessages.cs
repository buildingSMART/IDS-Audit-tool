using IdsLib.IdsSchema;
using IdsLib.IdsSchema.IdsNodes;
using Microsoft.Extensions.Logging;
using static IdsLib.Audit;

namespace IdsLib.Messages
{
	internal class IdsToolMessages
	{
		internal static Status ReportInvalidPattern(ILogger? logger, string omitIdsContentAuditPattern)
		{
			logger?.LogWarning("Invalid OmitIdsContentAuditPattern `{pattern}`.", omitIdsContentAuditPattern);
			return Status.InvalidOptionsError;
		}

		internal static Status ReportNoAudit(ILogger? logger)
		{
			logger?.LogError("No audit performed.");
			return Status.InvalidOptionsError;
		}

		internal static Status ReportNoActionRequired(ILogger? logger)
		{
			logger?.LogWarning("No audits are required, with the options passed.");
			return Status.InvalidOptionsError;
		}

		internal static Status ReportInvalidOptions(ILogger? logger)
		{
			logger?.LogError("Invalid options.");
			return Status.InvalidOptionsError;
		}

		internal static void ReportActions(ILogger? logger, ActionCollection auditsList)
		{
			logger?.LogInformation("Auditing: {audits}.", auditsList);
		}

		internal static Status ReportInvalidSource(ILogger? logger, string inputSource)
		{
			logger?.LogError("Invalid input source '{missingSource}'", inputSource);
			return Status.NotFoundError;
		}

		internal static void ReportReadCount(ILogger? logger, int cntRead)
		{
			logger?.LogDebug("Completed reading {cntRead} xml elements.", cntRead);
		}

		internal static void ReportFileProcessingStarted(ILogger? logger, string fullName)
		{
			logger?.LogInformation("Auditing file: `{filename}`.", fullName);
		}

		internal static void ReportFileProcessingEnded(ILogger? logger, int tally)
		{
			var fileCardinality = tally != 1 ? "files" : "file";
			logger?.LogInformation("{tally} {fileCardinality} processed.", tally, fileCardinality);
		}

		internal static Audit.Status ReportUnexpectedScenario(ILogger? logger, string scenarioMessage)
		{
			logger?.LogCritical("Unhandled scenario: {message}", scenarioMessage);
			return Audit.Status.NotImplementedError;
		}

		internal static Audit.Status ReportLocatedUnexpectedScenario(ILogger? logger, string scenarioMessage, BaseContext context)
		{
			logger?.LogCritical("Unhandled scenario: {message} on `{tp}` at line {line}, position {pos}.", scenarioMessage, context.type, context.StartLineNumber, context.StartLinePosition);
			return Audit.Status.NotImplementedError;
		}

		internal static Status ReportUnseekableStream(ILogger? logger)
		{
			logger?.LogCritical("The provided stream must be able to seek to detect the schema version from its content.");
			return Audit.Status.UnhandledError;
		}

		internal static Status ReportInvalidVersion(ILogger? logger)
		{
			logger?.LogCritical("Unrecognised version from location value.");
			return Audit.Status.IdsStructureError;
		}

		internal static Status ReportSourceNotFound(ILogger? logger, string diskSchema)
		{
			logger?.LogError("File `{schemaFile}` not found.", diskSchema);
			return Audit.Status.NotFoundError;
		}

		internal static Status ReportInvalidXsdSource(ILogger? logger, string diskSchema)
		{
			logger?.LogError("Error reading XSD Schema from `{schemaFile}`.", diskSchema);
			return Audit.Status.XsdSchemaError;
		}

		internal static Audit.Status ReportInvalidSchemaVersion(ILogger? logger, IdsVersion vrs)
		{
			logger?.LogError("Embedded schema for version {vrs} not implemented.", vrs);
			return Audit.Status.NotImplementedError;
		}
	}
}
