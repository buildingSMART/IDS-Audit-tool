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

## Audit features

We are planning to implement a number of audits (the ones with a check-mark are completed)

- [x] XSD Schema check
  - [x] Use Xsd from disk
  - [x] Use relevant Xsd from resource
- [x] IFC Schema check (individual facets)
  - [x] IfcEntity
    - [x] Predefined types
      - [x] Predefined types Names are hardcoded in the library (Ifc2x3, Ifc4, Ifc4x3)
      - [x] Predefined types are tested against values provided from the schema.
      - [x] Meaningful test cases
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
  - [x] Properties
    - [x] Prepare metadata infrastructure
    - [x] Prop and PropertySet names are treated as case sensitive
    - [x] no extensions of standard `Psets`
      - [x] properties are limited to the agreed set
    - [x] Includes IFC type inheritance
    - [x] Reserved prefix 
      - [x] No custom PropertySet can mandate a property starting with "Pset_" this prefix is reserved for the standard
        - [x] SimpleValues
        - [x] Enumerations
    - [x] Property Measures
      - [x] If a value is provided, it is checked against a closed list
        - [ ] Discuss the scope of the list, currently only measures, IFCTEXT and IFCLABEL (but it's now datatype, so we should probably expand).
        - [ ] The attribute is required, but it can be empty, is that valid?
      - [x] Test cases added
      - [x] Constrain datatype for properties identified in standard Psets
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

- [ ] Single IDS audit
  - [x] Run with stream and version enum
    - [x] Schema is taken from embedded resource
    - [ ] Schema is taken from url
      - [ ] requires buildingSmart to ensure content is available online
  - [ ] Run with stream and no version
- [x] Batch IDS audits on files and folders
  - [x] Get schema dependent on IDS content
  - [x] Load custom schema from file
  - [x] Load schema depending on IDS declaration

## Matters to be addressed by the standard

- [ ] Property facet
  - [ ] No misplacement of properties (ON HOLD)
    - [ ] Property with a recognized name (e.g. IsExternal) should not be outside of the agreed PSet.
      - [ ] Options to discuss:
        - [ ] perhaps just consider this a warning for user-defined property sets
        - [ ] warning or error as configuration (strict mode?)
  - [ ] Reserved prefix 
    - [ ] can the regex match any string starting with "Pset_".
      - [ ] we have a working solution to be included, but it may raise the technical complexity for implementers considerably
      - [ ] Perhaps one for the strict mode?
  - [ ] Measures
    - [ ] Clarify the list of valid measures.
      - [ ] Only concrete measures? 
      - [ ] Do they follow inheritance?
    - [ ] Clarify the case sensitivity logic of measures (currently UPPERCASE based on Development files)  
  - [ ] Clarify Misplacement of properties
    - [ ] if a predefined property name has to be restricted to the predefined property set
    - [ ] Look at IfcPropertyNameCoherenceAcrossSchemas for list of times where this does not happen for the predefined property sets
- [ ] Entity facet
  - [ ] Should we handle User-defined behavior as special case?

## Testing

The solution has an automated test project for quality control that helps reduce code regressions.

Some of the tests are designed to audit the ids files available in the main [buildingSMART IDS repository](https://github.com/buildingSMART/IDS). 
This is useful for debugging and coherence checks against the main documentation site.
If such reposioty is not found under the same parent folder of this one, those tests are marked as `skipped`.

To enable them, clone the two repositories next to each other under an arbitrary parent folder:

- Arbitrary parent folder
  - [...]
  - IDS repository
  - IDS Audit tool repository
  - [...]
