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
        return Read("Eplicta.Mets.Tests.Resources.Riksarkivet_IP_example_1_METS.xml");
    }

    /// <summary>
    /// The additionalPackageInfo document of IP_example_1, Riksarkivet's own published example.
    /// </summary>
    internal static string RiksarkivetExampleAdditionalPackageInfo()
    {
        return Read("Eplicta.Mets.Tests.Resources.Riksarkivet_IP_example_1_additionalPackageInfo.xml");
    }

    private static string Read(string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }
}
