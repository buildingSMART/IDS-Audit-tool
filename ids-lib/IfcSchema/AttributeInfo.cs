using System;
using System.Collections.Generic;
using System.Text;

namespace IdsLib.IfcSchema
{
	/// <summary>
	/// Metadata container for the attributes of a single entity in the IfcSchema.
	/// Contains their name, express type definition, and some helper properties to quickly access the most important information about the type.
	/// </summary>
	public record AttributeInfo
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public AttributeInfo(string name, string expressDefinition)
		{
			Name = name;
			ExpressType = new ExpressDefinition(expressDefinition);
		}

		/// <summary>
		/// Attribute Name as string (stored as camelcase)
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// full express type string definition
		/// </summary>
		public ExpressDefinition ExpressType { get; private set; }

		/// <inheritdoc cref="ExpressDefinition.IsOptional"/>
		public bool IsOptional => ExpressType.IsOptional;
		
		/// <inheritdoc cref="ExpressDefinition.IsCollection"/>
		public bool IsCollection => ExpressType.IsCollection;

		/// <inheritdoc cref="ExpressDefinition.XmlBaseType"/>
		public string BaseType => ExpressType.BaseType;

		/// <inheritdoc cref="ExpressDefinition.XmlBaseType"/>
		public string XmlBaseType => ExpressType.XmlBaseType;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Name}: {ExpressType.Definition}";
		}
	}
}
