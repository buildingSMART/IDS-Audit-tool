using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IfcSchema
{
    public partial class SchemaInfo
    {
        private static readonly object staticLocker = new();
        private static string[]? ifc2x3RelAssignClasses = null;
        private static string[]? ifc4RelAssignClasses = null;
        private static string[]? ifc4x3RelAssignClasses = null;

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
        private static string[] Ifc4RelAssignClasses
        {
            get
            {
                lock (staticLocker)
                {
                    ifc4RelAssignClasses ??=
                            SchemaIfc4["IFCOBJECTDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name).Union(
                            SchemaIfc4["IFCPROPERTYDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name)
                            ).ToArray();
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
                            SchemaIfc4x3["IFCOBJECTDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name).Union(
                            SchemaIfc4x3["IFCPROPERTYDEFINITION"]!.MatchingConcreteClasses.Select(x => x.Name)
                            ).ToArray();
                    return ifc4x3RelAssignClasses;
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

    }
}
