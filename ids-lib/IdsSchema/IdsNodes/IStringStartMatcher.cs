using System.Collections.Generic;

namespace IdsLib.IdsSchema.IdsNodes
{
    internal interface IFiniteStringMatcher
    {
        IEnumerable<string> GetDicreteValues();
    }
    internal interface IStringPrefixMatcher
    {
        // the case of xs:restriction is not easy to test
        // perhaps https://github.com/moodmosaic/Fare can help with regexes
        bool MatchesPrefix(string prefixString);
    }
}
