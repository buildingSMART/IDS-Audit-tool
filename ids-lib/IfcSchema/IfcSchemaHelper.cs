using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using System;

namespace IdsLib.IfcSchema;

internal static class IfcSchemaHelper
{
    public static IfcSchemaVersions GetSchemaVersions(this string sourceString, IdsSpecification context, Microsoft.Extensions.Logging.ILogger? logger)
    {
        var ret = IfcSchemaVersions.IfcNoVersion;
        var split = sourceString.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);   
        foreach ( var ver in split ) 
        {
            ret |= ver switch
            {
                "IFC2X3" => IfcSchemaVersions.Ifc2x3,
                "IFC4" => IfcSchemaVersions.Ifc4,
                "IFC4X3" => IfcSchemaVersions.Ifc4x3,
                _ => IdsMessages.Report107InvalidIfcSchemaString(logger, ver, context) 
            };
        }
        return ret;
    }
}
