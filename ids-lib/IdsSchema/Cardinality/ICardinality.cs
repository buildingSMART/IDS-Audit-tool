namespace IdsLib.IdsSchema.Cardinality
{
    internal interface ICardinality
    {
        bool IsRequired { get; }
        bool IsProhibited { get; }

        Audit.Status Audit(out string errorMessage);
    }

    internal static class CardinalityConstants
    {
        internal const Audit.Status CardinalityErrorStatus = IdsLib.Audit.Status.IdsContentError;
    }
}