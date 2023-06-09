﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IfcSchema
{
    /// <summary>
    /// Provides static methods to get the collection of classes in the published schemas.
    /// </summary>
    public partial class SchemaInfo : IEnumerable<ClassInfo>
    {
        /// <summary>
        /// The version of the schema represented in the info
        /// </summary>
        public IfcSchemaVersions Version { get; }

        /// <summary>
        /// Provides access to the property sets of the schema
        /// </summary>
        public IList<PropertySetInfo> PropertySets { get; }

        /// <summary>
        /// Provides metadata on IFC schemas, to support the correct compilation of the XIDS
        /// </summary>
        public SchemaInfo(IfcSchemaVersions schemaVersion)
        {
            Version = schemaVersion;
            Classes = new Dictionary<string, ClassInfo>();
            AttributesToAllClasses = new Dictionary<string, string[]>();
            AttributesToTopClasses = new Dictionary<string, string[]>();
            PropertySets = PropertySetInfo.GetSchema(schemaVersion)!;
        }

        /// <summary>
        /// Ensures that the information up to date.
        /// </summary>
        bool linked = false;
        private Dictionary<string, ClassInfo> Classes;

        /// <summary>
        /// from the attribute name to the names of the classes that have the attribute.
        /// </summary>
        private Dictionary<string, string[]> AttributesToAllClasses { get; set; }

        /// <summary>
        /// from the attribute name to the names of the minimum set of classes that declare the attribute (no subclasses).
        /// </summary>
        private Dictionary<string, string[]> AttributesToTopClasses { get; set; }

        /// <summary>
        /// Get the classinfo by name string.
        /// </summary>
        public ClassInfo? this[string className]
        {
            get
            {
                if (Classes.TryGetValue(className, out var cl))
                {
                    return cl;
                }
                return Classes.Values.FirstOrDefault(x => x.Name.Equals(className, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Add a new classInfo to the collection
        /// </summary>
        public void Add(ClassInfo classToAdd)
        {
            linked = false;
            Classes ??= new Dictionary<string, ClassInfo>();
            Classes.Add(classToAdd.Name, classToAdd);
        }

        /// <summary>
        /// Ensure relationships between classes are correct.
        /// </summary>
        private void LinkTree()
        {
            foreach (var currClass in Classes.Values)
            {
                var parent = currClass.ParentName;
                if (!string.IsNullOrWhiteSpace(parent) && Classes.TryGetValue(parent, out var resolvedParent))
                {
                    // if it's not in the subclasses yet, add it
                    if (!resolvedParent.SubClasses.Any(x => x.Name == currClass.Name))
                    {
                        resolvedParent.SubClasses.Add(currClass);
                    }
                    currClass.Parent = resolvedParent;
                }
            }
            linked = true;
        }

        private static SchemaInfo? schemaIfc4;
        /// <summary>
        /// Static property for the Ifc4 schema
        /// </summary>
        public static SchemaInfo SchemaIfc4
        {
            get
            {
                if (schemaIfc4 == null)
                {
                    var t = GetClassesIFC4();
                    GetRelationTypesIFC4(t);
                    GetAttributesIFC4(t);
                    SetTypeObject(t, "IfcTypeObject");
                    t.LinkTree();
                    schemaIfc4 = t;
                }
                return schemaIfc4;
            }
        }

        private static SchemaInfo? schemaIfc4x3;
        /// <summary>
        /// Static property for the Ifc4 schema
        /// </summary>
        public static SchemaInfo SchemaIfc4x3
        {
            get
            {
                if (schemaIfc4x3 == null)
                {
                    var t = GetClassesIFC4x3();
                    GetRelationTypesIFC4x3(t);
                    GetAttributesIFC4x3(t);
                    SetTypeObject(t, "IfcTypeObject");
                    t.LinkTree();
                    schemaIfc4x3 = t;
                }
                return schemaIfc4x3;
            }
        }


        private static void SetTypeObject(SchemaInfo t, string topTypeObjectClass)
        {
            foreach (var cls in t.Classes.Values)
            {
                if (cls.Is(topTypeObjectClass))
                    cls.FunctionalType = FunctionalType.TypeOfElement;
            }
        }

        /// <summary>
        /// Returns names of the classes that have an attribute.
        /// /// See <seealso cref="GetAttributeRelations(string)"/> for similar function with different return type.
        /// </summary>
        /// <param name="attributeName">The attribute being sought</param>
        /// <param name="onlyTopClasses">reduces the return to the minimum set of top level classes that have the attribute (no subclasses)</param>
        /// <returns>enumeration of class names, possibly empty, if not found</returns>
        public string[] GetAttributeClasses(string attributeName, bool onlyTopClasses = false)
        {
            var toUse = onlyTopClasses
                ? AttributesToTopClasses
                : AttributesToAllClasses;
            if (toUse.TryGetValue(attributeName, out var ret))
                return ret;
            return Array.Empty<string>();
        }

        private readonly Dictionary<string, ClassRelationInfo[]> relAttributes = new();

        /// <summary>
        /// Provides information of classes that have an attribute and the form of the relation to it.
        /// See <seealso cref="GetAttributeClasses(string, bool)"/> for similar function with different return type.
        /// </summary>
        /// <param name="attributeName">Name of the attribute in question</param>
        /// <returns></returns>
        public IEnumerable<ClassRelationInfo> GetAttributeRelations(string attributeName)
        {
            if (relAttributes.TryGetValue(attributeName, out var ret))
                return ret;
            var tmp = new List<ClassRelationInfo>();
            foreach (var className in GetAttributeClasses(attributeName, true))
            {
                var cls = this[className];
                if (cls == null)
                    continue;
                var tp = cls.FunctionalType == FunctionalType.TypeOfElement
                    ? ClassAttributeMode.ViaRelationType
                    : ClassAttributeMode.ViaElement;
                tmp.Add(new ClassRelationInfo()
                {
                    ClassName = className,
                    Connection = tp
                }
                    );
            }
            var t = tmp.ToArray();
            relAttributes.Add(attributeName, t);
            return t;
        }

        /// <summary>
        /// Relation that allows to connect an available attribute to an entity
        /// </summary>
        public enum ClassAttributeMode
        {
            /// <summary>
            /// The attribute is directly defined in an IfcClass
            /// </summary>
            ViaElement = 1,
            /// <summary>
            /// The attribute is defined in the type that can be related to an Ifc Class
            /// </summary>
            ViaRelationType = 2,
        }

        /// <summary>
        /// A structure contianing information about the ways in which an attribute is related to a class
        /// </summary>
        public struct ClassRelationInfo
        {
            /// <summary>
            /// Class name
            /// </summary>
            public string ClassName { get; set; }
            /// <summary>
            /// Mode of connection to the Class
            /// </summary>
            public ClassAttributeMode Connection { get; set; }
        }

        internal static IEnumerable<string> PossibleTypesForPropertySets(IfcSchemaVersions version, IEnumerable<string> possiblePsetNames)
        {
            IEnumerable<string> ret = Enumerable.Empty<string>();
            foreach (var psetName in possiblePsetNames)
            {
                IEnumerable<string>? thisPsetTypes = null;
                foreach (var schema in GetSchemas(version))
                {
                    var propSet = schema.PropertySets.Where(x => x.Name == psetName).FirstOrDefault();
                    if (propSet is null)
                    {
                        thisPsetTypes = Enumerable.Empty<string>();
                        break;
                    }

                    if (thisPsetTypes == null)
                        thisPsetTypes = schema.GetAllConcreteFrom(propSet.ApplicableClasses);
                    else
                        thisPsetTypes = thisPsetTypes.Intersect(schema.GetAllConcreteFrom(propSet.ApplicableClasses));
                }
                thisPsetTypes ??= Enumerable.Empty<string>();
                ret = ret.Union(thisPsetTypes);
            }
            return ret.Distinct();
        }

        private IEnumerable<string> GetAllConcreteFrom(IList<string> startingClassNames)
        {
            IEnumerable<string>? ret = null;
            foreach (var className in startingClassNames)
            {
                if (!Classes.TryGetValue(className, out var cls))
                    continue;
                if (ret is null)
                    ret = cls.MatchingConcreteClasses.Select(x => x.Name);
                else
                    ret = ret.Union(cls.MatchingConcreteClasses.Select(x => x.Name)).Distinct();
            }
            if (ret is null)
                return Enumerable.Empty<string>();
            return ret;
        }

        /// <summary>
        /// the set of classes that satisfy the attributes across all provided schemas
        /// </summary>
        internal static IEnumerable<string> SharedClassesForAttributes(IfcSchemaVersions version, IEnumerable<string> attributes)
        {
            IEnumerable<string> ret = Enumerable.Empty<string>();
            foreach (var attributeName in attributes)
            {
                IEnumerable<string>? thisAttributeClasses = null;
                foreach (var schema in GetSchemas(version))
                {
                    var attributeClasses = schema.GetAttributeClasses(attributeName);
                    if (thisAttributeClasses == null)
                        thisAttributeClasses = attributeClasses;
                    else
                        thisAttributeClasses = thisAttributeClasses.Intersect(attributeClasses);
                }
                thisAttributeClasses ??= Enumerable.Empty<string>();
                ret = ret.Union(thisAttributeClasses);
            }
            return ret;
        }

        internal static IEnumerable<string> SharedPropertyNames(IfcSchemaVersions version, IEnumerable<string> possiblePsetNames)
        {
            IEnumerable<string> ret = Enumerable.Empty<string>();
            foreach (var psetName in possiblePsetNames)
            {
                IEnumerable<string>? thisPsetProperties = null;
                foreach (var schema in GetSchemas(version))
                {
                    var propSet = schema.PropertySets.Where(x=>x.Name == psetName).FirstOrDefault();
                    if (propSet is null)
                    {
                        thisPsetProperties = Enumerable.Empty<string>();
                        break;
                    }    
                    if (thisPsetProperties == null)
                        thisPsetProperties = propSet.PropertyNames;
                    else
                        thisPsetProperties = thisPsetProperties.Intersect(propSet.PropertyNames);
                }
                thisPsetProperties ??= Enumerable.Empty<string>();
                ret = ret.Union(thisPsetProperties);
            }
            return ret;
        }

        // todo: this is probably a performance problem, we could be caching in memory
        internal static IEnumerable<string> SharedPropertySetNames(IfcSchemaVersions version)
        {
            IEnumerable<string>? ret = null;
            foreach (var schema in GetSchemas(version))
            {
                if (ret == null)
                    ret = schema.PropertySets.Select(x => x.Name);
                else
                    ret = ret.Intersect(schema.PropertySets.Select(x => x.Name));
            }
            if (ret is null)
                return Enumerable.Empty<string>();
            return ret;
        }



        //private static List<string>? allSchemaAttributes = null;

        ///// <summary>
        ///// The names of all attributes across all schemas.
        ///// </summary>
        //public static IEnumerable<string> AllSchemasAttributes
        //{
        //    get
        //    {
        //        allSchemaAttributes ??= SchemaIfc2x3.GetAttributeNames().Union(SchemaIfc4.GetAttributeNames()).Distinct().ToList();
        //        return allSchemaAttributes; 
        //    }
        //}

        private static SchemaInfo? schemaIfc2x3;
        /// <summary>
        /// Static property for the Ifc2x3 schema
        /// </summary>
        public static SchemaInfo SchemaIfc2x3
        {
            get
            {
                if (schemaIfc2x3 == null)
                {
                    var t = GetClassesIFC2x3();
                    GetRelationTypesIFC2x3(t);
                    GetAttributesIFC2x3(t);
                    SetTypeObject(t, "IfcTypeObject");
                    t.LinkTree();
                    schemaIfc2x3 = t;
                }
                return schemaIfc2x3;
            }
        }

        static partial void GetRelationTypesIFC2x3(SchemaInfo schema);
        static partial void GetRelationTypesIFC4(SchemaInfo schema);
        static partial void GetRelationTypesIFC4x3(SchemaInfo schema);

        internal void SetRelationType(string objClass, IEnumerable<string> typeClasses)
        {
            var c = this[objClass];
            c?.SetTypeClasses(typeClasses);
            foreach (var typeClass in typeClasses)
            {
                var tpC = this[typeClass];
                if (tpC != null)
                    tpC.FunctionalType = FunctionalType.TypeOfElement;
            }
        }


        private static partial SchemaInfo GetClassesIFC2x3();

        private static partial SchemaInfo GetClassesIFC4();

        private static partial SchemaInfo GetClassesIFC4x3();

        /// <summary>
        /// Returns all attribute names in the schema
        /// </summary>
        public IEnumerable<string> GetAttributeNames()
        {
            return AttributesToAllClasses.Keys;
        }

        static partial void GetAttributesIFC2x3(SchemaInfo destinationSchema);
        static partial void GetAttributesIFC4(SchemaInfo destinationSchema);
        static partial void GetAttributesIFC4x3(SchemaInfo destinationSchema);

        private void AddAttribute(string attributeName, string[] topClassNames, string[] allClassNames)
        {
            AttributesToAllClasses ??= new Dictionary<string, string[]>();
            AttributesToAllClasses.Add(attributeName, allClassNames);

            AttributesToTopClasses ??= new Dictionary<string, string[]>();
            AttributesToTopClasses.Add(attributeName, topClassNames);
        }

        /// <summary>
        /// The default enumerator for the schema returns the classes defined within
        /// </summary>
        public IEnumerator<ClassInfo> GetEnumerator()
        {
            if (!linked)
                LinkTree();
            return Classes.Values.GetEnumerator();
        }

        /// <summary>
        /// The default enumerator for the schema returns the classes defined within
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!linked)
                LinkTree();
            return Classes.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns the schema metadata information for the required versions.
        /// </summary>
        /// <param name="schemaVersions">required</param>
        /// <returns></returns>
        public static IEnumerable<SchemaInfo> GetSchemas(IfcSchemaVersions schemaVersions)
        {
            if (schemaVersions.HasFlag(IfcSchemaVersions.Ifc2x3))
                yield return SchemaIfc2x3;
            if (schemaVersions.HasFlag(IfcSchemaVersions.Ifc4))
                yield return SchemaIfc4;
            if (schemaVersions.HasFlag(IfcSchemaVersions.Ifc4x3))
                yield return SchemaIfc4x3;
        }

        internal static string? ValidMeasureForAllProperties(IfcSchemaVersions version, IEnumerable<string> possiblePsetNames, IEnumerable<string> possiblePropertyNames)
        {
            string? ret = null;
            foreach (var schema in GetSchemas(version))
            {
                var propsMatchingRequirements = schema.PropertySets.Where(x => possiblePsetNames.Contains(x.Name)).SelectMany(pset => pset.Properties.Where(prop => possiblePropertyNames.Contains(prop.Name)));
                foreach (var prop in propsMatchingRequirements)
                {
                    if (!prop.HasDataType(out var dt))
                        return null;
                    if (ret is null)
                        ret = dt;
                    else if (ret != dt)
                        return null;
                }
            }
            return ret;
        }
    }
}
