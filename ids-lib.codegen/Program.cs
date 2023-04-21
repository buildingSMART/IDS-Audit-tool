namespace IdsLib.codegen;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("Running code generation for ids-lib.");
        if (IdsRepo_Updater.UpdateRequiresRestart())
        {
            Message("Local code updated, need to restart the generation.", ConsoleColor.Yellow);
            return;
        }
        
        EvaluateContent(
            IfcSchema_ClassAndAttributeNamesGenerator.Execute(), 
            @"ids-lib\IfcSchema\SchemaInfo.ClassAndAttributeNames.g.cs"
            );

        EvaluateContent(
            IfcSchema_MeasureNamesGenerator.Execute(),
            @"ids-lib\IfcSchema\SchemaInfo.MeasureNames.g.cs");

        EvaluateContent(
            IfcSchema_ClassGenerator.Execute(),
            @"ids-lib\IfcSchema\SchemaInfo.Schemas.g.cs");

        EvaluateContent(
            IfcSchema_AttributesGenerator.Execute(),
            @"ids-lib\IfcSchema\SchemaInfo.Attributes.g.cs");

        EvaluateContent(
            IfcSchema_PartOfRelationGenerator.Execute(),
            @"ids-lib\IfcSchema\SchemaInfo.PartOfRelations.g.cs");        
        
        EvaluateContent(
            IfcSchema_PropertiesGenerator.Execute(),
            @"ids-lib\IfcSchema\SchemaInfo.Properties.g.cs");
        
        EvaluateContent(
            IdsTool_DocumentationUpdater.Execute(),
            @"ids-tool\README.md");
    }

    private static void EvaluateContent(string content, string solutionDestinationPath)
    {
        Console.Write($"Evaluating: {solutionDestinationPath}... ");
        var destinationPathFolder = new DirectoryInfo(@"..\..\..\..\");
        var destinationFullName = Path.Combine(destinationPathFolder.FullName, solutionDestinationPath);

        if (File.Exists(destinationFullName))
        {
            var current = File.ReadAllText(destinationFullName);
            if (content == current)
            {
                Message($"no change.", ConsoleColor.Green);
                return;
            }
        }

        File.WriteAllText(destinationFullName, content);
        Message($"updated.", ConsoleColor.DarkYellow);
    }

    private static void Message(string v, ConsoleColor messageColor)
    {
        var restore = Console.ForegroundColor;
        Console.ForegroundColor = messageColor;
        Console.WriteLine(v); 
        Console.ForegroundColor = restore;
    }

    static internal string[] schemas = new[] { "Ifc2x3", "Ifc4", "Ifc4x3" };
}