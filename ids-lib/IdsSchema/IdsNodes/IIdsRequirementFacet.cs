using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes
{
    /// <summary>
    /// Interface to ensure that facets used as requirements can have more checks run 
    /// on them:
    /// - min and max occurrence checks
    /// </summary>
    internal interface IIdsRequirementFacet
    {
        /// <summary>
        /// Used for facets that have more audits if used as requirements than if used in the applicability
        /// </summary>
        Audit.Status PerformAuditAsRequirement(ILogger? logger);
    }
}
