// taken on advice from 
// https://stackoverflow.com/questions/13188437/how-can-i-use-xml-schema-regex-in-c
// pointing to MS conversion implementation at
// https://github.com/Microsoft/referencesource/blob/master/System.Xml/System/Xml/Schema/FacetChecker.cs
// following MIT licence

using System.Text;

namespace IdsLib.IdsSchema.XsNodes;

internal class XmlRegex
{
    private struct Map
    {
        internal Map(char m, string r)
        {
            match = m;
            replacement = r;
        }
        internal char match;
        internal string replacement;
    };

    private static readonly Map[] c_map = {
        new Map('c', "\\p{_xmlC}"),
        new Map('C', "\\P{_xmlC}"),
        new Map('d', "\\p{_xmlD}"),
        new Map('D', "\\P{_xmlD}"),
        new Map('i', "\\p{_xmlI}"),
        new Map('I', "\\P{_xmlI}"),
        new Map('w', "\\p{_xmlW}"),
        new Map('W', "\\P{_xmlW}"),
    };


    internal static string Preprocess(string pattern, bool omitBoundaries = false)
    {
        var bufBld = new StringBuilder();
        if (!omitBoundaries)
            bufBld.Append('^');

        char[] source = pattern.ToCharArray();
        int length = pattern.Length;
        int copyPosition = 0;
        for (int position = 0; position < length - 1; position++)
        {
            if (source[position] == '\\')
            {
                if (source[position + 1] == '\\')
                {
                    position++; // skip it
                }
                else
                {
                    char ch = source[position + 1];
                    for (int i = 0; i < c_map.Length; i++)
                    {
                        if (c_map[i].match == ch)
                        {
                            if (copyPosition < position)
                            {
                                bufBld.Append(source, copyPosition, position - copyPosition);
                            }
                            bufBld.Append(c_map[i].replacement);
                            position++;
                            copyPosition = position + 1;
                            break;
                        }
                    }
                }
            }
        }
        if (copyPosition < length)
        {
            bufBld.Append(source, copyPosition, length - copyPosition);
        }
        if (!omitBoundaries)
            bufBld.Append('$');
        return bufBld.ToString();
    }

}
