using IdsLib.IdsSchema.IdsNodes;
using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes
{
	/// <summary>
	/// 
	/// </summary>
	internal class XsBoundary : IdsXmlNode, IFiniteStringMatcher
	{
		private readonly string value;

		public XsBoundary(XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
		{
			value = reader.GetAttribute("value") ?? string.Empty;
		}

		public IEnumerable<string> GetDicreteValues()
		{
			yield return value;
		}
	}
}
