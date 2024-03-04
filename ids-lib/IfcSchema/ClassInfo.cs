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
/// Complext metedata container for properties of IFC classes
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
    public IEnumerable<string> PredefinedTypeValues { get; private set; }

    /// <summary>
    /// List of attribute names for the type
    /// </summary>
    public IEnumerable<string> DirectAttributes { get; private set; }

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
    /// Public constructor
    /// </summary>
public ClassInfo(string name, string parentName, ClassType type, IEnumerable<string> predefined, string nameSpace, IEnumerable<string> directAttributes)
    {
        Name = name;
        ParentName = parentName;
        Type = type;
        PredefinedTypeValues = predefined;
        NameSpace = nameSpace;
        DirectAttributes = directAttributes;
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
        foreach (var subclass in SubClasses)
        {
            subclass.AddTypeClasses(typeClasses);
        }
    }
}
