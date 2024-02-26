using IdsLib.IdsSchema.IdsNodes;

namespace IdsLib
{
    /// <summary>
    /// Configuration parameters required within the inner loop of the audit.
    /// </summary>
    public class AuditProcessOptions
    {
        /// <summary>
        /// Defines the specific schemas to be loaded for the validation.
        /// </summary>
        public virtual Audit.ISchemaProvider SchemaProvider { get; set; } = new SchemaProviders.SeekableStreamSchemaProvider();

        /// <summary>
        /// If set to true skips the audit of the structural aspects of the IDS.
        /// </summary>
        public virtual bool OmitIdsSchemaAudit { get; set; } = false;

        /// <summary>
        /// If set to true skips the audit of the semantic aspects of the IDS.
        /// </summary>
        public virtual bool OmitIdsContentAudit { get; set; } = false;

        /// <summary>
        /// The action taken when an IDS file contains warnings about its structure defined by XSD Schema
        /// </summary>
        public virtual XmlWarningBehaviour XmlWarningAction { get; set; } = XmlWarningBehaviour.ReportAsInformation;

        /// <summary>
        /// Options for the reporting action taken when encountering Xsd warning in IDS files
        /// </summary>
        public enum XmlWarningBehaviour
        {
            /// <summary>
            /// No information or error is triggered, the warning is ignored. Report a status of <see cref="Audit.Status.Ok"/> and no information log.
            /// </summary>
            Ignore,
            /// <summary>
            /// Report a status of <see cref="Audit.Status.Ok"/>, but issue information to the logger.
            /// </summary>
            ReportAsInformation,
            /// <summary>
            /// Report a status of <see cref="Audit.Status.IdsStructureWarning"/>.
            /// </summary>
            ReportAsWarning,
            /// <summary>
            /// Report a status of <see cref="Audit.Status.IdsStructureError"/>.
            /// </summary>
            ReportAsError
        }
    }

    /// <summary>
    /// Configuration parameters needed to setup the audit of a single IDS.
    /// The <see cref="IdsVersion"/> property is currently required to avoid the need to seek the stream, 
    /// then resume the audit once the version is detected from the content.
    /// 
    /// Future versions will attempt to relax this requirement.
    /// 
    /// Ensure that the properties of the base class <see cref="AuditProcessOptions"/> are also populated.
    /// </summary>
    public class SingleAuditOptions : AuditProcessOptions
    {
        /// <summary>
        /// Defines what version of the schema we are auditing against.
        /// </summary>
        public virtual IdsVersion IdsVersion { get; set; } = IdsFacts.DefaultIdsVersion;
    }
}
