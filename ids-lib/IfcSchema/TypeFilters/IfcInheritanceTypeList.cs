using System;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IfcSchema.TypeFilters
{
    // todo: all IIfcTypeConstraint concrete classes need to be thoroughly be covered with unit tests
    internal class IfcInheritanceTypeConstraint : IIfcTypeConstraint
	{
		private readonly string upperInvariantTopType;

		private readonly IfcSchemaVersions requiredSchemaVersions;

		private IEnumerable<string>? concreteTypes = null;

        public IfcInheritanceTypeConstraint(string topType, IfcSchemaVersions requiredSchemaVersions)
		{
			this.upperInvariantTopType = topType.ToUpperInvariant();
			this.requiredSchemaVersions = requiredSchemaVersions;
		}

		public IEnumerable<string> ConcreteTypes
		{
			get
			{
				if (concreteTypes == null) 
				{
					// this gets to the name of the top class (it must exist in all required schemas)
					var topClass = SchemaInfo.AllClasses.Where(x =>
							(x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions
							&&
							x.UpperCaseName == upperInvariantTopType
						).FirstOrDefault();
					var schemas = SchemaInfo.GetSchemas(requiredSchemaVersions);
					// we identify the intersection of classes in all required schemas

					IfcConcreteTypeList? c = null;
                    foreach (var schema in schemas)
					{
						if (c == null)
							c = IfcConcreteTypeList.FromTopClass(schema, topClass);
						else
							c.Intersect(IfcConcreteTypeList.FromTopClass(schema, topClass));
						if (c.IsEmpty)
						{
							break;
						}
                    }
					c ??= IfcConcreteTypeList.Empty;
                    concreteTypes = c.ConcreteTypes;
				}
				return concreteTypes;
			}
		}

		public bool IsEmpty => string.IsNullOrEmpty(upperInvariantTopType) || requiredSchemaVersions == IfcSchemaVersions.IfcNoVersion;

		public IIfcTypeConstraint Intersect(IIfcTypeConstraint other)
		{
            // todo: there's room form optimizing this method if the other is also of type IfcInheritanceTypeConstraint
            if (this.IsEmpty || other.IsEmpty)
				return IfcConcreteTypeList.Empty;
			return new IfcConcreteTypeList(
				this.ConcreteTypes.Intersect(other.ConcreteTypes)
				);
		}
	}


}
