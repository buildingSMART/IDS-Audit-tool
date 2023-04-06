using CommandLine;
using System;
using static IdsLib.Audit;

namespace idsTool;

[Verb("errorcode", HelpText = "provides description of tool's error code, useful when providing useer feedback in batch commands.")]
internal class ErrorCodeOptions
{
    [Value(0,
        MetaName = "requestedCode",
        HelpText = "the error code number to describe.",
        Required = true)]
    public int RequestedCode { get; set; }

    internal static Status Run(ErrorCodeOptions opts)
    {
        try
        {
            Status s = (Status)opts.RequestedCode;
            var asString = s.ToString();
            if (asString == opts.RequestedCode.ToString())
            {
                Console.WriteLine($"The requested error code {opts.RequestedCode} appears meaningless.");
            }
            else
                Console.WriteLine(asString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing the requested error code {opts.RequestedCode}, {ex.Message}.");
        }

        return Status.Ok;
    }
}
