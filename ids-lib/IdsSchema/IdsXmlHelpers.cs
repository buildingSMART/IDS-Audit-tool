using IdsLib.IdsSchema.IdsNodes;
using IdsLib.IdsSchema.XsNodes;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace IdsLib.IdsSchema;

internal class IdsXmlHelpers
{
    internal static BaseContext GetContextFromElement(XmlReader reader, ILogger? logger)
    {
        return reader.LocalName switch
        {
            // ids
            "specification" => new IdsSpecification(reader, logger),
            "requirements" => new IdsRequirements(reader),
            "simpleValue" => new IdsSimpleValue(reader),
            "entity" => new IdsEntity(reader),
            "attribute" => new IdsAttribute(reader),
            "partOf" => new IdsFacet(reader),
            "classification" => new IdsFacet(reader),
            "property" => new IdsProperty(reader),
            "material" => new IdsFacet(reader),
            // xs
            "restriction" => new XsRestriction(reader),
            "enumeration" => new XsEnumeration(reader),
            "totalDigits" => new XsTotalDigits(reader),
            "pattern" => new XsPattern(reader),
            "length" => new XsLength(reader),
            // default
            _ => new BaseContext(reader),
        };
    }

    public static async Task<IdsInformation> GetIdsInformationAsync(FileInfo fileInformation)
    {
        if (fileInformation is null)
            throw new ArgumentNullException(nameof(fileInformation));
        using var fs = fileInformation.OpenRead();
        return await GetIdsInformationAsync(fs);
    }

    private enum ElementName
    {
        undefined,
        ids,
    }

    public static async Task<IdsInformation> GetIdsInformationAsync(Stream stream)
    {
        var ret = new IdsInformation();
        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreWhitespace = true,
            IgnoreComments = true,
        };

        // First element has to be an IDS
        // var currentElement = elementName.undefined;

        using XmlReader reader = XmlReader.Create(stream, settings);
        try
        {
            while (await reader.ReadAsync())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (!ret.IsIds && reader.LocalName != "ids")
                            return IdsInformation.CreateInvalid("ids element not found in file.");

                        switch (reader.LocalName)
                        {
                            case "ids":
                                ret.IsIds = true;
                                //currentElement = elementName.ids;
                                ret.SchemaLocation = reader.GetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance") ?? string.Empty;
                                return ret;
                            default:
                                break;
                        }
                        Debug.WriteLine("Start Element {0}", reader.Name);
                        break;
                        //case XmlNodeType.Attribute:
                        //    Debug.WriteLine("Attribute Node: {0}", await reader.GetValueAsync());
                        //    break;
                        //case XmlNodeType.Text:
                        //    Debug.WriteLine("Text Node: {0}", await reader.GetValueAsync());
                        //    break;
                        //case XmlNodeType.EndElement:
                        //    Debug.WriteLine("End Element {0}", reader.Name);
                        //    break;
                        //default:
                        //    Debug.WriteLine("Other node {0} with value {1}", reader.NodeType, reader.Value);
                        //    break;
                }
            }
        }
        catch (Exception ex)
        {
            return IdsInformation.CreateInvalid(ex.Message);
        }
        return IdsInformation.CreateInvalid("No XML element found.");
    }
}
