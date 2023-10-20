using IdsLib.IfcSchema.TypeFilters;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IfcSchema
{
    public partial class SchemaInfo
    {
        private static readonly object staticLocker = new();
        private static string[]? ifc2x3RelAssignClasses = null;
        private static string[]? ifc4RelAssignClasses = null;
        private static string[]? ifc4RelAssignClassificationClasses = null;
        private static string[]? ifc4x3RelAssignClasses = null;
        private static string[]? ifc4x3RelAssignClassificationClasses = null;

        private static string[] Ifc2x3RelAssignClasses
        {
            get
            {
                lock (staticLocker)
                {
                    ifc2x3RelAssignClasses ??= SchemaIfc2x3["IFCROOT"]!.MatchingConcreteClasses.Select(x=>x.Name).ToArray();
                    return ifc2x3RelAssignClasses;
                }
            }
        }

		private static string[] Ifc4RelAssignClassificationClasses
		{
			get
			{
				lock (staticLocker)
				{
					// we can also assign classification via IfcExternalReferenceRelationship
					// see https://standards.buildingsmart.org/IFC/RELEASE/IFC4/ADD2_TC1/HTML/schema/ifcexternalreferenceresource/lexical/ifcexternalreferencerelationship.htm
					ifc4RelAssignClassificationClasses ??=
							SchemaIfc4["IFCOBJECTDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name)
                            .Union(SchemaIfc4["IFCPROPERTYDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name))
							.Union(SchemaIfc4["IFCPROPERTYABSTRACTION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCPHYSICALQUANTITY"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCAPPLIEDVALUE"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCCONTEXTDEPENDENTUNIT"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCCONVERSIONBASEDUNIT"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCPROFILEDEF"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCACTORROLE"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCAPPROVAL"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCCONSTRAINT"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCTIMESERIES"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCMATERIALDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCPERSON"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCPERSONANDORGANIZATION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCORGANIZATION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCEXTERNALREFERENCE"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCEXTERNALINFORMATION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4["IFCSHAPEASPECT"]!.MatchingConcreteClasses.Select(x => x.Name))
							.Distinct()
                            .ToArray();
					return ifc4RelAssignClassificationClasses;
				}
			}
		}
		private static string[] Ifc4RelAssignClasses
        {
            get
            {
                lock (staticLocker)
                {
                    ifc4RelAssignClasses ??=
                            SchemaIfc4["IFCOBJECTDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name).
                            Union(SchemaIfc4["IFCPROPERTYDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .ToArray();
                    return ifc4RelAssignClasses;
                }
            }
        }
        private static string[] Ifc4x3RelAssignClasses
        {
            get
            {
                lock (staticLocker)
                {
                    ifc4x3RelAssignClasses ??=
                            SchemaIfc4x3["IFCOBJECTDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name)
                            .Union(SchemaIfc4x3["IFCPROPERTYDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .ToArray();
                    return ifc4x3RelAssignClasses;
                }
            }
        }
		private static string[] Ifc4x3RelAssignClassificationClasses
		{
			get
			{
				lock (staticLocker)
				{
					ifc4x3RelAssignClassificationClasses ??=
							SchemaIfc4x3["IFCOBJECTDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name)
							.Union(SchemaIfc4x3["IFCPROPERTYDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name))
							.Union(SchemaIfc4x3["IFCACTORROLE"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCAPPLIEDVALUE"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCAPPROVAL"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCCONSTRAINT"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCCONTEXTDEPENDENTUNIT"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCCONVERSIONBASEDUNIT"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCEXTERNALINFORMATION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCEXTERNALREFERENCE"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCMATERIALDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCORGANIZATION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCPERSON"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCPERSONANDORGANIZATION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCPHYSICALQUANTITY"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCPROFILEDEF"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCPROPERTYABSTRACTION"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCSHAPEASPECT"]!.MatchingConcreteClasses.Select(x => x.Name))
                            .Union(SchemaIfc4x3["IFCTIMESERIES"]!.MatchingConcreteClasses.Select(x => x.Name))
							.ToArray();
					return ifc4x3RelAssignClassificationClasses;
				}
			}
		}

        private static IIfcTypeConstraint? relAssignPropertyClassesIfc2x3 = null;
        private static IIfcTypeConstraint? relAssignPropertyClassesIfc4 = null;
        private static IIfcTypeConstraint? relAssignPropertyClassesIfc4x3 = null;

		internal IIfcTypeConstraint? GetRelAssignPropertyClasses
        {
            get
            {
                if (relAssignPropertyClassesIfc2x3 == null)
                {
                    // prepare the static values
                    relAssignPropertyClassesIfc2x3 = new IfcInheritanceTypeConstraint("IFCOBJECT", IfcSchemaVersions.Ifc2x3);
					relAssignPropertyClassesIfc2x3 = relAssignPropertyClassesIfc2x3.Union(new IfcInheritanceTypeConstraint("IFCMATERIAL", IfcSchemaVersions.Ifc2x3));

					relAssignPropertyClassesIfc4 = new IfcInheritanceTypeConstraint("IFCOBJECTDEFINITION", IfcSchemaVersions.Ifc4);
					relAssignPropertyClassesIfc4 = relAssignPropertyClassesIfc4.Union(new IfcInheritanceTypeConstraint("IFCMATERIALDEFINITION", IfcSchemaVersions.Ifc4));

					relAssignPropertyClassesIfc4x3 = new IfcInheritanceTypeConstraint("IFCOBJECTDEFINITION", IfcSchemaVersions.Ifc4x3);
					relAssignPropertyClassesIfc4x3 = relAssignPropertyClassesIfc4x3.Union(new IfcInheritanceTypeConstraint("IFCMATERIALDEFINITION", IfcSchemaVersions.Ifc4x3));
					relAssignPropertyClassesIfc4x3 = relAssignPropertyClassesIfc4x3.Union(new IfcInheritanceTypeConstraint("IFCPROFILEDEF", IfcSchemaVersions.Ifc4x3));
				}

				return Version switch
				{
					IfcSchemaVersions.Ifc2x3 => relAssignPropertyClassesIfc2x3,
					IfcSchemaVersions.Ifc4 => relAssignPropertyClassesIfc4,
					IfcSchemaVersions.Ifc4x3 => relAssignPropertyClassesIfc4x3,
					_ => null,
				};
			}
        }

		internal IEnumerable<string> GetRelAsssignClasses()
        {
            if (Version == IfcSchemaVersions.Ifc2x3)
                return Ifc2x3RelAssignClasses;
            if (Version == IfcSchemaVersions.Ifc4)
                return Ifc4RelAssignClasses;
            if (Version == IfcSchemaVersions.Ifc4x3)
                return Ifc4x3RelAssignClasses;
            return Enumerable.Empty<string>();
        }

		internal IEnumerable<string> GetRelAsssignClassificationClasses()
		{
            if (Version == IfcSchemaVersions.Ifc2x3)
                return Ifc2x3RelAssignClasses;
			if (Version == IfcSchemaVersions.Ifc4)
				return Ifc4RelAssignClassificationClasses;
			if (Version == IfcSchemaVersions.Ifc4x3)
				return Ifc4x3RelAssignClassificationClasses;
			return Enumerable.Empty<string>();
		}

	}
}
