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
| class [ClassInfo](./IdsLib.IfcSchema/ClassInfo.md) | Complext metedata container for properties of IFC classes |
| enum [ClassType](./IdsLib.IfcSchema/ClassType.md) | Information on the potential use of the class |
| class [DimensionalExponents](./IdsLib.IfcSchema/DimensionalExponents.md) | Supports conversion of measures from different forms of unit expression |
| enum [DimensionType](./IdsLib.IfcSchema/DimensionType.md) | One of the core SI units of measure |
| class [EnumerationPropertyType](./IdsLib.IfcSchema/EnumerationPropertyType.md) | Schema metadata for enumeration properties |
| enum [FunctionalType](./IdsLib.IfcSchema/FunctionalType.md) | the IFC classes we present can be classified with regards to their potential role in the IfcRelDefinesByType relation. |
| class [IfcAttributeInformation](./IdsLib.IfcSchema/IfcAttributeInformation.md) | Metadata container for attributes of entities in IfcSchema |
| class [IfcClassInformation](./IdsLib.IfcSchema/IfcClassInformation.md) | Simplistic metadata container for entities of an IfcSchema |
| class [IfcDataTypeInformation](./IdsLib.IfcSchema/IfcDataTypeInformation.md) | Metadata container for entities containing measures of an IfcSchema |
| struct [IfcMeasureInformation](./IdsLib.IfcSchema/IfcMeasureInformation.md) | Metadata about measure conversion behaviours. |
| class [IfcSchemaAttribute](./IdsLib.IfcSchema/IfcSchemaAttribute.md) | Metadata attribute to define if a value of [`IfcSchemaVersions`](./IdsLib.IfcSchema/IfcSchemaVersions.md) identifies a single version of the schema |
| [Flags] enum [IfcSchemaVersions](./IdsLib.IfcSchema/IfcSchemaVersions.md) | Enumerations for the identification of multiple schema versions. |
| static class [IfcSchemaVersionsExtensions](./IdsLib.IfcSchema/IfcSchemaVersionsExtensions.md) | Provides utility methods for the [`IfcSchemaVersions`](./IdsLib.IfcSchema/IfcSchemaVersions.md) enum. |
| interface [IPropertyTypeInfo](./IdsLib.IfcSchema/IPropertyTypeInfo.md) | Generalised metadata on IFC properties |
| static class [IPropertyTypeInfoExtensions](./IdsLib.IfcSchema/IPropertyTypeInfoExtensions.md) | Static class to contain extension method helpers for [`IPropertyTypeInfo`](./IdsLib.IfcSchema/IPropertyTypeInfo.md). |
| class [NamedPropertyType](./IdsLib.IfcSchema/NamedPropertyType.md) | Schema metadata for properties with name |
| class [PartOfRelationInformation](./IdsLib.IfcSchema/PartOfRelationInformation.md) | Metadata container for relations that are primarily one-to-many between IFC entities |
| class [PropertySetInfo](./IdsLib.IfcSchema/PropertySetInfo.md) | Information about standard property sets defined from bS |
| class [SchemaInfo](./IdsLib.IfcSchema/SchemaInfo.md) | Provides static methods to get the collection of classes in the published schemas. |
| class [SingleValuePropertyType](./IdsLib.IfcSchema/SingleValuePropertyType.md) | Schema metadata for single value properties |

## IdsLib.SchemaProviders namespace

| public type | description |
| --- | --- |
| class [FixedVersionSchemaProvider](./IdsLib.SchemaProviders/FixedVersionSchemaProvider.md) | A schema provider based on a specific recognised version |
| abstract class [SchemaProvider](./IdsLib.SchemaProviders/SchemaProvider.md) | Abstract class to provide methods to arbitrarily resolve xsd schemas |

<!-- DO NOT EDIT: generated by xmldocmd for ids-lib.dll -->
