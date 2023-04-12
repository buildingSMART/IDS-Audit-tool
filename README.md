# IDS-Tool

This repository contains a tool used for quality assurance of IDS files according to the buildingSMART standard.

It is comprised of two major components:

- a reusalble dll that can be embedded in other applications, and 
- a command line tool for direct usage.

The tool itself is a .NET console application tha returns errors and warnings
on the files, in order to verify their correctness.

## Usage

Executing `ids-tool help` provides the following guidance for available commands.

```
=== ids-tool - utility tool for buildingSMART IDS files.
ids-tool 1.0.10
Claudio Benghi

  audit        Audits for ids schemas and ids files.

  errorcode    provides description of tool's error code, useful when providing useer feedback in batch commands.

  help         Display more information on a specific command.

  version      Display version information.
```

`ids-tool help <command>` provides guidance on the options available for that command.

### the Audit command

Options for the `audit` verb are as follows:

```
  -x, --xsd            XSD schema(s) to load, this is useful when testing changes in the schema (e.g. GitHub repo). If
                       this is not specified, an embedded schema is adopted depending on the each ids's declaration of
                       version.

  -s, --schema         (Default: false) Check validity of the xsd schema(s) passed with the `xsd` option. This is useful
                       for the development of the schema and it is in use in the official repository for quality
                       assurance purposes.

  -e, --extension      (Default: ids) When passing a folder as source, this defines which files to audit by extension.

  -c, --omitContent    Skips the audit of the agreed limitation of IDS contents.

  --help               Display this help screen.

  --version            Display version information.

  source (pos. 0)      Input IDS to be processed; it can be a file or a folder.
```

## File Auditing Examples

Simple usage: `ids-tool audit path-to-some-file` or `ids-tool audit path-to-some-folder`.

If no option is specificed all available audits are performed on the IDS files.

## Audit Roadmap

We are planning to a number of audits (checked ones are implemented)

- [x] XSD Schema check
  - [x] Use Xsd from disk
  - [x] Use relevant Xsd from resource
- [ ] IFC Schema check (individual facets)
  - [x] IfcEntity
    - [x] Predefined types
      - [x] Predefined types Names are hardcoded in the library (Ifc2x3, Ifc4, Ifc4x3)
      - [x] Prefefined types are tested against values provided from the schema.
      - [x] Meaningful test cases
      - [ ] Should we handle User-defined behaviour as special case?
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
  - [ ] properties 
    - [x] Prepare metadata infrastructure
    - [ ] Is it correct that names are case sensitive?
    - [ ] no extensions of standard PSETs
      - [ ] properties are limited to the agreed set
    - [ ] No misplacement of properties
      - [ ] Property with a recognised name (e.g. IsExternal) should not be outside of the agreed pset.
        - [ ] Perhaps return a warning for userdefined property
        - [ ] Warning or error as configuration (strict)
    - [ ] Includes IFC type inheritance?
    - [ ] Reserved prefix
      - [ ] No custom pset can start with "PSET_" this prefix is reserved for the standard
  - [ ] Measures
    - [x] If a value is provided then it needs to be checked
    - [x] Test cases added
    - [ ] Discuss with IDS group: clarify the list of valid measures 
    - [ ] Discuss with IDS group: clarify the case sensitivity logic (currently PascalCase based on Development files)
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
- [ ] Type coherence audits
  - [ ] Extend with prohibited clauses (currently only dealing with required)
  - [x] Coherence between applicability and requirements
    - [x] Inconsistent types between applicability and requirements
  - [ ] Type constraining Facets
    - [x] PartOf (depending on relation)
    - [x] Entity
    - [ ] Property sets
      - [ ] determine type constraint by standard property sets
    - [ ] Material
      - [ ] Option: could be constraining types depending on implicit set or relations
    - [ ] Attribute
      - [ ] Option: could be defining type depending on attribute name 
    - [ ] Classification
      - [ ] Option: could be constraining types depending on implicit set or relations

## Feature Roadmap

- [ ] Implement Run with streams (as opposed to disk files)
