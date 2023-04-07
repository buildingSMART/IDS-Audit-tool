namespace IdsLib.codegen;

internal class Program
{
    static void Main()
    {
        var destPath = new DirectoryInfo(@"..\..\..\..\");
        Console.WriteLine("Running code generation for ids-lib.");
        if (IdsRepo_Updater.TryPromptUpdate())
        {
            Message("Local code updated, need to restart the generation.", ConsoleColor.Yellow);
            return;
        }

        
        var tmp = IfcSchema_ClassAndAttributeNamesGenerator.Execute();
        var dest = Path.Combine(destPath.FullName, @"ids-lib\IfcSchema\SchemaInfo.ClassAndAttributeNames.g.cs");
        File.WriteAllText(dest, tmp);

        tmp = IfcSchema_MeasureNamesGenerator.Execute();
        dest = Path.Combine(destPath.FullName, @"ids-lib\IfcSchema\SchemaInfo.MeasureNames.g.cs");
        File.WriteAllText(dest, tmp);

        tmp = IfcSchema_ClassGenerator.Execute();
        dest = Path.Combine(destPath.FullName, @"ids-lib\IfcSchema\SchemaInfo.Schemas.g.cs");
        File.WriteAllText(dest, tmp);

        tmp = IfcSchema_AttributesGenerator.Execute();
        dest = Path.Combine(destPath.FullName, @"ids-lib\IfcSchema\SchemaInfo.Attributes.g.cs");
        File.WriteAllText(dest, tmp);

        tmp = IfcSchema_PartOfRelationGenerator.Execute();
        dest = Path.Combine(destPath.FullName, @"ids-lib\IfcSchema\SchemaInfo.PartOfRelations.g.cs");
        File.WriteAllText(dest, tmp);
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