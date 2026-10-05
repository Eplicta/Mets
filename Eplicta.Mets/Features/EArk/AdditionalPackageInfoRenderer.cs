using System;
using System.Xml;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Renders the additionalPackageInfo.xml that Riksarkivet's application of E-ARK CSIP and SIP places in the
/// documentation folder.
/// </summary>
public class AdditionalPackageInfoRenderer
{
    private const string DateFormat = "yyyy-MM-dd";
    private readonly AdditionalPackageInfo _info;
    private readonly string _packageId;

    public AdditionalPackageInfoRenderer(AdditionalPackageInfo info, string packageId)
    {
        _info = info ?? throw new ArgumentNullException(nameof(info));
        _packageId = packageId;
    }

    public XmlDocument Render()
    {
        var document = new XmlDocument();
        document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", null));

        var root = document.CreateElement("additionalPackageInfo", EArkConstants.AdditionalPackageInfoNamespace);
        document.AppendChild(root);

        root.AppendChild(RenderControl(document));
        root.AppendChild(RenderPackage(document));

        return document;
    }

    private XmlElement RenderControl(XmlDocument document)
    {
        var control = document.CreateElement("control", EArkConstants.AdditionalPackageInfoNamespace);

        var identification = Append(control, "identification", _info.Identification.ToString());
        identification.SetAttribute("type", "UUID");

        Append(control, "status", _info.Status.ToString().ToUpperInvariant());
        Append(control, "creationDate", EArkRenderer.ToUtc(_info.CreationDate));

        var creator = document.CreateElement("creator", EArkConstants.AdditionalPackageInfoNamespace);
        Append(creator, "creatorName", _info.CreatorName);
        if (!string.IsNullOrEmpty(_info.CreatorCode))
        {
            var creatorCode = Append(creator, "creatorCode", _info.CreatorCode);
            creatorCode.SetAttribute("type", _info.CreatorCodeType.ToString());
        }

        AppendIfSet(creator, "creatorDesc", _info.CreatorDescription);
        control.AppendChild(creator);

        return control;
    }

    private XmlElement RenderPackage(XmlDocument document)
    {
        var package = document.CreateElement("package", EArkConstants.AdditionalPackageInfoNamespace);
        package.SetAttribute("id", _packageId);

        AppendIfSet(package, "archiveName", _info.ArchiveName);
        AppendFlagged(package, "disposal", "disposable", _info.Disposable, "disposalDesc", _info.DisposalDescription);
        AppendFlagged(package, "accessRestrict", "restricted", _info.AccessRestricted, "accessRestrictDesc", _info.AccessRestrictDescription);
        AppendFlagged(package, "useRestrict", "restricted", _info.UseRestricted, "useRestrictDesc", _info.UseRestrictDescription);
        if (_info.StartDate != null) Append(package, "startDate", _info.StartDate.Value.ToString(DateFormat));
        if (_info.EndDate != null) Append(package, "endDate", _info.EndDate.Value.ToString(DateFormat));
        AppendIfSet(package, "informationClass", _info.InformationClass);
        AppendIfSet(package, "securityClassification", _info.SecurityClassification);

        var audience = document.CreateElement("audience", EArkConstants.AdditionalPackageInfoNamespace);
        audience.SetAttribute("publishable", _info.Publishable ? "true" : "false");
        package.AppendChild(audience);

        return package;
    }

    private static XmlElement Append(XmlElement parent, string name, string value)
    {
        var element = parent.OwnerDocument!.CreateElement(name, EArkConstants.AdditionalPackageInfoNamespace);
        element.InnerText = value ?? string.Empty;
        parent.AppendChild(element);
        return element;
    }

    private static void AppendIfSet(XmlElement parent, string name, string value)
    {
        if (!string.IsNullOrEmpty(value)) Append(parent, name, value);
    }

    private static void AppendFlagged(XmlElement parent, string name, string flagName, bool flag, string descriptionName, string description)
    {
        var element = parent.OwnerDocument!.CreateElement(name, EArkConstants.AdditionalPackageInfoNamespace);
        element.SetAttribute(flagName, flag ? "true" : "false");
        AppendIfSet(element, descriptionName, description);
        parent.AppendChild(element);
    }
}
