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

```

`ids-tool help <command>` provides guidance on the options available for that command.

### the Audit command

Options for the `audit` verb are as follows:

```

```

## File Auditing Examples

Simple usage: `ids-tool audit path-to-some-file` or `ids-tool audit path-to-some-folder`.

If no option is specified all available audits are performed on the IDS files.

## Github actions

An example of using this tool for automation purpose on github is available at https://github.com/CBenghi/IDS using the [Nuke build system](https://nuke.build/).

