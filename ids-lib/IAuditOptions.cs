using System.Collections.Generic;

namespace IdsLib;

/// <summary>
/// This interface contains the parameters to configure the execution of the audit.
/// </summary>
public interface IAuditOptions
{
    /// <summary>
    /// XSD schema to load, this is useful when testing changes in the schema (e.g. GitHub repo).
    /// </summary>
    IEnumerable<string> SchemaFiles { get; set; }
    /// <summary>
    /// Check validity of the xsd schema(s) passed with the <see cref="SchemaFiles"/> parameter.
    /// </summary>
    bool AuditSchemaDefinition { get; set; }
    /// <summary>
    /// This is used to define the extension to load when passing a folder in the <see cref="InputSource"/> parameter. In doubt set it to "ids".
    /// </summary>
    string InputExtension { get; set; }
    /// <summary>
    /// Input IDS to be processed; it can be a file or a folder.
    /// </summary>
    string InputSource { get; set; }
    /// <summary>
    /// If set to true skips the audit of the semantic aspects of the IDS.
    /// </summary>
    bool OmitIdsContentAudit { get; set; }
    /// <summary>
    /// Regex applied to file name to omit the audit of the semantic aspects of the IDS.
    /// This can be useful to omit strict audit of files that are known to have content issues 
    /// (e.g. failing documentation of the buildingSMART IDS website).
    /// </summary>
    string OmitIdsContentAuditPattern { get; set; }
}