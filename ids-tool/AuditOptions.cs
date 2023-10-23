using CommandLine;
using IdsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsTool;

/// <summary>
/// Class has been renamed, use <see cref="BatchAuditOptions"/>
/// </summary>
[Obsolete("Class has been renamed to BatchAuditOptions")]
public class AuditOptions
{

}

/// <summary>
/// Concrete class passed to the ids-tool library, when Running the audit.
/// See <see cref="IBatchAuditOptions"/> for more details.
/// </summary>
[Verb("audit", HelpText = "Audits ids files and/or their xsd schema.")]
public class BatchAuditOptions : IBatchAuditOptions
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Option('x', "xsd", Required = false, HelpText = "XSD schema(s) to load, this is useful when testing changes in the schema (e.g. GitHub repo). If this is not specified, an embedded schema is adopted depending on the each ids's declaration of version.")]
    public IEnumerable<string> SchemaFiles { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Option('s', "schema", Default = false, Required = false, HelpText = "Check validity of the xsd schema(s) passed with the `xsd` option. This is useful for the development of the schema and it is in use in the official repository for quality assurance purposes.")]
    public bool AuditSchemaDefinition { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Option('e', "extension", Default = "ids", Required = false, HelpText = "When passing a folder as source, this defines which files to audit by extension.")]
    public string InputExtension { get; set; } = "ids";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Value(0,
        MetaName = "source",
        HelpText = "Input IDS to be processed; it can be a file or a folder.",
        Required = false)]
    public string InputSource { get; set; } = string.Empty; 

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Option('c', "omitContent", Required = false, HelpText = "Skips the audit of the agreed limitation of IDS contents.")]
    public bool OmitIdsContentAudit { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Option('p', "omitContentAuditPattern", Default = "", Required = false, HelpText = "Regex applied to file name to omit the audit of the semantic aspects of the IDS.")]
    public string OmitIdsContentAuditPattern { get; set; } = string.Empty;
}
