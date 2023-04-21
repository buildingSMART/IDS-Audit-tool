using IdsLib.IdsSchema.IdsNodes;

namespace IdsLib
{
    /// <summary>
    /// Configuration parameters required within the inner loop of the audit.
    /// </summary>
    public class AuditProcessOptions
    {
        /// <summary>
        /// If set to true skips the audit of the semantic aspects of the IDS, 
        /// which results in a basic test of the adherence to the xsd schema.
        /// </summary>
        public virtual bool OmitIdsContentAudit { get; set; } = false;
    }

    /// <summary>
    /// Configuration parameters needed to setup the execution of a single file audit.
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
        public virtual IdsVersion IdsVersion { get; set; } = IdsVersion.Ids0_9;
    }
}
