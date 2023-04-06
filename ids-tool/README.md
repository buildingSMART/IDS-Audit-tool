# ids-tool.CommandLine

This tool provides access to the functions exported by the ids-lib for automation functions for buildingSMART IDS files.

You can use the pre-compiled version of this tool as a command line executable, or reference ids-lib for your own development.

Run the `ids-tool` executable with no parameters for instructions.

The tool uses a `verb + options` approach. Available verbs are:

- `audit` check files for issues.
- `errorcode` provides description of tool's error code.
- `help` Display more information on a specific command.
- `version` Display version information.

More help is available for each verb typing launching the tool with `help <verb>` syntax; e.g. `help audit`.

## Github actions

An example of using this tool for automation purpose on github is available at https://github.com/CBenghi/IDS using the [Nuke build system](https://nuke.build/).
