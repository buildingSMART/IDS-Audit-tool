using IdsLib.IdsSchema.IdsNodes;

namespace IdsLib
{
    /// <summary>
    /// Configuration parameters needed within the execution loop of the audit.
    /// </summary>
    public class AuditProcessOptions
    {
        /// <summary>
        /// If set to true skips the audit of the semantic aspects of the IDS.
        /// </summary>
        public virtual bool OmitIdsContentAudit { get; set; } = false;
    }

    /// <summary>
    /// Configuration parameters needed to setup the execution of a single file audit.
    /// </summary>
    public class SingleAuditOptions : AuditProcessOptions
    {
        /// <summary>
        /// Defines what version of the schema we are auditing against.
        /// </summary>
        public virtual IdsVersion IdsVersion { get; set; } = IdsVersion.Ids0_9;
    }
}
