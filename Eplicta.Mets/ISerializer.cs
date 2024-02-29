using System.Xml;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets;

public interface ISerializer
{
    DeserializedMets Deserialize(XmlDocument xmlDocument);
    DeserializedMets Deserialize(string xmlString);
}