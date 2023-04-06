namespace IdsLib.IdsSchema.IdsNodes;

public enum IdsVersion
{
    Invalid,
    Ids0_5,
    Ids0_9,
    Ids1_0,
}

public class IdsInformation
{
    public bool IsIds { get; internal set; } = false;
    public string SchemaLocation { get; internal set; } = string.Empty;
    public string Message { get; internal set; } = string.Empty;
    public IdsVersion Version
    {
        get
        {
            return SchemaLocation switch
            {
                "http://standards.buildingsmart.org/IDS/ids_05.xsd" => IdsVersion.Ids0_5, // todo: this is invalid and needs to be fixed in the IDS repository
                "http://standards.buildingsmart.org/IDS  ids_09.xsd" => IdsVersion.Ids0_9, // todo: this is invalid and needs to be fixed in the IDS repository
                "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids_09.xsd" => IdsVersion.Ids0_9,
                "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids_1_0.xsd" => IdsVersion.Ids1_0,
                "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids.xsd" => IdsVersion.Ids0_9,
                _ => IdsVersion.Invalid,
            };
        }
    }

    internal static IdsInformation CreateInvalid(string InvalidMessage)
    {
        return new IdsInformation
        {
            SchemaLocation = IdsVersion.Invalid.ToString(),
            Message = InvalidMessage
        };
    }
}
