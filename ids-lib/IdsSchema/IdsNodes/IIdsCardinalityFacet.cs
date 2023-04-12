using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes
{
    /// <summary>
    /// Standard interface to identify facets
    /// </summary>
    internal interface IIdsFacet
    {
        bool IsValid { get; }
    }
    /// <summary>
    /// Interface to ensure that facets used as requirements can have more checks run 
    /// on them:
    /// - min and max occurrence checks
    /// </summary>
    internal interface IIdsCardinalityFacet : IIdsFacet
    {
        /// <summary>
        /// Used for facets that have more audits if used as requirements than if used in the applicability
        /// </summary>
        Audit.Status PerformCardinalityAudit(ILogger? logger);

        /// <summary>
        /// This will be true for all applicabiliy facets, but will be false for optional and prohibited
        /// Logical type constraints are only applied to the ones that are required, excluiding optional and prohibited.
        /// </summary>
        bool IsRequired { get; }
    }

}
