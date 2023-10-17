using System.Collections.Generic;

namespace IdsLib.IdsSchema.IdsNodes
{
    internal interface IFiniteStringMatcher
    {
        IEnumerable<string> GetDicreteValues();
    }
    internal interface IStringPrefixMatcher
    {
        // the case of xs:restriction is not easy to test for regexes
        bool MatchesPrefix(string prefixString);
        // the string value for error feedback
        string Value { get; }
    }
}
