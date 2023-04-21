# ids-tool

This tool references the ids-lib, providing automation functions for buildingSMART IDS files.

You can use the compiled version of this tool as a command line executable, or reference ids-lib for your own development.

Run the `ids-tool` executable with no parameters for instructions.

The tool uses a `verb + options` approach. For instance, verbs are:

- `ids-tool audit` check files for issues.
- `ids-tool errorcode` provides description of tool's error code.
- `ids-tool help` displays help information on the tool or a specific command.
- `ids-tool version` displays version information.

More help is available for each verb typing launching the tool with `ids-tool help <verb>` syntax; e.g. `ids-tool help audit`.

## Usage

Executing `ids-tool help` provides the following guidance for available commands.

```
  audit        Audits ids files and/or their xsd schema.

  errorcode    provides description of tool's error code, useful when providing
               useer feedback in batch commands.

  help         Display more information on a specific command.

  version      Display version information.
```

`ids-tool help <command>` provides guidance on the options available for that command.

### the Audit command

Options for the `audit` verb are as follows:

```
  -x, --xsd                        XSD schema(s) to load, this is useful when
                                   testing changes in the schema (e.g. GitHub
                                   repo). If this is not specified, an embedded
                                   schema is adopted depending on the each ids's
                                   declaration of version.

  -s, --schema                     (Default: false) Check validity of the xsd
                                   schema(s) passed with the `xsd` option. This
                                   is useful for the development of the schema
                                   and it is in use in the official repository
                                   for quality assurance purposes.

  -e, --extension                  (Default: ids) When passing a folder as
                                   source, this defines which files to audit by
                                   extension.

  -c, --omitContent                Skips the audit of the agreed limitation of
                                   IDS contents.

  -p, --omitContentAuditPattern    (Default: ) Regex applied to file name to
                                   omit the audit of the semantic aspects of the
                                   IDS.

  --help                           Display this help screen.

  --version                        Display version information.

  source (pos. 0)                  Input IDS to be processed; it can be a file
                                   or a folder.
```

## File Auditing Examples

Simple usage: `ids-tool audit path-to-some-file` or `ids-tool audit path-to-some-folder`.

If no option is specified all available audits are performed on the IDS files.

## Github actions

An example of using this tool for automation purpose on github is available at https://github.com/CBenghi/IDS using the [Nuke build system](https://nuke.build/).

