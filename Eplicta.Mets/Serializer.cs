using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets;

public static class Serializer
{
    public static DeserializedMets Deserialize(XmlDocument xmlDocument)
    {
        var xmlString = xmlDocument.OuterXml;

        return Deserialize(xmlString);
    }

    public static DeserializedMets Deserialize(string xmlString)
    {
        var serializer = new XmlSerializer(typeof(DeserializedMets));

        using TextReader reader = new StringReader(xmlString);
        var result = (DeserializedMets)serializer.Deserialize(reader);

        return result;
    }
}