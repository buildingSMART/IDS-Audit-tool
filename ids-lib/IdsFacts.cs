using System;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacts
{
    internal static IdsVersion GetVersionFromLocation(string location, Microsoft.Extensions.Logging.ILogger? logger = null)
    {
        switch (location)
        {
            // the following are the only canonical versions accepted
			case "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/0.9.6/ids.xsd":
				return IdsVersion.Ids0_9;
			case "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/1.0/ids.xsd":
                return IdsVersion.Ids1_0;
            default:
                return IdsVersion.Invalid;
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
    /// Version 0.9; this might be removed before first official release
    /// </summary>
    Ids0_9,
    /// <summary>
    /// Version 1; this is not final until formal release.
    /// </summary>
    Ids1_0
}