using Microsoft.Extensions.Logging;

namespace IdsLib.Messages;

internal class XsdMessages
{
	internal static Audit.Status ReportMissingIdsDefinition(ILogger? logger)
	{
		logger?.LogError("IDS definition missing in schema.");
		return Audit.Status.XsdSchemaError;
	}

	internal static Audit.Status ReportNullSchema(ILogger? logger, string schemaFile)
	{
		logger?.LogError("XSD error on {schemaFile}.", schemaFile);
		return Audit.Status.XsdSchemaError;
	}

	internal static Audit.Status ReportSchemaException(ILogger? logger, string schemaFile, string message, int lineNumber, int linePosition)
	{
		logger?.LogError("XSD error on {schemaFile}: {errMessage} at line {line}, position {pos}.", schemaFile, message, lineNumber, linePosition);
		return Audit.Status.XsdSchemaError;
	}

	internal static Audit.Status ReportException(ILogger? logger, string schemaFile, string message)
	{
		logger?.LogError("XSD error on {schemaFile}: {errMessage}.", schemaFile, message);
		return Audit.Status.XsdSchemaError;
	}

	internal static Audit.Status ReportXsdCompilationError(ILogger? logger, string message)
	{
		logger?.LogError("Schema compilation error: {message}", message);
		return Audit.Status.XsdSchemaError;
	}

	internal static void ReportSchemaIssue(ILogger? logger, LogLevel level, string message)
	{
		logger?.Log(level, "{message}", message);
	}

	internal static void ReportUnexpectedSchema(ILogger? logger, string schema)
	{
		logger?.LogError("Unexpected import schema {schema}.", schema);
	}
}
