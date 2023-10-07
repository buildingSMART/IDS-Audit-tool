using IdsLib.IdsSchema;
using IdsLib.IdsSchema.IdsNodes;
using Microsoft.Extensions.Logging;
using System;
using System.Xml;
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

		internal static void InformReadCount(ILogger? logger, int cntRead)
		{
			logger?.LogInformation("Completed reading {cntRead} xml elements.", cntRead);
		}

		internal static void InformFileProcessingStarted(ILogger? logger, string fullName)
		{
			logger?.LogInformation("Auditing file: `{filename}`.", fullName);
		}

		internal static void InformFileProcessingEnded(ILogger? logger, int tally)
		{
			var fileCardinality = tally != 1 ? "files" : "file";
			logger?.LogInformation("{tally} {fileCardinality} processed.", tally, fileCardinality);
		}

		internal static Audit.Status ReportUnexpectedScenario(ILogger? logger, string scenarioMessage)
		{
			logger?.LogCritical("Unhandled scenario: {message}", scenarioMessage);
			return Audit.Status.NotImplementedError;
		}

		internal static Status ReportUnseekableStream(ILogger? logger)
		{
			logger?.LogCritical("The provided stream must be able to seek to detect the schema version from its content.");
			return Status.UnhandledError;
		}

		internal static Status ReportInvalidVersion(string providedSchemaLocation, ILogger? logger)
		{
			logger?.LogCritical("Unrecognised version from location value ({providedLocation}).", providedSchemaLocation);
			return Status.IdsStructureError;
		}

		internal static Status ReportSourceNotFound(ILogger? logger, string diskSchema)
		{
			logger?.LogError("File `{schemaFile}` not found.", diskSchema);
			return Status.NotFoundError;
		}

		internal static Status ReportInvalidXsdSource(ILogger? logger, string diskSchema)
		{
			logger?.LogError("Error reading XSD Schema from `{schemaFile}`.", diskSchema);
			return Status.XsdSchemaError;
		}

		internal static Status Report502XmlSchemaException(ILogger? logger, XmlException ex_xml)
		{
			logger?.LogError("Error {errorCode}: XML Exception. Error message: {}", 502, ex_xml);
			return Status.IdsStructureError;
		}

		internal static Status Report503Exception(ILogger? logger, Exception ex)
		{
			logger?.LogError("Error {errorCode}: Generic Exception. Error message: {}", 503, ex);
			return Status.UnhandledError;
		}

		internal static Status Report504NotImplementedIdsSchemaVersion(ILogger? logger, IdsVersion vrs)
		{
			logger?.LogError("Error {errorCode}: Embedded schema for version {vrs} not implemented.", 504, vrs);
			return Status.NotImplementedError;
		}
	}
}
