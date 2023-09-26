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

		internal static IEnumerable<string> GetRelAsssignClasses(IfcSchemaVersions version)
        {
            if (version == IfcSchemaVersions.IfcNoVersion)
                return Enumerable.Empty<string>();
            // valid classes are the intersection of the schemas
            IEnumerable<string>? ret = null;
            if (version.HasFlag(IfcSchemaVersions.Ifc2x3))
                ret = Ifc2x3RelAssignClasses;
            if (version.HasFlag(IfcSchemaVersions.Ifc4))
            {
                if (ret == null)
                    ret = Ifc4RelAssignClasses;
                else
                    ret = ret.Intersect(Ifc4RelAssignClasses);
            }
            if (version.HasFlag(IfcSchemaVersions.Ifc4x3))
            {
                if (ret == null)
                    ret = Ifc4x3RelAssignClasses;
                else
                    ret = ret.Intersect(Ifc4x3RelAssignClasses);
            }
            if (ret == null)
                return Enumerable.Empty<string>();
            return ret;
        }

		internal static IEnumerable<string> GetRelAsssignClassificationClasses(IfcSchemaVersions version)
		{
			if (version == IfcSchemaVersions.IfcNoVersion)
				return Enumerable.Empty<string>();
			// valid classes are the intersection of the schemas
			IEnumerable<string>? ret = null;
			if (version.HasFlag(IfcSchemaVersions.Ifc2x3))
				ret = Ifc2x3RelAssignClasses;
			if (version.HasFlag(IfcSchemaVersions.Ifc4))
			{
				if (ret == null)
					ret = Ifc4RelAssignClassificationClasses;
				else
					ret = ret.Intersect(Ifc4RelAssignClassificationClasses);
			}
			if (version.HasFlag(IfcSchemaVersions.Ifc4x3))
			{
				if (ret == null)
					ret = Ifc4x3RelAssignClassificationClasses;
				else
					ret = ret.Intersect(Ifc4x3RelAssignClassificationClasses);
			}
			if (ret == null)
				return Enumerable.Empty<string>();
			return ret;
		}

	}
}
