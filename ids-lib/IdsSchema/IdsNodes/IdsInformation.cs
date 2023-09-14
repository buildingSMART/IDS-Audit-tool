namespace IdsLib.IdsSchema.IdsNodes;


/// <summary>
/// Status information of an IDS source
/// </summary>
public class IdsInformation
{
    /// <summary>
    /// Boolean value that determines if the source is considered a valid IDS
    /// </summary>
    public bool IsIds { get; internal set; } = false;
    /// <summary>
    /// The declared schema location of the source
    /// </summary>
    public string SchemaLocation { get; internal set; } = string.Empty;
    /// <summary>
    /// A status message associated with the source. Empty if all is good.
    /// </summary>
    public string StatusMessage { get; internal set; } = string.Empty;
    /// <summary>
    /// The IDS version detected from the source, providing logging feedback.
    /// </summary>
    public IdsVersion GetVersion(Microsoft.Extensions.Logging.ILogger? logger = null)
    {
        return IdsFacts.GetVersionFromLocation(SchemaLocation, logger);
    }
	/// <summary>
	/// The IDS version detected from the source, without any logging feedback.
	/// </summary>
	public IdsVersion Version
    {
        get
        {
            return IdsFacts.GetVersionFromLocation(SchemaLocation);
        }
    }

	internal static IdsInformation CreateInvalid(string InvalidMessage)
    {
        return new IdsInformation
        {
            SchemaLocation = IdsVersion.Invalid.ToString(),
            StatusMessage = InvalidMessage
        };
    }
}
