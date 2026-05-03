# SchemaInfo class

Provides static methods to get the collection of classes in the published schemas.

```csharp
public class SchemaInfo : IEnumerable<ClassInfo>
```

## Public Members

| name | description |
| --- | --- |
| [SchemaInfo](SchemaInfo/SchemaInfo.md)(…) | Provides metadata on IFC schemas, to support the correct compilation of the XIDS |
| static [SchemaIfc2x3](SchemaInfo/SchemaIfc2x3.md) { get; } | Static property for the Ifc2x3 schema |
| static [SchemaIfc4](SchemaInfo/SchemaIfc4.md) { get; } | Static property for the Ifc4 schema |
| static [SchemaIfc4x3](SchemaInfo/SchemaIfc4x3.md) { get; } | Static property for the Ifc4 schema |
| [AllPartOfRelations](SchemaInfo/AllPartOfRelations.md) { get; } | The names of classes across all schemas. |
| [Item](SchemaInfo/Item.md) { get; } | Get the classinfo by name string. Fastest lookup is available on CamelCase, otherwise it will try a case insensitive search which is more expensive |
| [PropertySets](SchemaInfo/PropertySets.md) { get; } | Provides access to the property sets of the schema |
| [Version](SchemaInfo/Version.md) { get; } | The version of the schema represented in the info |
| [Add](SchemaInfo/Add.md)(…) | Add a new classInfo to the collection |
| [GetAttributeClasses](SchemaInfo/GetAttributeClasses.md)(…) | Returns names of the classes that have an attribute. /// See  for similar function with different return type. |
| [GetAttributeNames](SchemaInfo/GetAttributeNames.md)() | Returns all attribute names in the schema |
| [GetAttributeRelations](SchemaInfo/GetAttributeRelations.md)(…) | Provides information of classes that have an attribute and the form of the relation to it. See  for similar function with different return type. |
| [GetAttributesIfcTypes](SchemaInfo/GetAttributesIfcTypes.md)(…) | Returns a distinct enumerable of the IFC backing types of the required attributes, given a set of attribut names |
| [GetAttributesXsdTypes](SchemaInfo/GetAttributesXsdTypes.md)(…) | Returns a distinct enumerable of the backing types of the required attributes, given a set of attribut names |
| [GetAttributesXsdTypesEnum](SchemaInfo/GetAttributesXsdTypesEnum.md)(…) | Returns a distinct enumerable of the XSD backing types of the required attributes, given a set of attribut names |
| [GetEnumerator](SchemaInfo/GetEnumerator.md)() | The default enumerator for the schema returns the classes defined within |
| static [AllAttributes](SchemaInfo/AllAttributes.md) { get; } | The names of all attributes across all schemas. |
| static [AllConcreteClasses](SchemaInfo/AllConcreteClasses.md) { get; } | The names of all concrete classes across known IFC schemas |
| static [AllDataTypes](SchemaInfo/AllDataTypes.md) { get; } | The names of dataType classes across all schemas. |
| static [AllMeasureInformation](SchemaInfo/AllMeasureInformation.md) { get; } | A selection of all the measures available in [`AllDataTypes`](./SchemaInfo/AllDataTypes.md). |
| static [Ifc2x3SpecialEntityMaps](SchemaInfo/Ifc2x3SpecialEntityMaps.md) { get; } | Provides a list of IDS entity names that need to be remapped when dealing with IFC2x3 schema. See [`Ifc2x3EntityMappingInformation`](./Ifc2x3EntityMappingInformation.md) for more information on the mapping and its intended use. |
| static [StandardConversionUnits](SchemaInfo/StandardConversionUnits.md) { get; } | Some standard unit conversions found in IFC files, including those defined in the buildingSMART documentation |
| static [GetConcreteClassesFrom](SchemaInfo/GetConcreteClassesFrom.md)(…) | Returns a list of the concrete class names that implement a given top class. When multiple schema flags are passed the list is the non-repeating union of the values of each schema |
| static [GetMeasureInformation](SchemaInfo/GetMeasureInformation.md)(…) | A selection of measures available in relevant schemas[`AllDataTypes`](./SchemaInfo/AllDataTypes.md). |
| static [GetSchemas](SchemaInfo/GetSchemas.md)(…) | Returns the schema metadata information for the required versions. |
| static [TryGetMeasureInformation](SchemaInfo/TryGetMeasureInformation.md)(…) | Returns the IfcMeasureInformation for a given measure class name. |
| static [TryParseIfcDataType](SchemaInfo/TryParseIfcDataType.md)(…) | Attempts to convert a string value to an instance of the IfcMeasureInformation |
| static [TrySimplifyTopClasses](SchemaInfo/TrySimplifyTopClasses.md)(…) | Attempts to identify the minimal set of top-level classes that collectively cover all specified concrete class names within the given schema versions. |
| enum [ClassAttributeMode](SchemaInfo.ClassAttributeMode.md) | Relation that allows to connect an available attribute to an entity |
| struct [ClassRelationInfo](SchemaInfo.ClassRelationInfo.md) | A structure contianing information about the ways in which an attribute is related to a class |

## See Also

* class [ClassInfo](./ClassInfo.md)
* namespace [IdsLib.IfcSchema](../ids-lib.md)
* [SchemaInfo.cs](https://github.com/buildingSMART/IDS-Audit-tool/tree/main/ids-lib/IfcSchema/SchemaInfo.cs)

<!-- DO NOT EDIT: generated by xmldocmd for ids-lib.dll -->
