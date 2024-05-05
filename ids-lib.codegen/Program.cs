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
        var GeneratedContentChanged = false;

		GeneratedContentChanged = EvaluateContentChanged(
			IfcSchema_ObjectToTypeGenerator.Execute(),
			@"ids-lib\IfcSchema\SchemaInfo.ObjectTypes.g.cs") | GeneratedContentChanged;

		GeneratedContentChanged = EvaluateContentChanged(
            IfcSchema_ClassAndAttributeNamesGenerator.Execute(), 
            @"ids-lib\IfcSchema\SchemaInfo.ClassAndAttributeNames.g.cs") | GeneratedContentChanged;

        GeneratedContentChanged = EvaluateContentChanged(
            IfcSchema_DatatypeNamesGenerator.Execute(out var dataTypeDictionary),
            @"ids-lib\IfcSchema\SchemaInfo.MeasureNames.g.cs") | GeneratedContentChanged;

		GeneratedContentChanged = EvaluateContentChanged(
			IfcSchema_DocumentationGenerator.Execute(dataTypeDictionary),
			@"ids-lib.codegen\buildingSMART\DataTypes.md") | GeneratedContentChanged;

		GeneratedContentChanged = EvaluateContentChanged(
			XmlSchema_XsTypesGenerator.Execute(dataTypeDictionary),
			@"ids-lib\IdsSchema\XsNodes\XsTypes.g.cs") | GeneratedContentChanged;

		GeneratedContentChanged = EvaluateContentChanged(
            IfcSchema_ClassGenerator.Execute(),
            @"ids-lib\IfcSchema\SchemaInfo.Schemas.g.cs") | GeneratedContentChanged;

        GeneratedContentChanged = EvaluateContentChanged(
            IfcSchema_AttributesGenerator.Execute(dataTypeDictionary),
            @"ids-lib\IfcSchema\SchemaInfo.Attributes.g.cs") | GeneratedContentChanged;

        GeneratedContentChanged = EvaluateContentChanged(
            IfcSchema_PartOfRelationGenerator.Execute(),
            @"ids-lib\IfcSchema\SchemaInfo.PartOfRelations.g.cs") | GeneratedContentChanged;

        GeneratedContentChanged = EvaluateContentChanged(
            IfcSchema_PropertiesGenerator.Execute(),
            @"ids-lib\IfcSchema\SchemaInfo.Properties.g.cs") | GeneratedContentChanged;

        GeneratedContentChanged = EvaluateContentChanged(
            IdsTool_DocumentationUpdater.Execute(),
            @"ids-tool\README.md") | GeneratedContentChanged;

        if (GeneratedContentChanged)
        {
            Message("Generated code updated, need to restart the generation.", ConsoleColor.Yellow);
            return;
        }

        // documentation changes do not require restart of code generation
        IdsLib_DocumentationUpdater.Execute();
    }

    private static bool EvaluateContentChanged(string content, string solutionDestinationPath)
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
                return false;
            }
        }

        File.WriteAllText(destinationFullName, content);
        Message($"updated.", ConsoleColor.DarkYellow);
        return true;
    }

    internal static void Message(string v, ConsoleColor messageColor)
    {
        var restore = Console.ForegroundColor;
        Console.ForegroundColor = messageColor;
        Console.WriteLine(v); 
        Console.ForegroundColor = restore;
    }

    static internal string[] schemas { get; } = new[] { "Ifc2x3", "Ifc4", "Ifc4x3" };
}