namespace IdsLib.IdsSchema.Cardinality
{
    internal interface ICardinality
    {
        bool IsRequired { get; }

        Audit.Status Audit(out string errorMessage);
    }
}