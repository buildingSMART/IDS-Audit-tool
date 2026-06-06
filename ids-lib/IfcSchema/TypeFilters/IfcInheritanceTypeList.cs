using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IdsLib.IfcSchema.TypeFilters
{

	/// <summary>
	/// Represents a type constraint based on inheritance. 
	/// It defines that a top type, and all subclass shat inherit from it, in the required schemas are considered valid.
	/// </summary>
	[DebuggerDisplay("Types inheriting from {upperInvariantTopType} ({ConreteTypesCount})")]
    public class IfcInheritanceTypeConstraint : IIfcTypeConstraint
	{
		private readonly string upperInvariantTopType;

		private readonly IfcSchemaVersions requiredSchemaVersions;

		private IEnumerable<string>? concreteTypes = null;

		internal int ConreteTypesCount => ConcreteTypes.Count();

		/// <summary>
		/// Default constructor
		/// </summary>
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
				    IfcTypeConcreteListConstraint? c = null;
				
					var schemas = SchemaInfo.GetSchemas(requiredSchemaVersions);
					// we identify the intersection of classes in all required schemas

					foreach (var schema in schemas)
					{
						if (c == null)
							c = IfcTypeConcreteListConstraint.FromTopClass(schema, upperInvariantTopType);
						else
							c.Intersect(IfcTypeConcreteListConstraint.FromTopClass(schema, upperInvariantTopType));
						if (c.IsEmpty)
						{
							break;
						}
					}
					c ??= IfcTypeConcreteListConstraint.Empty;
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
				return IfcTypeConcreteListConstraint.Empty;
			if (other is IfcInheritanceTypeConstraint otherInheritance)
			{
				return GetLowest(otherInheritance);
			}
			return new IfcTypeConcreteListConstraint(
				ConcreteTypes.Intersect(other.ConcreteTypes)
				);
		}

		/// <summary>
		/// Returns this, if they are the same.
		/// Returns the highest of the two, if one inherits from the other;
		/// Otherwise returns empty.
		/// </summary>
		private IIfcTypeConstraint GetHighest(IfcInheritanceTypeConstraint otherInheritance)
		{
			if (otherInheritance.GetInheritanceList().Contains(upperInvariantTopType))
				return this;
			if (GetInheritanceList().Contains(otherInheritance.upperInvariantTopType))
				return otherInheritance;
			return IfcTypeConcreteListConstraint.Empty;
		}

		/// <summary>
		/// Returns this, if they are the same.
		/// Returns the lowest of the two, if one inherits from the other;
		/// Otherwise returns empty.
		/// </summary>
		private IIfcTypeConstraint GetLowest(IfcInheritanceTypeConstraint otherInheritance)
		{
			if (GetInheritanceList().Contains(otherInheritance.upperInvariantTopType))
				return this;
			if (otherInheritance.GetInheritanceList().Contains(upperInvariantTopType))
				return otherInheritance;
			return IfcTypeConcreteListConstraint.Empty;
		}

		/// <summary>
		/// Gets the list of all types from this one upwards
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		private IEnumerable<string> GetInheritanceList()
		{
			var schema = SchemaInfo.GetSchemas(requiredSchemaVersions).FirstOrDefault();
			if (schema is null)
				yield break;
			var tp = schema[upperInvariantTopType];
			while (tp != null)
			{
				yield return tp.Name.ToUpperInvariant();
				tp = tp.Parent;
			}
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
			if (other is IfcInheritanceTypeConstraint otherInheritance)
			{
				return GetHighest(otherInheritance);
			}
			return new IfcTypeConcreteListConstraint(
				ConcreteTypes.Union(other.ConcreteTypes)
				);
		}
	}

}
