using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdsLib.IfcSchema
{
	/// <summary>
	/// Metadata container for the definition of an express entity. This is used to store the full express definition of a class or type, including cardinality and optionality.
	/// </summary>
	public record ExpressDefinition
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ExpressDefinition(string expressDefinition)
		{
			Definition = expressDefinition;
			XmlBaseType = SchemaInfo.TryParseIfcDataType(BaseType, out var dataTypeInfo)
				? dataTypeInfo.BackingType ?? "" // initialize to empty string if a backing type is not defined
				: string.Empty; // if it's not a measure, we keep the base type as the xml base type (e.g. for entity references)
		}

		/// <summary>
		/// The Xs:Type corresponding to the express definition, if it can be determined. 
		/// For example, for "IfcLabel" it would return "xs:string", for "IfcInteger" it would return "xs:integer", 
		/// and for "IfcGloballyUniqueId" it would return "xs:string".
		/// </summary>
		public string XmlBaseType { get; private set; }

		/// <summary>
		/// The full express definition of a class or type, including cardinality and optionality.
		/// </summary>
		public string Definition { get; }

		/// <summary>
		/// true if the type is defined as optional (i.e. it can be null).
		/// This is also valid for collections.
		/// </summary>
		public bool IsOptional
		{
			get
			{
				return Definition.StartsWith("OPTIONAL ", StringComparison.InvariantCultureIgnoreCase); 
			}
		}

		/// <summary>
		/// true if the type is a collection (e.g. a SET, LIST, ARRAY, etc.).
		/// </summary>
		public bool IsCollection
		{
			get
			{
				return Definition.Contains(" OF ", StringComparison.OrdinalIgnoreCase);
			}
		}

		/// <summary>
		/// Regardless of nullability and cardinality, provides the underlying type name used in the express definition. 
		/// For example, for "OPTIONAL IfcLabel" it would return "IfcLabel", for "SET [1:?] OF IfcInteger" it would return "IfcInteger", 
		/// and for "IfcGloballyUniqueId" it would return "IfcGloballyUniqueId".
		/// </summary>
		public string BaseType
		{
			get
			{
				return Definition.Split([' '], StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
			}
		}

	}
}
