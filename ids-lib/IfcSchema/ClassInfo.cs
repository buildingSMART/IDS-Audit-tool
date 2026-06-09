using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IdsLib.IfcSchema;


/// <summary>
/// Information on the potential use of the class
/// </summary>
public enum ClassType
{
	/// <summary>
	/// Will not have concrete instances
	/// </summary>
	Abstract,
	/// <summary>
	/// Can have a concrete instantiation
	/// </summary>
	Concrete,
	/// <summary>
	/// Is an enumeration or closed values
	/// </summary>
	Enumeration
}

/// <summary>
/// the IFC classes we present can be classified with regards to their potential role in the IfcRelDefinesByType relation.
/// </summary>
public enum FunctionalType
{
	/// <summary>
	/// Can be found in the RelatedObjects side of the relation.
	/// Does not have a specific class defined in the RelatingType side of the relation.
	/// </summary>
	Element,
	/// <summary>
	/// Can be found in the RelatedObjects side of the relation.
	/// Does have a specific class defined in the RelatingType side of the relation.
	/// </summary>
	ElementWithTypes,
	/// <summary>
	/// Can be found in the RelatingType side of the relation.
	/// </summary>
	TypeOfElement
}

/// <summary>
/// Metadata container for properties of IFC classes
/// </summary>
[DebuggerDisplay("{Name}")]
public partial class ClassInfo
{
	/// <summary>
	/// Class Name as string (stored as camelcase, convert to upper for valid IDS class names)
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Parent name as string
	/// </summary>

	public string ParentName { get; private set; }

	/// <summary>
	/// Metadata about the class (concrete, abstract or enum).
	/// </summary>
	public ClassType Type { get; private set; }

	/// <summary>
	/// Resolved parent Classinfo
	/// </summary>
	public ClassInfo? Parent { get; internal set; }

	/// <summary>
	/// Is the class 
	/// </summary>
	public FunctionalType FunctionalType { get; internal set; } = FunctionalType.Element;

	/// <summary>
	/// List of predefined type strings from the schema
	/// </summary>
	public IEnumerable<string>? PredefinedTypeValues { get; private set; } = [];

	/// <summary>
	/// List of all attribute names for the type, if you need background attribute info, use <see cref="DirectAttributesInfo"/>
	/// </summary>
	public IEnumerable<string>? DirectAttributes => DirectAttributesInfo?.Select(x => x.Name);

	/// <summary>
	/// List of all attribute for the type with their metadata
	/// </summary>
	public IEnumerable<AttributeInfo>? DirectAttributesInfo { get; private set; } = [];

	/// <summary>
	/// List of enumeration values 
	/// </summary>
	public IEnumerable<string>? EnumerationValues { get; private set; } = [];

	/// <summary>
	/// Similar to the c# Is clause
	/// </summary>
	/// <param name="className">the class we are comparing against</param>
	public bool Is(string className)
	{
		if (Name.Equals(className, StringComparison.InvariantCultureIgnoreCase))
			return true;
		if (Parent != null)
			return Parent.Is(className);
		return false;
	}

	/// <summary>
	/// The namespace of the class
	/// </summary>
	public string NameSpace { get; internal set; }

	/// <summary>
	/// List of all subclasses.
	/// </summary>
	public List<ClassInfo> SubClasses = new();


	/// <summary>
	/// All matching concrete classes, including self and entire subclass tree
	/// </summary>
	public IEnumerable<ClassInfo> MatchingConcreteClasses
	{
		get
		{
			if (Type == ClassType.Concrete)
				yield return this;
			foreach (var item in SubClasses)
			{
				foreach (var sub in item.MatchingConcreteClasses)
				{
					yield return sub;
				}
			}
		}
	}

	/// <summary>
	/// Public constructor for classes without attribute info. Fakes attribute info with empty types.
	/// </summary>
	/// <param name="name">Class name</param>
	/// <param name="parentName">parent class name</param>
	/// <param name="type">Class type</param>
	/// <param name="predefined">List of valid predefinedtype strings</param>
	/// <param name="nameSpace">The IFC schema namespace</param>
	/// <param name="directAttributeNames">If you do not have attribute info available, use just the names, otherwise prefer the constructor with <see cref="AttributeInfo"/>.</param>
	public ClassInfo(string name, string parentName, ClassType type, IEnumerable<string> predefined, string nameSpace, IEnumerable<string> directAttributeNames)
	{
		Name = name;
		ParentName = parentName;
		Type = type;
		PredefinedTypeValues = predefined;
		NameSpace = nameSpace;
		DirectAttributesInfo = directAttributeNames.Select(x => new AttributeInfo(x, string.Empty));
	}

	/// <summary>
	/// Public constructor
	/// </summary>
	/// <param name="name">Class name</param>
	/// <param name="parentName">parent class name</param>
	/// <param name="type">Class type</param>
	/// <param name="predefined">List of valid predefinedtype strings</param>
	/// <param name="nameSpace">The IFC schema namespace</param>
	/// <param name="directAttributes">List of attribute info objects</param>
	public ClassInfo(string name, string parentName, ClassType type, IEnumerable<string> predefined, string nameSpace, IEnumerable<AttributeInfo>? directAttributes = null)
	{
		Name = name;
		ParentName = parentName;
		Type = type;
		PredefinedTypeValues = predefined;
		NameSpace = nameSpace;
		DirectAttributesInfo = directAttributes;
	}

	/// <summary>
	/// Create an enumeration type classinfo
	/// </summary>
	public ClassInfo(string enumName, string enumNameSpace, string[] values)
	{
		Name = enumName;
		NameSpace = enumNameSpace;
		Type = ClassType.Enumeration;
		ParentName = string.Empty;
		EnumerationValues = values;
	}

	/// <summary>
	/// What are the Type Classes related to the current (e.g. IfcWall to IfcWallType).
	/// </summary>
	public string[]? RelationTypeClasses { get; private set; }

	internal void AddTypeClasses(IEnumerable<string> typeClasses)
	{
		// if the object has type classes it's an entity
		FunctionalType = FunctionalType.ElementWithTypes;
		if (RelationTypeClasses is null)
		{
			RelationTypeClasses = typeClasses.ToArray();
		}
		else
		{
			RelationTypeClasses = typeClasses.Concat(RelationTypeClasses).ToArray();
		}
		if(Type == ClassType.Concrete)
		{
			// Allow propogation of things like IfcWallType to IfcWallStandardCase
			// while avoiding adding IfcElementType etc to all subclasses of IfcElement
			foreach (var subclass in SubClasses)
			{
				subclass.AddTypeClasses(typeClasses);
			}
		}
	}

	internal static ClassInfo CreateEnumeration(string enumName, string enumNameSpace, string[] values)
	{
		return new ClassInfo(enumName, enumNameSpace, values);
	}
}
