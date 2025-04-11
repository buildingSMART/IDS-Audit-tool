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
	internal class EnumerationSetMatcher : IStringListMatcher
	{
		private readonly List<XsEnumeration> _enumerations = new List<XsEnumeration>();
		private XsRestriction xsRestriction;

		public EnumerationSetMatcher(XsRestriction xsRestriction)
		{
			this.xsRestriction = xsRestriction;
		}

		public Audit.Status MustMatchAgainstCandidates(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string variableName, IfcSchemaVersions schemaContext)
		{
			// in the case of enumeration, not all must match against the candidates, but at least one match must be found
			var ret = Audit.Status.Ok;
			var mtc = TryMatch(candidateStrings, ignoreCase, out matches);
			if (!mtc)
			{
				ret |= IdsErrorMessages.Report103InvalidListMatcher(xsRestriction, "Enumeration options", logger, variableName, schemaContext, candidateStrings);
			}
			return ret;
		}

		public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
		{
			// conditions are in OR with themselves for the enums
			//
			matches = new List<string>(); // start with empty set
			foreach (var child in _enumerations)
			{
				if (child.TryMatch(candidateStrings, ignoreCase, out var thisChildMatch))
					matches = matches.Union(thisChildMatch).ToList(); // add the last matches
			}
			return matches.Any();
		}

		internal void Add(XsEnumeration asEnum)
		{
			_enumerations.Add(asEnum);
		}
	}
}
