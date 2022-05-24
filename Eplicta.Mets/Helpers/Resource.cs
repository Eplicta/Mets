using System.IO;
using System.Reflection;
using System.Xml;

namespace Eplicta.Mets.Helpers;

public static class Resource
{
    public static string Get(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Eplicta.Mets.Resources.{name}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();
        return result;
    }

    public static XmlDocument GetXml(string name)
    {
        var xsd = new XmlDocument();
        xsd.LoadXml(Get(name));
        return xsd;
    }
}