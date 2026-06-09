using System;
using System.Collections.Generic;
using System.Text;

namespace IdsLib.IfcSchema
{
	/// <summary>
	/// Metadata container for attributes of IFC classes
	/// </summary>
	public record AttributeInfo
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public AttributeInfo(string name, string expressType)
		{
			Name = name;
			ExpressType = expressType;
		}

		/// <summary>
		/// Attribute Name as string (stored as camelcase)
		/// </summary>
		public string Name { get; private set; }
		
		/// <summary>
		/// full express type string definition
		/// </summary>
		public string ExpressType { get; private set; }

		/// <summary>
		/// Defines if the attribute is marked optional in the schema
		/// </summary>
		public bool IsOptional
		{
			get
			{
				return false;
			}
		}
	}
}
