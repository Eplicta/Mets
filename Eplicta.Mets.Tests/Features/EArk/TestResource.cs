using System.IO;
using System.Reflection;

namespace Eplicta.Mets.Tests.Features.EArk;

internal static class TestResource
{
    /// <summary>
    /// The METS document of IP_example_1, Riksarkivet's own published example of their application of E-ARK
    /// CSIP and SIP. Used as the reference the renderer's output is measured against.
    /// </summary>
    internal static string RiksarkivetExampleMets()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Eplicta.Mets.Tests.Resources.Riksarkivet_IP_example_1_METS.xml");
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }
}
