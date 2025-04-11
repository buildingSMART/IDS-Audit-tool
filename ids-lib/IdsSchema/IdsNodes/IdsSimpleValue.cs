using IdsLib.IdsSchema.XsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;

namespace IdsLib.IdsSchema.IdsNodes;

internal partial class IdsSimpleValue : IdsXmlNode, IStringListMatcher, IStringPrefixMatcher, IFiniteStringMatcher
{
    internal string Content = string.Empty;

    public string Value => Content;

	public IdsSimpleValue(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
    }

    protected internal override void SetContent(string contentString)
    {
        Content = contentString ?? string.Empty;
    }     

    public Audit.Status MustMatchAgainstCandidates(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string variableName, IfcSchema.IfcSchemaVersions schemaContext)
    {
        if (!TryMatch(candidateStrings, ignoreCase, out matches))
            return IdsErrorMessages.Report103InvalidListMatcher(this, Content, logger, variableName, schemaContext, candidateStrings);
        return Audit.Status.Ok;
    }

    public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
    {
        var compCase = ignoreCase
            ? System.StringComparison.OrdinalIgnoreCase
            : System.StringComparison.Ordinal;
        matches = candidateStrings.Where(x => x.Equals(Content, compCase)).ToList();
        return matches.Any();
    }

    public bool MatchesPrefix(string prefixString)
    {
        return Content.StartsWith(prefixString);
    }

    public IEnumerable<string> GetDicreteValues()
    {
        yield return Content;
    }

    private static readonly string[] emptyStringArray = new[] { "" }; 

    internal static bool IsNullOrEmpty(IdsXmlNode? parentNode)
    {
        if (parentNode is null)
            return true;
        var frst = parentNode.Children.FirstOrDefault();
        return frst switch
        {
            IdsSimpleValue simple => simple.Content == string.Empty, // we need a spec, if it's empty then its null or empty
            XsRestriction rest => rest.TryMatch(emptyStringArray, false, out _),// if it can match an empty string then it's not a valid value spec
            _ => true,// if it's neither, then null
        };
    }
}
