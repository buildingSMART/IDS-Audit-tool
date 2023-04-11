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
using System.Diagnostics;
using IdsLib.IdsSchema.IdsNodes;
using System.Collections.Concurrent;

namespace IdsLib;

/// <summary>
/// Main static class for the execution of the audit functions of the tool.
/// See the <see cref="Run(IdsLib.IAuditOptions, ILogger?)"/> and <see cref="Run(Stream, IdsLib.IAuditOptions, ILogger?)"/> for more details.
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
    }

    /// <summary>
    /// Main entry point to access the library features, when dealing with streams
    /// This is currently not implemented, while we work out the best way to pass streams
    /// to the underlying XML APIs that we use in <see cref="Run(IAuditOptions, ILogger?)"/>.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    [Obsolete("Still not implemeted; it's not to be considered obsolete, rather a reminder of work to do.")]
    public static Status Run(Stream idsSource, IAuditOptions opts, ILogger? logger = null)
    {
        throw new NotImplementedException("Will come soon.");
    }

    /// <summary>
    /// main entry point to access the library features, when dealing with files on the disk
    /// </summary>
    /// <param name="opts">configuraion options for the execution of audits</param>
    /// <param name="logger">the optional logger provides fine-grained feedback on all the audits performed</param>
    /// <returns>A status enum</returns>
    public static Status Run(IAuditOptions opts, ILogger? logger = null)
    {
        Status retvalue = Status.Ok;
        if (string.IsNullOrEmpty(opts.InputSource) && !opts.SchemaFiles.Any())
        {
            // no IDS and no schema => nothing to do
            logger?.LogWarning("No audits are required, with the options passed.");
            retvalue |= Status.InvalidOptionsError;
        }
        else if (string.IsNullOrEmpty(opts.InputSource)) 
        {
            // No ids, but we have a schemafile => check the schema itself
            opts.AuditSchemaDefinition = true;
        }
        if (!string.IsNullOrWhiteSpace(opts.OmitIdsContentAuditPattern))
        {
            try
            {
                // we are trying to see if the 
                var r = new Regex(opts.OmitIdsContentAuditPattern);
            }
            catch (ArgumentException)
            {
                logger?.LogWarning("Invalid OmitIdsContentAuditPattern `{pattern}`.", opts.OmitIdsContentAuditPattern);
                retvalue |= Status.InvalidOptionsError;
            }
        }
        if (retvalue.HasFlag(Status.InvalidOptionsError))
        {
            logger?.LogError("No audit performed.", opts.OmitIdsContentAuditPattern);
            return retvalue;
        }

        var auditsList = new List<string>();
        if (!string.IsNullOrEmpty(opts.InputSource))
            auditsList.Add("Ids structure");
        if (opts.AuditSchemaDefinition)
            auditsList.Add("Xsd schemas correctness");
        if (!opts.OmitIdsContentAudit)
        {
            if (!string.IsNullOrWhiteSpace(opts.OmitIdsContentAuditPattern))
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
        if (opts.AuditSchemaDefinition)
        {
            retvalue |= PerformSchemaCheck(opts, logger);
            if (retvalue != Status.Ok)
                return retvalue;
        }

        if (Directory.Exists(opts.InputSource))
        {
            var t = new DirectoryInfo(opts.InputSource);
            var ret = ProcessFolder(t, new AuditInfo(opts, logger), logger);
            return CompleteWith(ret, logger);
        }
        else if (File.Exists(opts.InputSource))
        {
            var t = new FileInfo(opts.InputSource);
            var ret = ProcessSingleFile(t, new AuditInfo(opts, logger), logger);
            return CompleteWith(ret, logger);
        }
        logger?.LogError("Invalid input source '{missingSource}'", opts.InputSource);
        return Status.NotFoundError;
    }

    private static Status CompleteWith(Status ret, ILogger? writer)
    {
        writer?.LogInformation("Completed with status: {status}.", ret);
        return ret;
    }

    private async static Task<Status> AuditIdsComplianceAsync(AuditInfo c, FileInfo theFile, ILogger? logger)
    {
        c.ValidatingFile = theFile.FullName;

        XmlReaderSettings rSettings;
        if (c.Options.SchemaFiles.Any())
        {
            // we load the schema settings from the configuration options
            rSettings = GetSchemaSettings(c.Options.SchemaFiles, logger);
        }
        else
        {
            // we load the schema settings from the file
            var info = IdsXmlHelpers.GetIdsInformationAsync(theFile).Result;
            if (info.Version == IdsVersion.Invalid)
            {
                logger?.LogError("IDS schema version not found, or not recognised ({vrs}).", info.SchemaLocation);
                return Status.IdsStructureError;
            }
            var sett = GetSchemaSettings(info.Version, logger);
            if (sett is null)
            {
                logger?.LogError("Embedded schema not found for IDS version {vrs}.", info.Version);
                return Status.NotImplementedError;
            }
            rSettings = sett;
        }
        rSettings.ValidationType = ValidationType.Schema;
        rSettings.Async = true;
        rSettings.ValidationEventHandler += new ValidationEventHandler(c.ValidationReporter);
        rSettings.IgnoreComments = true;
        rSettings.IgnoreWhitespace = true;

        using var src = File.OpenRead(theFile.FullName);
        var reader = XmlReader.Create(src, rSettings);
        var cntRead = 0;
        var elementsStack = new Stack<BaseContext>(); // we prepare the stack to evaluate the IDS content
        BaseContext? current = null;
        Status contentStatus = Status.Ok;

        bool omitContent = c.Options.OmitIdsContentAudit
            ||
            (!string.IsNullOrWhiteSpace(c.Options.OmitIdsContentAuditPattern) && Regex.IsMatch(theFile.FullName, c.Options.OmitIdsContentAuditPattern, RegexOptions.IgnoreCase));

        while (await reader.ReadAsync()) // the loop reads the entire file to trigger validation events.
        {
            cntRead++;
            if (!omitContent) // content audit can be omitted, but the while loop is still executed
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
        c.Logger?.LogDebug("Read {fullname}, {cntRead} elements.", theFile.FullName, cntRead);
        return c.Status | contentStatus;
    }

    private static XmlReaderSettings? GetSchemaSettings(IdsVersion vrs, ILogger? logger)
    {
        var rSettings = new XmlReaderSettings();
        var files = GetSchemaFiles(vrs, logger);
        if (!files.Any())
            return null;
        foreach (var schema in files) // from GetSchemaSettings
        {
            var tns = GetSchemaNamespace(schema);
            rSettings.Schemas.Add(tns, schema.FullName);
        }
        return rSettings;
    }

    private static XmlReaderSettings GetSchemaSettings(IEnumerable<string> diskSchemas, ILogger? logger)
    {
        var rSettings = new XmlReaderSettings();
        foreach (var schema in GetSchemaFiles(diskSchemas, logger)) // from GetSchemaSettings
        {
            var tns = GetSchemaNamespace(schema);
            rSettings.Schemas.Add(tns, schema.FullName);
        }
        return rSettings;
    }

    static readonly ConcurrentDictionary<string, string> NameSpaces = new();

    private static string GetSchemaNamespace(FileInfo schemaFile)
    {
        if (NameSpaces.ContainsKey(schemaFile.FullName))
            return NameSpaces[schemaFile.FullName];

        string tns = "";
        var re = new Regex(@"targetNamespace=""(?<tns>[^""]*)""");
        var t = File.ReadAllText(schemaFile.FullName);
        var m = re.Match(t);
        if (m.Success)
        {
            tns = m.Groups["tns"].Value;
        }
        NameSpaces.TryAdd(schemaFile.FullName, tns);
        return tns;
    }


    private static Status ProcessSingleFile(FileInfo theFile, AuditInfo c, ILogger? logger)
    {
        Status ret = Status.Ok;
        logger?.LogInformation("Auditing file: `{filename}`.", theFile.FullName);
        ret |= AuditIdsComplianceAsync(c, theFile, logger).Result;
        return ret;
    }

    private static Status ProcessFolder(DirectoryInfo directoryInfo, AuditInfo c, ILogger? logger)
    {
        string idsExtension = c.Options.InputExtension;
#if NETSTANDARD2_0
        var allIdss = directoryInfo.GetFiles($"*.{idsExtension}", SearchOption.AllDirectories).ToList();
#else
        var eop = new EnumerationOptions() { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };
        var allIdss = directoryInfo.GetFiles($"*.{idsExtension}", eop).ToList();
#endif
        Status ret = Status.Ok;
        var tally = 0;
        foreach (var ids in allIdss.OrderBy(x => x.FullName))
        {
            var sgl = ProcessSingleFile(ids, c, logger);
            ret |= sgl;
            tally++;
        }
        var fileCardinality = tally != 1 ? "files" : "file";
        c.Logger?.LogInformation("{tally} {fileCardinality} processed.", tally, fileCardinality);
        return ret;
    }

    private static Status PerformSchemaCheck(IAuditOptions auditOptions, ILogger? logger)
    {
        Status ret = Status.Ok;
        var rSettings = new XmlReaderSettings();
        foreach (var schemaFile in GetSchemaFiles(auditOptions.SchemaFiles, logger)) // within PerformSchemaCheck
        {
            try
            {
                var ns = GetSchemaNamespace(schemaFile);
                rSettings.Schemas.Add(ns, schemaFile.FullName);
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

    /// <summary>
    /// Provides access to the latest XSD schema, saved as a file in the temporary directory.
    /// This is useful because some of Microsoft's XML APIs rely on files rather than streams.
    /// </summary>
    /// <returns>A valid FileInfo class if successful, null on failure.</returns>
    public static FileInfo? GetLatestIdsSchema()
    {
        var file = ExtractResources(new[] {"ids.xsd"}).FirstOrDefault();
        if (file is null)
            return null;
        return new FileInfo(file);
    }

    private static IEnumerable<FileInfo> GetSchemaFiles(IdsVersion vrs, ILogger? logger = null)
    {
        List<string> resourceList;
        switch (vrs)
        {
            case IdsVersion.Ids0_9:
                resourceList = new List<string> { "xsdschema.xsd", "xml.xsd", "ids.xsd" };
                break;
            default:
                logger?.LogError("Embedded schema for version {vrs} not implemented.", vrs);
                yield break;
        }
        List<string> saved = ExtractResources(resourceList, logger);
        foreach (var item in saved)
        {
            var f = new FileInfo(item);
            if (f.Exists)
                yield return f;
        }
    }

    private static IEnumerable<FileInfo> GetSchemaFiles(IEnumerable<string> userDiskFiles, ILogger? logger)
    {
        var resourceList = new List<string> { "xsdschema.xsd", "xml.xsd" };
        List<string> saved = ExtractResources(resourceList, logger);
        foreach (var item in userDiskFiles.Union(saved))
        {
            var f = new FileInfo(item);
            if (f.Exists)
                yield return f;
        }
    }

    private static readonly ConcurrentDictionary<string, string> resourceToFile = new();

    private static List<string> ExtractResources(IEnumerable<string> resourcesIdentifiers, ILogger? logger = null)
    {
        // get the resources
        var resolvedFiles = new List<string>();
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var resourceIdentifier in resourcesIdentifiers)
        {
            if (resourceToFile.TryGetValue(resourceIdentifier, out var file))
            {
                resolvedFiles.Add(file);
            }
            else
            {
                string resourceName = assembly.GetManifestResourceNames()
                    .Single(str => str.EndsWith(resourceIdentifier));
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    logger?.LogError("Error extracting resource: {schema}", resourceIdentifier);
                    continue;
                }
                using var reader = new StreamReader(stream);
                string result = reader.ReadToEnd();
                var tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, result);
                
                // try to add, if cannot be done, get it from the dic,
                // it must have been added in the meanwhile to the static dictionary
                //
                if (resourceToFile.TryAdd(resourceIdentifier, tempFile))
                    resolvedFiles.Add(tempFile);
                else
                    resolvedFiles.Add(resourceToFile[resourceIdentifier]);
            }
        }
        return resolvedFiles;
    }
}
