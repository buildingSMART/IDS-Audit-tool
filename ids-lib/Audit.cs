using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using IdsLib.IdsSchema;
using IdsLib.IdsSchema.IdsNodes;
using System.Data;

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
        /// When auditing an IDS, ne or more errors encountered in the XML structure (includes XSD compliance errors).
        /// </summary>
        IdsStructureError = 1 << 3,
        /// <summary>
        /// When auditing an IDS, one or more errors encountered auditing against the implementation agreement.
        /// </summary>
        IdsContentError = 1 << 4,
        /// <summary>
        /// A custom XSD was passed, but it could not be used because of an error in its content or structure.
        /// </summary>
        XsdSchemaError = 1 << 5,
        /// <summary>
        /// An unmanaged error occurred in the main audit methods. Please contact the authors to address the problem.
        /// </summary>
        UnhandledError = 1 << 6,
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
        var xsett = GetSchemaSettings(options.IdsVersion, logger);
        if (xsett is null)
            return Status.NotImplementedError;
        FinalizeSettings(xsett);
        return AuditStreamAsync(idsSource, auditSettings, xsett, logger).Result;
    }

    /// <summary>
    /// Entry point to access the library features in batch mode either on directories or single files
    /// </summary>
    /// <param name="options">configuration options for the execution of audits</param>
    /// <param name="logger">the optional logger provides fine-grained feedback on all the audits performed</param>
    /// <returns>A status enum that summarizes the result for all audits executed</returns>
    public static Status Run(IBatchAuditOptions options, ILogger? logger = null)
    {
        Status retvalue = Status.Ok;
        if (string.IsNullOrEmpty(options.InputSource) && !options.SchemaFiles.Any())
        {
            // no IDS and no schema => nothing to do
            logger?.LogWarning("No audits are required, with the options passed.");
            retvalue |= Status.InvalidOptionsError;
        }
        else if (string.IsNullOrEmpty(options.InputSource))
        {
            // No ids, but we have a schemafile => check the schema itself
            options.AuditSchemaDefinition = true;
        }
        if (!string.IsNullOrWhiteSpace(options.OmitIdsContentAuditPattern))
        {
            try
            {
                // we are trying to see if the 
                var r = new Regex(options.OmitIdsContentAuditPattern);
            }
            catch (ArgumentException)
            {
                logger?.LogWarning("Invalid OmitIdsContentAuditPattern `{pattern}`.", options.OmitIdsContentAuditPattern);
                retvalue |= Status.InvalidOptionsError;
            }
        }
        if (retvalue.HasFlag(Status.InvalidOptionsError))
        {
            logger?.LogError("No audit performed.", options.OmitIdsContentAuditPattern);
            return retvalue;
        }

        var auditsList = new List<string>();
        if (!string.IsNullOrEmpty(options.InputSource))
            auditsList.Add("Ids structure");
        if (options.AuditSchemaDefinition)
            auditsList.Add("Xsd schemas correctness");
        if (!options.OmitIdsContentAudit)
        {
            if (!string.IsNullOrWhiteSpace(options.OmitIdsContentAuditPattern))
                auditsList.Add("Ids content (omitted on regex match)");
            else
                auditsList.Add("Ids content");
        }
        if (!auditsList.Any())
        {
            logger?.LogError("Invalid options.");
            return Status.InvalidOptionsError;
        }
        // inform on the config
        logger?.LogInformation("Auditing: {audits}.", string.Join(", ", auditsList.ToArray()));

        // start audit
        if (options.AuditSchemaDefinition)
        {
            retvalue |= PerformSchemaCheck(options, logger);
            if (retvalue != Status.Ok)
                return retvalue;
        }

        if (Directory.Exists(options.InputSource))
        {
            var t = new DirectoryInfo(options.InputSource);
            var ret = ProcessFolder(t, options, logger);
            return CompleteWith(ret, logger);
        }
        else if (File.Exists(options.InputSource))
        {
            var t = new FileInfo(options.InputSource);
            var ret = ProcessSingleFile(t, options, logger);
            return CompleteWith(ret, logger);
        }
        logger?.LogError("Invalid input source '{missingSource}'", options.InputSource);
        return Status.NotFoundError;
    }

    private static Status CompleteWith(Status ret, ILogger? writer)
    {
        writer?.LogInformation("Completed with status: {status}.", ret);
        return ret;
    }

    private async static Task<Status> AuditIdsComplianceAsync(IBatchAuditOptions options, FileInfo theFile, ILogger? logger)
    {
        XmlReaderSettings rSettings;
        if (options.SchemaFiles.Any())
        {
            // we load the schema settings from the configuration options
            rSettings = GetSchemaSettings(options.SchemaFiles, logger);
        }
        else
        {
            // we determine the schema version from the file
            var info = IdsXmlHelpers.GetIdsInformationAsync(theFile).Result;
            var vrs = info.Version;
            var loc = info.SchemaLocation;

            if (vrs == IdsVersion.Invalid)
            {
                logger?.LogError("IDS schema version not found, or not recognised ({vrs}).", loc);
                return Status.IdsStructureError;
            }
            var sett = GetSchemaSettings(vrs, logger);
            if (sett is null)
            {
                logger?.LogError("Embedded schema not found for IDS version {vrs}.", vrs);
                return Status.NotImplementedError;
            }
            rSettings = sett;
        }
        FinalizeSettings(rSettings);

        var opts = new AuditProcessOptions()
        {
            OmitIdsContentAudit = options.OmitIdsContentAudit ||
                (!string.IsNullOrWhiteSpace(options.OmitIdsContentAuditPattern) && Regex.IsMatch(theFile.FullName, options.OmitIdsContentAuditPattern, RegexOptions.IgnoreCase))
        };
        var auditSettings = new AuditHelper(logger, opts);

        using var stream = File.OpenRead(theFile.FullName);
        return await AuditStreamAsync(stream, auditSettings, rSettings, logger);
    }

    private static void FinalizeSettings(XmlReaderSettings rSettings)
    {
        rSettings.ValidationType = ValidationType.Schema;
        rSettings.Async = true;
        rSettings.IgnoreComments = true;
        rSettings.IgnoreWhitespace = true;
    }

    private static async Task<Status> AuditStreamAsync(Stream theStream, AuditHelper auditSettings, XmlReaderSettings rSettings, ILogger? logger)
    {
        Status contentStatus = Status.Ok;
        XmlReader reader;
        try
        {
            // the creation is inside a try block because there might be problems with when 
            // using schemas from the end user
            //
            reader = XmlReader.Create(theStream, rSettings);
        }
        catch (Exception ex)
        {
            logger?.LogCritical("{exceptionType}: {exceptionMessage}", ex.GetType().Name, ex.Message);
            return auditSettings.SchemaStatus | contentStatus | Status.XsdSchemaError;
        }
#if false
        int tot = 0;
        foreach (XmlSchema item in rSettings.Schemas.Schemas())
        {
            tot += item.Elements.Names.Count;
        }
        logger?.LogDebug("XmlReaderSettings has {schemaCount} schemas and {tot} elements", rSettings.Schemas.Count, tot);
#endif 
        var cntRead = 0;
        var elementsStack = new Stack<BaseContext>(); // we prepare the stack to evaluate the IDS content
        BaseContext? current = null;
        rSettings.ValidationEventHandler += new ValidationEventHandler(auditSettings.ValidationReporter);

        try
        {
            while (await reader.ReadAsync()) // the loop reads the entire file to trigger validation events.
            {
                cntRead++;
                if (!auditSettings.Options.OmitIdsContentAudit) // content audit can be omitted, but the while loop is still executed
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            // Debug.WriteLine($"Start Element {reader.LocalName}");
                            BaseContext? parent = null;
#if NETSTANDARD2_0
                            if (elementsStack.Count > 0)
                                parent = elementsStack.Peek();
#else
                            if (elementsStack.TryPeek(out var peeked))
                                parent = peeked;
#endif
                            var newContext = IdsXmlHelpers.GetContextFromElement(reader, parent, logger); // this is always not null

                            // we only push on the stack if it's not empty, e.g.: <some /> does not go on the stack
                            if (!reader.IsEmptyElement)
                                elementsStack.Push(newContext);
                            else
                                contentStatus |= newContext.PerformAudit(logger); // invoking audit empty element
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
                            contentStatus |= closing.PerformAudit(logger); // invoking audit on end of element
                            break;
                        default:
                            // Debug.WriteLine("Other node {0} with value '{1}'.", reader.NodeType, reader.Value);
                            break;
                    }
                }
            }
        }
        catch (XmlSchemaValidationException svex)
        {
            logger?.LogError("{exceptionType}: {exceptionMessage}", svex.GetType().Name, svex.Message);
            return auditSettings.SchemaStatus | contentStatus | Status.IdsStructureError;
        }
        catch (Exception ex)
        {
            logger?.LogError("{exceptionType}: {exceptionMessage}", ex.GetType().Name, ex.Message);
            return auditSettings.SchemaStatus | contentStatus | Status.UnhandledError;
        }
        finally
        {
            reader.Dispose();
            rSettings.ValidationEventHandler -= new ValidationEventHandler(auditSettings.ValidationReporter);
        }
        auditSettings.Logger?.LogDebug("Completed reading {cntRead} xml elements.", cntRead);
        return auditSettings.SchemaStatus | contentStatus;
    }

    private static XmlReaderSettings? GetSchemaSettings(IdsVersion vrs, ILogger? logger)
    {
        var rSettings = new XmlReaderSettings();
        var schemas = GetSchemasByVersion(vrs, logger);
        if (!schemas.Any())
            return null;
        foreach (var schema in schemas)
        {
            rSettings.Schemas.Add(schema);
        }
        return rSettings;
    }

    private static XmlReaderSettings GetSchemaSettings(IEnumerable<string> diskSchemas, ILogger? logger)
    {
        var rSettings = new XmlReaderSettings();
        var imports = new List<string>();
        foreach (var diskSchema in diskSchemas) 
        {
            using var reader = File.OpenText(diskSchema);
            var schema = XmlSchema.Read(reader, null);
            if (schema is null)
            {
                logger?.LogError("XSD\t{schemaFile}\tSchema error.", diskSchema);
                continue;
            }
            foreach (var location in schema.Includes.OfType<XmlSchemaImport>().Select(x => x.SchemaLocation))
            {
                if (location is null)
                    continue;
                imports.Add(location);
            }

            rSettings.Schemas.Add(schema);
        }
        // also get required reference schemas
        foreach (var schema in GetSchemasFromImports(logger, imports))
        {
            rSettings.Schemas.Add(schema);
        }
        return rSettings;
    }

    private static IEnumerable<XmlSchema> GetSchemasFromImports(ILogger? logger, IEnumerable<string> imports)
    {
        var distinct = imports.Distinct();
        foreach (var schema in distinct) 
        {
            switch (schema)
            {
                case "http://www.w3.org/2001/xml.xsd":
                    yield return GetSchema("xml.xsd")!;
                    yield return GetSchema("xsdschema.xsd")!;
                    break;
                case "https://www.w3.org/2001/XMLSchema.xsd":
                    break;
                case "http://www.w3.org/2001/XMLSchema-instance":
                    break;
                default:
                    logger?.LogError("Unexpected import schema {schema}.", schema);
                    break;
            }
        }
    }

    private static Status ProcessSingleFile(FileInfo theFile, IBatchAuditOptions options, ILogger? logger)
    {
        Status ret = Status.Ok;
        logger?.LogInformation("Auditing file: `{filename}`.", theFile.FullName);
        ret |= AuditIdsComplianceAsync(options, theFile, logger).Result;
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
        var fileCardinality = tally != 1 ? "files" : "file";
        logger?.LogInformation("{tally} {fileCardinality} processed.", tally, fileCardinality);
        return ret;
    }

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
                    logger?.LogError("XSD\t{schemaFile}\tSchema error.", schemaFile);
                    ret |= Status.XsdSchemaError;
                    continue;
                }
                rSettings.Schemas.Add(schema);
            }
            catch (XmlSchemaException ex)
            {
                logger?.LogError("XSD\t{schemaFile}\tSchema error: {errMessage} at line {line}, position {pos}.", schemaFile, ex.Message, ex.LineNumber, ex.LinePosition);
                ret |= Status.XsdSchemaError;
            }
            catch (Exception ex)
            {
                logger?.LogError("XSD\t{schemaFile}\tSchema error: {errMessage}.", schemaFile, ex.Message);
                ret |= Status.XsdSchemaError;
            }
        }
        return ret;
    }

    private static IEnumerable<XmlSchema> GetSchemasByVersion(IdsVersion vrs, ILogger? logger = null)
    {
        List<string> resourceList;
        switch (vrs)
        {
            case IdsVersion.Ids0_9:
            case IdsVersion.Ids1_0:
                resourceList = new List<string> { "xsdschema.xsd", "xml.xsd", "ids.xsd" };
                break;
            default:
                logger?.LogError("Embedded schema for version {vrs} not implemented.", vrs);
                yield break;
        }
        foreach (var item in resourceList.Select(x => GetSchema(x)))
        {
            if (item is not null)
                yield return item;
        }
        
    }
        
    private static XmlSchema GetSchema(string name)
    {
        var fullName = "IdsLib.Resources.XsdSchemas." + name;
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName)
                ?? throw new NotImplementedException("Null resource stream.");
        var schema = XmlSchema.Read(stream, null)
            ?? throw new NotImplementedException("Invalid resource stream.");
        return schema;
    }
}
