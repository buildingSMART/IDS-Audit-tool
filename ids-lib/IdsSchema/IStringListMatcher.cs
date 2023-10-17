using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace IdsLib.IdsSchema
{
    internal interface IStringListMatcher
    {
        Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string variableName, IfcSchema.IfcSchemaVersions schemaContext);
        bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches);
    }
}
