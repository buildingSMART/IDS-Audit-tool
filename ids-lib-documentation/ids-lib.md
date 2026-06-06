# ids-lib assembly

## IdsLib namespace

| public type | description |
| --- | --- |
| enum [Action](./IdsLib/Action.md) | Types of controls that the tools performs, resulting from the evaluation of options provided |
| class [ActionCollection](./IdsLib/ActionCollection.md) | Provides an expressive way to present the collection of actions that the tool is performing |
| static class [Audit](./IdsLib/Audit.md) | Main static class for invoking the audit functions. If you wish to audit a single file, the best entry point is [`Run`](./IdsLib/Audit/Run.md). This method allows you to run audits on the provided stream. For more complex auditing scenarios (e.g. those used by the tool), some automation can be achieved with [`Run`](./IdsLib/Audit/Run.md). Both APIs provide a return value that can be interpreted to determine if errors have been found. For more detailed feedback on the specific location of issues encountered, you must pass an ILogger interface, and collect events. |
| class [AuditProcessOptions](./IdsLib/AuditProcessOptions.md) | Configuration parameters required within the inner loop of the audit. |
| interface [IBatchAuditOptions](./IdsLib/IBatchAuditOptions.md) | This interface contains the parameters to configure a complex execution of the audit in a batch scenario. |
| static class [LibraryInformation](./IdsLib/LibraryInformation.md) | General information on the assembly without reflection. This is useful for environments that do not allow to load information from the DLL dynamically (e.g. Blazor). |
| class [SingleAuditOptions](./IdsLib/SingleAuditOptions.md) | Configuration parameters needed to setup the audit of a single IDS. The [`IdsVersion`](./IdsLib/SingleAuditOptions/IdsVersion.md) property is currently required to avoid the need to seek the stream, then resume the audit once the version is detected from the content. Future versions will attempt to relax this requirement. Ensure that the properties of the base class [`AuditProcessOptions`](./IdsLib/AuditProcessOptions.md) are also populated. |

## IdsLib.IdsSchema.IdsNodes namespace

| public type | description |
| --- | --- |
| class [IdsFacts](./IdsLib.IdsSchema.IdsNodes/IdsFacts.md) | Collection of relevant IDS assumptions |
| class [IdsInformation](./IdsLib.IdsSchema.IdsNodes/IdsInformation.md) | Status information of an IDS source |
| enum [IdsVersion](./IdsLib.IdsSchema.IdsNodes/IdsVersion.md) | Enumeration to identify a single IDS version. |
| class [NodeIdentification](./IdsLib.IdsSchema.IdsNodes/NodeIdentification.md) | Provides a way to identify the element of the xml with line/poistion or relative index inside the IDS Instances of this class are passed as a parameter in the ILogger calls, and by default it presents the location by line and number. A custom implementation of ILogger allows you to cast the received state parameter to IReadOnlyList&lt;KeyValuePair&lt;string, object&gt;&gt; and receive instances of NodeIdentification as parameters, to access the precise identifier. |

## IdsLib.IdsSchema.XsNodes namespace

| public type | description |
| --- | --- |
| static class [XsTypes](./IdsLib.IdsSchema.XsNodes/XsTypes.md) | Utility class for XSD type management |

## IdsLib.IfcSchema namespace

| public type | description |
| --- | --- |
| class [ClassInfo](./IdsLib.IfcSchema/ClassInfo.md) | Metadata container for properties of IFC classes |
| enum [ClassType](./IdsLib.IfcSchema/ClassType.md) | Information on the potential use of the class |
| class [DimensionalExponents](./IdsLib.IfcSchema/DimensionalExponents.md) | Supports conversion of measures from different forms of unit expression |
| enum [DimensionType](./IdsLib.IfcSchema/DimensionType.md) | One of the core SI units of measure |
| class [EnumerationPropertyType](./IdsLib.IfcSchema/EnumerationPropertyType.md) | Schema metadata for enumeration properties |
| enum [FunctionalType](./IdsLib.IfcSchema/FunctionalType.md) | the IFC classes we present can be classified with regards to their potential role in the IfcRelDefinesByType relation. |
| class [Ifc2x3EntityMappingInformation](./IdsLib.IfcSchema/Ifc2x3EntityMappingInformation.md) | Mapping information for an entity mapping from an IFC4 class to the equivalent IFC2x3 class and type, as documented by buildingSMART in the ifc2x3-occurrence-type-mapping-table.md file. For example, the definition of an IDS applicability facet with entity `IfcFilter`, should result in the identification of all `IfcFlowTreatmentDevice` in the model that are associated with a type `IfcFilterType`. |
| class [IfcAttributeInformation](./IdsLib.IfcSchema/IfcAttributeInformation.md) | Metadata container for attributes of entities in IfcSchema |
| class [IfcClassInformation](./IdsLib.IfcSchema/IfcClassInformation.md) | Simplistic metadata container for entities of an IfcSchema |
| record [IfcConversionUnitInformation](./IdsLib.IfcSchema/IfcConversionUnitInformation.md) | Provides information to assemble conversion units, as per the documentation available on buidlingSMART's website. |
| class [IfcDataTypeInformation](./IdsLib.IfcSchema/IfcDataTypeInformation.md) | Metadata container for entities containing measures of an IfcSchema. Access the list from [`AllDataTypes`](./IdsLib.IfcSchema/SchemaInfo/AllDataTypes.md). |
| record [IfcMeasureInformation](./IdsLib.IfcSchema/IfcMeasureInformation.md) | Metadata about measure conversion behaviours. Get a list of the available measures and their metadata from [`AllMeasureInformation`](./IdsLib.IfcSchema/SchemaInfo/AllMeasureInformation.md). Otherwise get a list of the ones for a specific schema from [`GetMeasureInformation`](./IdsLib.IfcSchema/SchemaInfo/GetMeasureInformation.md). |
| class [IfcSchemaAttribute](./IdsLib.IfcSchema/IfcSchemaAttribute.md) | Metadata attribute to define if a value of [`IfcSchemaVersions`](./IdsLib.IfcSchema/IfcSchemaVersions.md) identifies a single version of the schema |
| [Flags] enum [IfcSchemaVersions](./IdsLib.IfcSchema/IfcSchemaVersions.md) | Enumerations for the identification of multiple schema versions. |
| static class [IfcSchemaVersionsExtensions](./IdsLib.IfcSchema/IfcSchemaVersionsExtensions.md) | Provides utility methods for the [`IfcSchemaVersions`](./IdsLib.IfcSchema/IfcSchemaVersions.md) enum. |
| interface [IPropertyTypeInfo](./IdsLib.IfcSchema/IPropertyTypeInfo.md) | Generalised metadata on IFC properties |
| static class [IPropertyTypeInfoExtensions](./IdsLib.IfcSchema/IPropertyTypeInfoExtensions.md) | Static class to contain extension method helpers for [`IPropertyTypeInfo`](./IdsLib.IfcSchema/IPropertyTypeInfo.md). |
| interface [IUnitInformation](./IdsLib.IfcSchema/IUnitInformation.md) | Interface for unit information, used to provide metadata about units and their conversion behaviours. |
| class [ListValuePropertyType](./IdsLib.IfcSchema/ListValuePropertyType.md) | Schema metadata for list value properties. For the purpose of IDS, these are functionally equivalent to single value properties but we want to be able to distinguish them in the schema to support any procedural code generation that may be needed for their IFC verification. |
| class [NamedPropertyType](./IdsLib.IfcSchema/NamedPropertyType.md) | Schema metadata for properties with name |
| class [PartOfRelationInformation](./IdsLib.IfcSchema/PartOfRelationInformation.md) | Metadata container for relations that are primarily one-to-many between IFC entities |
| class [PropertySetInfo](./IdsLib.IfcSchema/PropertySetInfo.md) | Information about standard property sets defined from bS |
| class [SchemaInfo](./IdsLib.IfcSchema/SchemaInfo.md) | Provides static methods to get the collection of classes in the published schemas. |
| class [SingleValuePropertyType](./IdsLib.IfcSchema/SingleValuePropertyType.md) | Schema metadata for single value properties |
| class [TableValuePropertyType](./IdsLib.IfcSchema/TableValuePropertyType.md) | Schema metadata for single value properties |

## IdsLib.IfcSchema.TypeFilters namespace

| public type | description |
| --- | --- |
| class [IfcInheritanceTypeConstraint](./IdsLib.IfcSchema.TypeFilters/IfcInheritanceTypeConstraint.md) | Represents a type constraint based on inheritance. It defines that a top type, and all subclass shat inherit from it, in the required schemas are considered valid. |
| class [IfcTypeConcreteListConstraint](./IdsLib.IfcSchema.TypeFilters/IfcTypeConcreteListConstraint.md) | Represents a type constraint defined by a specific list of concrete types. |
| static class [IfcTypeConstraint](./IdsLib.IfcSchema.TypeFilters/IfcTypeConstraint.md) | Helper class for the interpretation of nullable [`IIfcTypeConstraint`](./IdsLib.IfcSchema.TypeFilters/IIfcTypeConstraint.md) instances. |
| interface [IIfcTypeConstraint](./IdsLib.IfcSchema.TypeFilters/IIfcTypeConstraint.md) | Represents a constraint on the types of an entity, such as the allowed types for an attribute or the types of entities that can be related in a relationship. The constraint is defined as a set of concrete types, represented as upper invariant strings. The interface provides methods for intersecting and unioning constraints, as well as checking if a constraint is empty (i.e., has no allowed types). The standard empty constraint is the static [`Empty`](./IdsLib.IfcSchema.TypeFilters/IfcTypeConcreteListConstraint/Empty.md). |

## IdsLib.SchemaProviders namespace

| public type | description |
| --- | --- |
| class [FixedVersionSchemaProvider](./IdsLib.SchemaProviders/FixedVersionSchemaProvider.md) | A schema provider based on a specific recognised version |
| abstract class [SchemaProvider](./IdsLib.SchemaProviders/SchemaProvider.md) | Abstract class to provide methods to arbitrarily resolve xsd schemas |

<!-- DO NOT EDIT: generated by xmldocmd for ids-lib.dll -->
