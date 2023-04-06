using CommandLine;
using IdsLib;
using IdsTool;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace idsTool;

public partial class Program
{
    static public int Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
            .AddFilter("idsTool.Program", LogLevel.Information)
            .AddSimpleConsole(options =>
            {
                options.SingleLine = true;
            });
        });
        var writer = Console.Out;
        writer.WriteLine("=== ids-tool - utility tool for buildingSMART IDS files.");
        ILogger logger = loggerFactory.CreateLogger<Program>();
        var t = Parser.Default.ParseArguments<AuditOptions, ErrorCodeOptions>(args)
          .MapResult(
            (AuditOptions opts) => Audit.Run(opts, logger),
            (ErrorCodeOptions opts) => ErrorCodeOptions.Run(opts),
            errs => Audit.Status.InvalidOptionsError);
        if (!args.Any())
            writer.WriteLine("The syntax of the command is `ids-tool <verb> [options]` (i.e. verb is mandatory, options depend on the verb).");
        return (int)t;
    }


}
