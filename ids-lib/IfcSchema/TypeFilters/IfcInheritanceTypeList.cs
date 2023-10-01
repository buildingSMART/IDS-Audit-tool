using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IdsLib.IfcSchema.TypeFilters
{
    // todo: all IIfcTypeConstraint concrete classes need to covered thoroughly with unit tests

    [DebuggerDisplay("Types inheriting from {upperInvariantTopType} ({ConreteTypesCount})")]
    internal class IfcInheritanceTypeConstraint : IIfcTypeConstraint
	{
		private readonly string upperInvariantTopType;

		private readonly IfcSchemaVersions requiredSchemaVersions;

		private IEnumerable<string>? concreteTypes = null;

		internal int ConreteTypesCount => ConcreteTypes.Count();

        public IfcInheritanceTypeConstraint(string topType, IfcSchemaVersions requiredSchemaVersions)
		{
			this.upperInvariantTopType = topType.ToUpperInvariant();
			this.requiredSchemaVersions = requiredSchemaVersions;
		}

		/// <inheritdoc/>
		public IEnumerable<string> ConcreteTypes
		{
			get
			{
				if (concreteTypes is null) 
				{
					// this gets to the name of the top class (it must exist in all required schemas)
				    IfcConcreteTypeList? c = null;
				
					var schemas = SchemaInfo.GetSchemas(requiredSchemaVersions);
					// we identify the intersection of classes in all required schemas

					foreach (var schema in schemas)
					{
						if (c == null)
							c = IfcConcreteTypeList.FromTopClass(schema, upperInvariantTopType);
						else
							c.Intersect(IfcConcreteTypeList.FromTopClass(schema, upperInvariantTopType));
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

		/// <inheritdoc/>
		public bool IsEmpty => string.IsNullOrEmpty(upperInvariantTopType) || requiredSchemaVersions == IfcSchemaVersions.IfcNoVersion;

		/// <inheritdoc/>
		public IIfcTypeConstraint Intersect(IIfcTypeConstraint? other)
		{
			if (other is null)
				return this;
            if (this.IsEmpty || other.IsEmpty)
				return IfcConcreteTypeList.Empty;
			return new IfcConcreteTypeList(
				ConcreteTypes.Intersect(other.ConcreteTypes)
				);
		}

		/// <inheritdoc/>
		public IIfcTypeConstraint Union(IIfcTypeConstraint? other)
		{
			if (other is null)
				return this;
			if (other.IsEmpty)
				return this;
			if (this.IsEmpty)
				return other;
			return new IfcConcreteTypeList(
				ConcreteTypes.Union(other.ConcreteTypes)
				);
		}
	}


}
