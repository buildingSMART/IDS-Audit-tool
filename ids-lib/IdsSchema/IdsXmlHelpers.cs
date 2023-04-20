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
    internal static BaseContext GetContextFromElement(XmlReader reader, BaseContext? parent, ILogger? logger)
    {
        return reader.LocalName switch
        {
            // ids
            "specification" => new IdsSpecification(reader, parent, logger),
            "requirements" => new IdsFacetCollection(reader, parent),
            "applicability" => new IdsFacetCollection(reader, parent),
            "simpleValue" => new IdsSimpleValue(reader, parent),
            "entity" => new IdsEntity(reader, parent),
            "attribute" => new IdsAttribute(reader, parent),
            "partOf" => new IdsPartOf(reader, parent),
            "property" => new IdsProperty(reader, parent),
            "classification" => new IdsFacet(reader, parent),
            "material" => new IdsFacet(reader, parent),
            // xs
            "restriction" => new XsRestriction(reader, parent),
            "enumeration" => new XsEnumeration(reader, parent),
            "totalDigits" => new XsTotalDigits(reader, parent),
            "pattern" => new XsPattern(reader, parent),
            "length" => new XsLength(reader, parent),
            // default
            _ => new BaseContext(reader, parent),
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
