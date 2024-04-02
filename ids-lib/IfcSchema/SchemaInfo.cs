using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;

namespace IdsLib.IfcSchema
{
    /// <summary>
    /// Provides static methods to get the collection of classes in the published schemas.
    /// </summary>
    [DebuggerDisplay("{Version}")]
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
            AttributesToAllClasses = [];
			AttributesToTopClasses = [];
            AttributesToValueTypes = [];
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
		/// from the attribute name to the names of the minimum set of classes that declare the attribute (no subclasses).
		/// </summary>
		private Dictionary<string, string[]?> AttributesToValueTypes { get; set; }

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
		/// Obsolete function, please use the direct replacement TryParseIfcDataType instead, with the same parameter structure
		/// </summary>
		/// <returns></returns>
		[Obsolete("Use TryParseIfcDataType instead")]
		public static bool TryParseIfcMeasure(string value, [NotNullWhen(true)] out IfcDataTypeInformation? found, bool strict = true)
        {
            return TryParseIfcDataType(value, out found, strict);
		}

		/// <summary>
		/// Attempts to convert a string value to an instance of the IfcMeasureInformation
		/// </summary>
		/// <param name="value">the dataType string to parse</param>
		/// <param name="found"></param>
		/// <param name="strict">if true only accepts capitalized data, otherwise tolerates case inconsistencies</param>
		/// <returns>true if a match is found</returns>
		public static bool TryParseIfcDataType(string value, [NotNullWhen(true)] out IfcDataTypeInformation? found, bool strict = true)
		{
			if (value == null)
			{
				found = null;
				return false;
			}
			if (strict)
			{
				found = AllDataTypes.FirstOrDefault(x => x.IfcDataTypeClassName.ToUpper() == value);
				return found != null;
			}
			found = AllDataTypes.FirstOrDefault(x => x.IfcDataTypeClassName.Equals(value, StringComparison.InvariantCultureIgnoreCase));
			return found != null;
		}

		/// <summary>
		/// A selection of all the measures available in <see cref="AllDataTypes"/>.
		/// </summary>
		public static IEnumerable<IfcMeasureInformation> AllMeasureInformation => AllDataTypes.Where(x => x.Measure is not null).Select(x => x.Measure!.Value);

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
                    GetAttributesIFC4(t);
                    SetTypeObject(t, "IfcTypeObject");
                    t.LinkTree();
                    GetRelationTypesIFC4(t);
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
                    var tmpSchema = GetClassesIFC4x3();
                    GetAttributesIFC4x3(tmpSchema);
                    SetTypeObject(tmpSchema, "IfcTypeObject");
                    tmpSchema.LinkTree();
                    GetRelationTypesIFC4x3(tmpSchema);
                    schemaIfc4x3 = tmpSchema;
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

                    // We now need to expand to the types of the objects
                    var typeObjects = new List<string>();
                    foreach (var item in thisPsetTypes)
                    {
                        var tp = schema[item];
                        if (tp is not null && tp.RelationTypeClasses is not null)
                            typeObjects.AddRange(tp.RelationTypeClasses);
                    }
                    if (typeObjects.Any())
                        thisPsetTypes = thisPsetTypes.ToList().Concat(typeObjects.Distinct()).ToList();
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
                    GetAttributesIFC2x3(t);
                    SetTypeObject(t, "IfcTypeObject");
                    t.LinkTree();
                    GetRelationTypesIFC2x3(t);
                    schemaIfc2x3 = t;
                }
                return schemaIfc2x3;
            }
        }

        static partial void GetRelationTypesIFC2x3(SchemaInfo schema);
        static partial void GetRelationTypesIFC4(SchemaInfo schema);
        static partial void GetRelationTypesIFC4x3(SchemaInfo schema);

        internal void AddRelationType(string objClass, params string[] typeClasses)
        {
            var c = this[objClass];
            if (c is null)
            {
                // Debug.WriteLine($"Did not find {objClass} in {Version}");
                return;
            }
            List<string> found = new();
			foreach (var typeClass in typeClasses)
			{
				var tpC = this[typeClass];
                if (tpC != null)
                {
                    tpC.FunctionalType = FunctionalType.TypeOfElement;
                    found.Add(typeClass);
				}
			}
            if (found.Any())
            {
                // var t = $"\t\tschema.AddRelationType(\"{objClass}\", \"{found.First()}\");";
				// Debug.WriteLine(t);
				// Debug.WriteLine($"{objClass} {string.Join(",", found)}");
				c.AddTypeClasses(found);
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

        private void AddAttribute(string attributeName, string[] topClassNames, string[] allClassNames, string[]? valueTypes = null)
        {
            AttributesToAllClasses ??= new();
            AttributesToAllClasses.Add(attributeName, allClassNames);

            AttributesToTopClasses ??= new();
            AttributesToTopClasses.Add(attributeName, topClassNames);

            AttributesToValueTypes ??= new();
            if (valueTypes is null)
				AttributesToValueTypes.Add(attributeName, null);
            else
				AttributesToValueTypes.Add(attributeName, valueTypes);
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

		internal static bool TryGetSchemaInformation(IfcSchemaVersions schemas, out IEnumerable<SchemaInfo> schemaInfo)
		{
			var ret = new List<SchemaInfo>(GetSchemas(schemas));
			schemaInfo = ret;
			return ret.Any();
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

        /// <summary>
        /// Returns a list of the concrete class names that implement a given top class.
        /// When multiple schema flags are passed the list is the non-repeating union of the values of each schema
        /// </summary>
        /// <returns>A non null enumerable that might be empty if the topClass is not found or no schema is provided</returns>
        public static IEnumerable<string> GetConcreteClassesFrom(string topClass, IfcSchemaVersions schemaVersions)
        {
            if (schemaVersions.IsSingleSchema())
            {
				var schema = GetSchemas(IfcSchemaVersions.IfcAllVersions).First();
                if (schema is null)
                    return Enumerable.Empty<string>();
                var top = schema[topClass];
                if (top is null)
                    return Enumerable.Empty<string>();
				return top.MatchingConcreteClasses.Select(x=>x.Name);
			}

			var schemas = GetSchemas(IfcSchemaVersions.IfcAllVersions);
            List<string> ret = new();
			foreach (var schema in schemas)
            {
                var top = schema[topClass];
                if (top is null)
                    continue;
                ret = ret.Union(top.MatchingConcreteClasses.Select(x => x.Name)).ToList();
			}
            return ret;
        }

        /// <summary>
        /// the inner dictionary maps a type built as [TYPE]_[COUNT] to the topClass
        /// </summary>
        readonly private static Dictionary<IfcSchemaVersions, Dictionary<string, string>> inheritanceListCache = new();


        /// <summary>
        /// Attempts to identify a single top class inheritance from a list of class names
        /// </summary>
        /// <param name="concreteClassNames">the enumeration of class names must be uppercase</param>
        /// <param name="schemas">the schemas in which the concrete classes are found</param>
        /// <param name="topClass">The resulting name of the schema (PascalCase)</param>
        /// <returns>true if a single top class can be found, false otherwise</returns>
		public static bool TrySearchTopClass(IEnumerable<string> concreteClassNames, IfcSchemaVersions schemas, [NotNullWhen(true)] out string? topClass)
		{
            var sorted = concreteClassNames.OrderBy(x => x).ToList();
            if (sorted.Count == 0)
            {
                topClass = null;
                return false;
            }
			if (sorted.Count == 1)
			{
                var first = sorted.First();
				topClass = GetAllClassesFor(schemas).Where(x=>x.ToUpperInvariant() == first).FirstOrDefault();
				return topClass is not null;
			}

            if (!inheritanceListCache.TryGetValue(schemas, out var dic))
            {
				Dictionary<string, List<string>> tempHashes = new();
				foreach (var className in GetAllClassesFor(schemas))
				{
					var t = GetConcreteClassesFrom(className, schemas).OrderBy(x => x).ToList();
                    if (!t.Any())
                        continue;
					var hash = $"{t.First().ToUpper()}_{t.Count}";
					if (tempHashes.TryGetValue(hash, out var list))
					{
						list.Add(className);
					}
					else
					{
						tempHashes.Add(hash, new List<string>() { className });
					}
					// Debug.WriteLine($"{className}\t{t.First()}\t{t.Count}");
				}

				// now prepare the cahced dictionary
				dic = new Dictionary<string, string>();
				foreach (var pair in tempHashes)
                {
                    // skip the single ones
                    if (pair.Key.EndsWith("_1"))
                        continue;
                    if (pair.Value.Count > 1)
                    {
                        // there's more than one possible match, we have to establish which one is the closest to the list (i.e. drop the higher abstract class)
                        if (TryGetMostSpecific(pair.Value, schemas, out var specific))
                        {
							dic.Add(pair.Key, specific);
						}
                        else
                        {
                            // here it's unresolved, e.g. the case of IfcBuildingElement that has been renamed to IfcBuiltElement
                        }
                    }
                    else
                    {
                        // a single 
                        dic.Add(pair.Key, pair.Value.First());
                    }
                }
			}

			// search a match
			var seekHash = $"{sorted.First().ToUpper()}_{sorted.Count}";
            if (dic.TryGetValue(seekHash, out var candidateClassName))
            {
				// now we have a candidate, but we need to make sure that all the items match
				var schemaConcretes = GetConcreteClassesFrom(candidateClassName, schemas).OrderBy(x => x).Select(x=>x.ToUpperInvariant()).ToList();
                if (schemaConcretes.SequenceEqual(sorted))
                {
                    topClass = candidateClassName;
                    return true;
                }
			}
           
			topClass = null;
            return false;
		}


        private static bool TryGetMostSpecific(IEnumerable<string> classNames, IfcSchemaVersions schemas, [NotNullWhen(true)] out string? specific)
        {
            var schemaInfos = GetSchemas(schemas);
            List<string> parentNames = new();
            foreach (var schema in schemaInfos)
            {
                foreach (var className in classNames)
                {
                    var c = schema[className];
                    if (c == null)
                        continue;
                    var parentName = c.ParentName;
                    if (string.IsNullOrEmpty(parentName))
                        continue;
                    parentNames.Add(parentName);
                }
            }
            var left = classNames.Except(parentNames).ToList();
            if (left.Count == 1)
            {
                specific = left.First();
                return true;
            }
            specific = null;
            return false;
		}

		private static IEnumerable<string> GetAllClassesFor(IfcSchemaVersions versions)
		{
			var schemaInfos = GetSchemas(versions);
			var allSchemaClassNames = new List<string>();
			foreach (var schema in schemaInfos)
			{
                allSchemaClassNames = allSchemaClassNames.Union(schema.Select(x => x.Name)).ToList();
			}
            return allSchemaClassNames;
		}

        /// <summary>
        /// Returns a distinct enumerable of the backing types of the required attributes, given a set of attribut names
        /// </summary>
        /// <param name="attributeNames">The names of the attributes to evaluate</param>
        /// <returns>string names of the types found in the evaluation of the attributes</returns>
		public IEnumerable<string> GetAttributesTypes(IEnumerable<string> attributeNames)
		{
            List<string> possible = new List<string>();
            foreach(var attribute in attributeNames)
            {
                if (!AttributesToValueTypes.TryGetValue(attribute, out var types))
                    continue;
                if (types is null)
                    continue;
                possible.AddRange(types);
            }
            return possible.Distinct();
		}

	}
}
