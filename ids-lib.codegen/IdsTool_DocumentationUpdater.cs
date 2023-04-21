using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.codegen;

internal class IdsTool_DocumentationUpdater
{
    static public string Execute()
    {
        var stub = File.ReadAllText("documentation/ids-tool-README.md");
        stub = stub.Replace("$help$", IdsRepo_Updater.ExecuteCommandLine("help"));
        stub = stub.Replace("$audithelp$", IdsRepo_Updater.ExecuteCommandLine("help audit"));
        return stub;
    }


}
