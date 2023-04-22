using System;

namespace IdsLib.IdsSchema.IdsNodes;

/// <summary>
/// Enumeration to identify a single IDS version.
/// </summary>
public enum IdsVersion
{
    /// <summary>
    /// Invalid or incomplete version information
    /// </summary>
    Invalid,
    /// <summary>
    /// Legacy version 0.5
    /// </summary>
    [Obsolete("This will likely be removed before first official release.")]
    Ids0_5,
    /// <summary>
    /// Version 0.9; this might be removed before first official release
    /// </summary>
    Ids0_9,
    /// <summary>
    /// Version 1; this is not final until formal release.
    /// </summary>
    Ids1_0,
}

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
    /// The IDS version detected from the source.
    /// </summary>
    public IdsVersion Version
    {
        get
        {
            return SchemaLocation switch
            {
                "http://standards.buildingsmart.org/IDS/ids_05.xsd" => IdsVersion.Ids0_5, // todo: this is invalid and needs to be fixed in the IDS repository
                "http://standards.buildingsmart.org/IDS  ids_09.xsd" => IdsVersion.Ids0_9, // todo: this is invalid and needs to be fixed in the IDS repository
                "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids_09.xsd" => IdsVersion.Ids0_9,
                "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids_1_0.xsd" => IdsVersion.Ids1_0,
                "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids.xsd" => IdsVersion.Ids0_9,
                _ => IdsVersion.Invalid,
            };
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
