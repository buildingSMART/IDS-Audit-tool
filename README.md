# IDS-Audit-Tool

This repository contains software used for quality assurance of IDS files according to the buildingSMART standard.

It is comprised of two major components:

- an executable command-line tool for direct usage from a console, and
- a reusable library that can be embedded in other applications.

## Components

Both components are implemented using Microsoft .NET, and can be used under multiple operating systems.

### Executable console application

The tool itself is a .NET console application tha returns feedback on ids files that you want to test.

If you are a final user, [read the tool documentation](ids-tool/README.md).

### Library

If you are a developer, [read the library documentation](ids-lib/README.md).

## Audit Road-map

We are planning to implement a number of audits (the ones with a check-mark are completed)

- [x] XSD Schema check
  - [x] Use Xsd from disk
  - [x] Use relevant Xsd from resource
- [ ] IFC Schema check (individual facets)
  - [x] IfcEntity
    - [x] Predefined types
      - [x] Predefined types Names are hardcoded in the library (Ifc2x3, Ifc4, Ifc4x3)
      - [x] Predefined types are tested against values provided from the schema.
      - [x] Meaningful test cases
      - [ ] Question: Should we handle User-defined behavior as special case?
    - [x] IfcTypeNames
      - [x] IfcType Names are hardcoded in the library (Ifc2x3, Ifc4, Ifc4x3)
      - [x] Simple type names are audited
      - [x] More complex case of type match need to be audited
        - [x] Regex matches
        - [x] Multiple values allowed
    - [x] Attribute names
      - [x] Attribute Names are hardcoded in the library
      - [x] Simple attribute names are audited
      - [x] More complex case of type match need to be audited
        - [x] Regex matches
        - [x] Multiple values allowed
  - [ ] Properties
    - [x] Prepare metadata infrastructure
    - [x] Prop and PSet names are treated as case sensitive
    - [x] no extensions of standard PSets
      - [x] properties are limited to the agreed set
    - [ ] No misplacement of properties
      - [ ] Property with a recognized name (e.g. IsExternal) should not be outside of the agreed PSet.
        - [ ] Options to discuss:
          - [ ] perhaps just consider this a warning for user-defined property sets
          - [ ] warning or error as configuration (strict mode?)
    - [x] Includes IFC type inheritance
    - [ ] Reserved prefix (partial implementation)
      - [ ] No custom PSet can start with "PSet_" this prefix is reserved for the standard
        - [ ] If a PSet name does not match one of the valid names it triggers an error
          - [x] SimpleValues
          - [x] Enumerations
          - [ ] Regular expressions: (this is not trivial to implement, but we have a working solution to be included if considered appropriate)
            - [ ] can the regex match any string starting with "PSet_".
    - [ ] Property Measures
      - [x] If a value is provided then it is checked against a closed list
      - [x] Test cases added
      - [ ] Discuss with IDS group: clarify the list of valid measures.
      - [ ] Discuss with IDS group: clarify the case sensitivity logic (currently PascalCase based on Development files)
      - [ ] Further constrain IfcMeasure for identified properties in standard sets
  - [x] Cardinality for facets (in requirements)
    - [x] partOf
    - [x] classification
    - [x] property
    - [x] material  
  - [x] PartOf
    - [x] Entity
      - [x] Constrained to relation type
    - [x] Relation
- [x] Cardinality
  - [x] Min and Max values are intrinsically valid (xml constraints)
  - [x] Min and Max values are restricted to agreed patterns (IDS implementation agreement)
- [x] Type coherence audits
  - [x] Excludes prohibited and optional clauses
  - [x] Coherence between applicability and requirements
    - [x] Inconsistent types between applicability and requirements
  - [x] Explicit Type constraining Facets
    - [x] PartOf (depending on relation)
    - [x] Entity
    - [x] Property sets
      - [x] determine type constraint by standard property sets
    - [x] Attribute
      - [x] limit type depending on attribute name
  - [x] Implicit Type constraining Facets
    - [x] Material and Classification both require a relation inheriting from IfcRelAssociates
      - [x] for Ifc4x3 - IfcObjectDefinition and IfcPropertyDefinition
      - [x] for Ifc4 - IfcObjectDefinition and IfcPropertyDefinition
      - [x] for Ifc2x3 - IfcRoot

## APIs

- [x] Run with streams (as opposed to disk files)
  - [x] Get schema settings dependent on IDS version from streams
    - [ ] from url
      - [ ] would require buildingSmart to ensure service
    - [x] from library resources
    - [ ] need seekable stream to be able to restart reading after version is identified
      - [ ] Implement test with non seekable stream
  - [x] Isolate configuration for running audit on single entities (i.e. stream and file)
  - [x] test that Line and Position are available in all streams as well as fileStream.
  - [x] Added test cases
    - [x] Multiple files
    - [x] Seekable and non seekable network stream
