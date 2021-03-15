using System.IO;
using System.Reflection;
using System.Xml;

namespace Eplicta.Mets.Tests
{
    public static class Resource
    {
        internal static XmlDocument Get(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Eplicta.Mets.Tests.Resources.{name}";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new StreamReader(stream);
            var result = reader.ReadToEnd();

            var xsd = new XmlDocument();
            xsd.LoadXml(result);
            return xsd;
        }
    }
}