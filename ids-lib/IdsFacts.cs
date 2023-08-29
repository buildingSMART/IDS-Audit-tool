using System;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacts
{
    internal static IdsVersion GetVersionFromLocation(string location)
    {
        return location switch
        {
            "http://standards.buildingsmart.org/IDS/ids_05.xsd" => IdsVersion.Ids0_5, // todo: this is invalid and needs to be fixed in the IDS repository
            "http://standards.buildingsmart.org/IDS  ids_09.xsd" => IdsVersion.Ids0_9, // todo: this is invalid and needs to be fixed in the IDS repository
            "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids_09.xsd" => IdsVersion.Ids0_9,
            "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids_1_0.xsd" => IdsVersion.Ids1_0,
            "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids.xsd" => IdsVersion.Ids1_0,
            "http://standards.buildingsmart.org/IDS ids.xsd" => IdsVersion.Ids1_0,
            _ => IdsVersion.Invalid,
        };
    }
}


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
    Ids1_0
}