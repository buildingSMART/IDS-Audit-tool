using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using IdsLib.IdsSchema;
using System.Data;
using IdsLib.SchemaProviders;
using IdsLib.Messages;
using IdsLib.IdsSchema.IdsNodes;

namespace IdsLib;

/// <summary>
/// Main static class for invoking the audit functions.
/// 
/// If you wish to audit a single file, the best entry point is <see cref="Run(Stream, SingleAuditOptions, ILogger?)"/>.
/// This method allows you to run audits on the provided stream.
/// 
/// For more complex auditing scenarios (e.g. those used by the tool), some automation can be achieved with <see cref="Run(IdsLib.IBatchAuditOptions, ILogger?)"/>.
/// 
/// Both APIs provide a return value that can be interpreted to determine if errors have been found.
/// 
/// For more detailed feedback on the specific location of issues encountered, you must pass an <see cref="ILogger"/> interface, and collect events.
/// </summary>
public static partial class Audit
{
    /// <summary>
    /// Summary return status of the audit functions
    /// </summary>
    [Flags]
    public enum Status
    {
        /// <summary>
        /// No errors encountered
        /// </summary>
        Ok = 0,
        /// <summary>
        /// The tool did not complete all the audits, because some aspect of the process are not implemented yet
        /// </summary>
        NotImplementedError = 1 << 0,
        /// <summary>
        /// The options provided in input are incomplete or inconsistent.
        /// </summary>
        InvalidOptionsError = 1 << 1,
        /// <summary>
        /// A resources passed via file name was not found.
        /// </summary>
        NotFoundError = 1 << 2,
        /// <summary>
        /// When auditing an IDS, one or more errors were encountered in the XML structure (includes XSD compliance errors).
        /// Depending on the <see cref="AuditProcessOptions.XmlWarningAction"/> property, this might include XSD schema warnings.
        /// </summary>
        IdsStructureError = 1 << 3,
        /// <summary>
        /// When auditing an IDS, one or more errors encountered auditing against the implementation agreement.
        /// </summary>
        IdsContentError = 1 << 4,
        /// <summary>
        /// A custom XSD was passed, but it could not be used because of an error in its content or structure.
        /// </summary>
        XsdSchemaError = 1 << 6,
        /// <summary>
        /// An unmanaged error occurred in the main audit methods. Please contact the authors to address the problem.
        /// </summary>
        UnhandledError = 1 << 7,
        /// <summary>
        /// When auditing an IDS, one or more warnings were encountered in the XML structure as defined by the XSD schemas.
        /// Triggering this status is configurable using the <see cref="AuditProcessOptions.XmlWarningAction"/> property.
        /// </summary>
        IdsStructureWarning = 1 << 8,
    }

    /// <summary>
    /// Main entry point to access the library features via a stream to read the IDS content.
    /// </summary>
    /// <param name="idsSource">the stream providing access to the content of the IDS to be audited</param>
    /// <param name="options">specifies the behaviour of the audit</param>
    /// <param name="logger">the optional logger provides fine-grained feedback on all the audits performed and any issues encountered</param>
    /// <returns>A status enum that summarizes the result for all audits on the single stream</returns>
    public static Status Run(Stream idsSource, SingleAuditOptions options, ILogger? logger = null)
    {
        var auditSettings = new AuditHelper(logger, options);
        return AuditStreamAsync(idsSource, auditSettings, logger).Result; // in  run(stream)
    }

    /// <summary>
    /// Entry point to access the library features in batch mode either on directories or single files
    /// </summary>
    /// <param name="batchOptions">configuration options for the execution of audits on a file or folder</param>
    /// <param name="logger">the optional logger provides fine-grained feedback on all the audits performed</param>
    /// <returns>A status enum that summarizes the result for all audits executed</returns>
    public static Status Run(IBatchAuditOptions batchOptions, ILogger? logger = null)
    {
        Status retvalue = Status.Ok;
        if (string.IsNullOrEmpty(batchOptions.InputSource) && !batchOptions.SchemaFiles.Any())
		{
			// no IDS and no schema => nothing to do
			retvalue |= IdsToolMessages.ReportNoActionRequired(logger);
		}
		else if (string.IsNullOrEmpty(batchOptions.InputSource))
        {
            // No ids, but we have a schemafile => check the schema itself
            batchOptions.AuditSchemaDefinition = true;
        }
        if (!string.IsNullOrWhiteSpace(batchOptions.OmitIdsContentAuditPattern))
        {
            try
            {
                // we are trying to see if the 
                var r = new Regex(batchOptions.OmitIdsContentAuditPattern);
            }
            catch (ArgumentException)
            {
                retvalue |= IdsToolMessages.ReportInvalidPattern(logger, batchOptions.OmitIdsContentAuditPattern);
            }
        }
        if (retvalue.HasFlag(Status.InvalidOptionsError))
        {
			IdsToolMessages.ReportNoAudit(logger);
            return retvalue;
        }

        var auditsList = new ActionCollection();
        if (!string.IsNullOrEmpty(batchOptions.InputSource))
            auditsList.Add(Action.IdsStructure);
        if (batchOptions.AuditSchemaDefinition)
            auditsList.Add(Action.XsdCorrectness);
        if (!batchOptions.OmitIdsContentAudit)
        {
            if (!string.IsNullOrWhiteSpace(batchOptions.OmitIdsContentAuditPattern))
                auditsList.Add(Action.IdsContentWithOmissions);
            else
                auditsList.Add(Action.IdsContent);
        }
        if (!auditsList.Any())
        {
            return IdsToolMessages.ReportInvalidOptions(logger);
        }

        // inform on the config
        
        IdsToolMessages.ReportActions(logger, auditsList);

        // start audit
        if (batchOptions.AuditSchemaDefinition)
        {
            retvalue |= PerformSchemaCheck(batchOptions, logger);
            if (retvalue != Status.Ok)
                return retvalue;
        }

        if (Directory.Exists(batchOptions.InputSource))
        {
            var t = new DirectoryInfo(batchOptions.InputSource);
            var ret = ProcessFolder(t, batchOptions, logger);
            return CompleteWith(ret, logger);
        }
        else if (File.Exists(batchOptions.InputSource))
        {
            var t = new FileInfo(batchOptions.InputSource);
            var ret = ProcessSingleFile(t, batchOptions, logger);
            return CompleteWith(ret, logger);
        }
        return IdsToolMessages.ReportInvalidSource(logger, batchOptions.InputSource);
	}

	private static Status CompleteWith(Status ret, ILogger? writer)
    {
        writer?.LogInformation("Completed with status: {status}.", ret);
        return ret;
    }

    private async static Task<Status> AuditIdsComplianceAsync(IBatchAuditOptions options, FileInfo theFile, ILogger? logger)
    {
        var opts = new AuditProcessOptions()
        {
            SchemaProvider =
                (options.SchemaFiles.Any())
                ? new FileBasedSchemaProvider(options.SchemaFiles, logger) // we load the schemas from the configuration options
                : new SeekableStreamSchemaProvider(), // we determine the schema version from the file,
            OmitIdsContentAudit =
                options.OmitIdsContentAudit ||
                (!string.IsNullOrWhiteSpace(options.OmitIdsContentAuditPattern) && Regex.IsMatch(theFile.FullName, options.OmitIdsContentAuditPattern, RegexOptions.IgnoreCase))
        };
        var auditSettings = new AuditHelper(logger, opts);
        using var stream = File.OpenRead(theFile.FullName);
        return await AuditStreamAsync(stream, auditSettings, logger); // in AuditIdsComplianceAsync
    }

    private static XmlReaderSettings GetXmlSettings(AuditProcessOptions options)
    {
        // todo: we should set the validation type only once the schemas are loaded
        var rSettings = new XmlReaderSettings()
        {
            ValidationType = options.OmitIdsSchemaAudit ? ValidationType.None : ValidationType.Schema,
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true
        };
        rSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        return rSettings;
    }

    private static async Task<Status> AuditStreamAsync(Stream theStream, AuditHelper auditSettings, ILogger? logger)
    {
        Status contentStatus = Status.Ok;
        // the handler needs to be set before creating the reader,
        // otherwise the validation event is not registered
        var rSettings = GetXmlSettings(auditSettings.Options);
        if (!auditSettings.Options.OmitIdsSchemaAudit)
            rSettings.ValidationEventHandler += new ValidationEventHandler(auditSettings.ValidationReporter);
        var schemaLoadingStatus = PopulateSchema(theStream, auditSettings.Options.SchemaProvider, logger, rSettings.Schemas);
        if (schemaLoadingStatus != Status.Ok)
            return auditSettings.SchemaStatus | contentStatus | schemaLoadingStatus;

        var reader = XmlReader.Create(theStream, rSettings);

        var cntRead = 0;
        var elementsStack = new Stack<IdsXmlNode>(); // prepare the stack to evaluate the IDS content
        int iSpecification = 1;
        IdsXmlNode? current = null;
        var prevSchemaStatus = auditSettings.SchemaStatus;

		while (await reader.ReadAsync()) // the loop reads the entire file to trigger validation events.
        {
            cntRead++;

            switch (reader.NodeType)
            {
                // audits are performed on closing the element end, so that all the children are available for evaluation.
                // but empty elements (e.g., <someElement />) are audited upon opening, as there are no children to evaluate
                //
                case XmlNodeType.Element:
                    IdsXmlNode? parent = null;
#if NETSTANDARD2_0
                        if (elementsStack.Count > 0)
                            parent = elementsStack.Peek();
#else
                    if (elementsStack.TryPeek(out var peeked))
                        parent = peeked;
#endif
                    var newContext = IdsXmlHelpers.GetContextFromElement(reader, parent, logger); // this is always not null
                    if (newContext is IdsSpecification spec)
                        // parents of IdsSpecification do not retain children for Garbage Collection purposes
                        // so we need to set the positional index manually
                        spec.PositionalIndex = iSpecification++;
                    while (auditSettings.BufferedValidationIssues.Any())
                    {
                        var queuedIssue = auditSettings.BufferedValidationIssues.Dequeue();
                        if (
                            newContext.type == "attribute"
                            &&
                            (queuedIssue.Message.Contains("minOccurs") || queuedIssue.Message.Contains("maxOccurs"))
                            )
                        {
                            // this could fail under some circumstances, but it's a temporary workaround
                            auditSettings.SchemaStatus = prevSchemaStatus;
                            continue;
						}
                        queuedIssue.Notify(logger, newContext);
                    }

                    // we only push on the stack if it's not empty, e.g.: <some /> does not go on the stack
                    if (!reader.IsEmptyElement)
                        elementsStack.Push(newContext);
                    else
                    {
						if (!auditSettings.Options.OmitIdsContentAudit)
							contentStatus |= newContext.PerformAudit(logger); // invoking audit on empty element happens immediately
                    }
                    current = newContext;
                    break;

                case XmlNodeType.Text:
                    // Debug.WriteLine($"  Text Node: {reader.GetValueAsync().Result}");
                    current!.SetContent(reader.GetValueAsync().Result);
                    break;
                case XmlNodeType.EndElement:
                    // Debug.WriteLine($"End Element {reader.LocalName}");
                    var closing = elementsStack.Pop();
                    // Debug.WriteLine($"  auditing {closing.type} on end element");
                    if (!auditSettings.Options.OmitIdsContentAudit)
                    {
                        contentStatus |= closing.PerformAudit(logger); // invoking audit on end of element
                    }
                    break;
                default:
                    // Debug.WriteLine("Other node {0} with value '{1}'.", reader.NodeType, reader.Value);
                    break;
            }
			prevSchemaStatus = auditSettings.SchemaStatus;
		}

        reader.Dispose();
        if (!auditSettings.Options.OmitIdsSchemaAudit)
            rSettings.ValidationEventHandler -= new ValidationEventHandler(auditSettings.ValidationReporter);

        IdsToolMessages.ReportReadCount(logger, cntRead);

        return auditSettings.SchemaStatus | contentStatus;
    }

    private static Status PopulateSchema(Stream vrs, ISchemaProvider schemaProvider, ILogger? logger, XmlSchemaSet destSchemas)
    {
        var ret = schemaProvider.GetSchemas(vrs, logger, out var schemas);
        if (ret != Status.Ok)
            return ret;
        foreach (var schema in schemas)
        {
            destSchemas.Add(schema);
        }
        try
        {
            destSchemas.Compile();
            var names = destSchemas.GlobalElements.Names.OfType<XmlQualifiedName>().Select(x => x.ToString());
            if (!names.Contains("http://standards.buildingsmart.org/IDS:ids"))
                return XsdMessages.ReportMissingIdsDefinition(logger);
        }
        catch (Exception ex)
        {
			return XsdMessages.ReportXsdCompilationError(logger, ex.Message);
        }
        return ret;

    }

    private static Status ProcessSingleFile(FileInfo theFile, IBatchAuditOptions batchOptions, ILogger? logger)
    {
        Status ret = Status.Ok;
        IdsToolMessages.ReportFileProcessingStarted(logger, theFile.FullName);
        ret |= AuditIdsComplianceAsync(batchOptions, theFile, logger).Result;
        return ret;
    }

    private static Status ProcessFolder(DirectoryInfo directoryInfo, IBatchAuditOptions options, ILogger? logger)
    {
#if NETSTANDARD2_0
        var allIdss = directoryInfo.GetFiles($"*.{options.InputExtension}", SearchOption.AllDirectories).ToList();
#else
        var eop = new EnumerationOptions() { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };
        var allIdss = directoryInfo.GetFiles($"*.{options.InputExtension}", eop).ToList();
#endif
        Status ret = Status.Ok;
        var tally = 0;
        foreach (var ids in allIdss.OrderBy(x => x.FullName))
        {
            var sgl = ProcessSingleFile(ids, options, logger);
            ret |= sgl;
            tally++;
        }
		IdsToolMessages.ReportFileProcessingEnded(logger, tally);

		return ret;
    }

    /// todo: remove, possibly relocate to <see cref="FileBasedSchemaProvider"/>.
    private static Status PerformSchemaCheck(IBatchAuditOptions auditOptions, ILogger? logger)
    {
        Status ret = Status.Ok;
        var rSettings = new XmlReaderSettings();
        foreach (var schemaFile in auditOptions.SchemaFiles) // within PerformSchemaCheck
        {
            try
            {
                using var reader = File.OpenText(schemaFile);
                var schema = XmlSchema.Read(reader, null);
                if (schema is null)
                {
                    ret |= XsdMessages.ReportNullSchema(logger, schemaFile);
					continue;
                }
                rSettings.Schemas.Add(schema);
            }
            catch (XmlSchemaException ex)
            {
				ret |= XsdMessages.ReportSchemaException(logger, schemaFile, ex.Message, ex.LineNumber, ex.LinePosition);
            }
            catch (Exception ex)
            {
				ret |= XsdMessages.ReportException(logger, schemaFile, ex.Message);
            }
        }
        return ret;
    }
}
