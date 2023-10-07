using System.Xml.Schema;
using System.Xml;
using Microsoft.Extensions.Logging;
using IdsLib.Messages;
using static IdsLib.Audit;
using System.Collections.Generic;
using System;

namespace IdsLib;

internal class AuditHelper
{
    internal ILogger? Logger;

    internal record BufferedValidationIssue
    {
		public LogLevel Level { get; init; }
		public string Message { get; init; }
		public int Line { get; init; }
		public int Position { get; init; }
		public Original Schema { get; init; }

        public enum Original
        {
            SchemaError,
			SchemaWarning,
        }

		public BufferedValidationIssue(LogLevel information, string message, int line, int pos, Original schema)
		{
            Level = information;
			Message = message;
            Line = line;
            Position = pos;
            Schema = schema;
		}

		internal void Notify(ILogger? logger, IdsSchema.IdsXmlNode newContext)
		{
            var loc =  newContext.GetNodeIdentification();
            loc.StartLineNumber = Line ;
            loc.StartLinePosition = Position;
            loc.NodeType = newContext.type;
            if (Schema == Original.SchemaError)
                IdsErrorMessages.Report306SchemaComplianceError(logger, loc, Message);
            else
                IdsErrorMessages.Report307SchemaComplianceWarning(logger, Level, loc, Message);
		}
	}

    public AuditProcessOptions Options { get; }

    internal Queue<BufferedValidationIssue> BufferedValidationIssues = new();

    public AuditHelper(ILogger? logger, AuditProcessOptions options)
    {
        Logger = logger;
        Options = options;
    }

    public Status SchemaStatus { get; internal set; }

    public void ValidationReporter(object? sender, ValidationEventArgs e)
    {
        int line = 0, pos = 0;
        // preparing location
        var location = "position unknown";
        if (sender is IXmlLineInfo rdr)
        {
            location = $"line {rdr.LineNumber}, position {rdr.LinePosition}";
            line = rdr.LineNumber;
            pos = rdr.LinePosition;
        }
        // reporting issues
        if (e.Severity == XmlSeverityType.Warning)
        {
            switch (Options.XmlWarningAction)
            {
                case AuditProcessOptions.XmlWarningBehaviour.ReportAsInformation:
                    //IdsMessages.ReportSchemaComplianceWarning(Logger, LogLevel.Information, location, e.Message);
                    BufferedValidationIssues.Enqueue(new BufferedValidationIssue(
                        LogLevel.Information, e.Message, line, pos, BufferedValidationIssue.Original.SchemaWarning
						));
					// status is not changed
					break;
                case AuditProcessOptions.XmlWarningBehaviour.ReportAsWarning:
                    //IdsMessages.ReportSchemaComplianceWarning(Logger, LogLevel.Warning, location, e.Message);
					BufferedValidationIssues.Enqueue(new BufferedValidationIssue(
						LogLevel.Warning, e.Message, line, pos, BufferedValidationIssue.Original.SchemaWarning
						));
					SchemaStatus |= Status.IdsStructureWarning;
                    break;
                case AuditProcessOptions.XmlWarningBehaviour.ReportAsError:
                    // the type is reported as an error, but its original nature of warning is retained to help debug
                    //IdsMessages.ReportSchemaComplianceWarning(Logger, LogLevel.Error, location, e.Message);
					BufferedValidationIssues.Enqueue(new BufferedValidationIssue(
						LogLevel.Error, e.Message, line, pos, BufferedValidationIssue.Original.SchemaWarning
						));
					SchemaStatus |= Status.IdsStructureError;
                    break;
                default: // nothing to do
                    break;
            }
        }
        else if (e.Severity == XmlSeverityType.Error)
        {
            //IdsMessages.ReportSchemaComplianceError(Logger, location, e.Message);
			BufferedValidationIssues.Enqueue(new BufferedValidationIssue(
						LogLevel.Error, e.Message, line, pos, BufferedValidationIssue.Original.SchemaError
						));
			SchemaStatus |= Status.IdsStructureError;
        }
    }
}
