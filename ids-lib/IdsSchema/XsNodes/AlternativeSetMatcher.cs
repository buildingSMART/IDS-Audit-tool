using IdsLib.IfcSchema;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;

namespace IdsLib.IdsSchema.XsNodes
{
	internal class AlternativeSetMatcher : IStringListMatcher
	{
		private readonly List<IStringListMatcher> _alternatives = new List<IStringListMatcher>();
		private XsRestriction _xsRestriction;
		private string _alternativeTypeName;
		public AlternativeSetMatcher(XsRestriction xsRestriction, string alternativeTypeName)
		{
			_xsRestriction = xsRestriction;
			_alternativeTypeName = alternativeTypeName;
		}

		public Audit.Status MustMatchAgainstCandidates(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string variableName, IfcSchemaVersions schemaContext)
		{
			// in the case of enumeration, not all must match against the candidates, but at least one match must be found
			var ret = Audit.Status.Ok;
			var mtc = TryMatch(candidateStrings, ignoreCase, out matches);
			if (!mtc)
			{
				ret |= IdsErrorMessages.Report103InvalidListMatcher(_xsRestriction, _alternativeTypeName + " options", logger, variableName, schemaContext, candidateStrings);
			}
			return ret;
		}

		public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
		{
			// conditions are in OR with themselves for the enums
			//
			matches = []; // start with empty set
			foreach (var child in _alternatives)
			{
				if (child.TryMatch(candidateStrings, ignoreCase, out var thisChildMatch))
					matches = matches.Union(thisChildMatch).ToList(); // add the last matches
			}
			return matches.Any();
		}

		internal void Add(IStringListMatcher asEnum)
		{
			_alternatives.Add(asEnum);
		}
	}
}
