using System;
using System.Xml;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets;

public class Parser
{
    public (MetsData MetsData, DateTime CreateTime) GetMetsData(string xmlString)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xmlString);

        var namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("mets", "http://www.loc.gov/METS/");

        var createTime = GetDateTime(doc, namespaceManager);

        //TODO: Unpack all data
        var metsData = new MetsData
        {
            Attributes = []
        };

        return (metsData, createTime);
    }

    private static DateTime GetDateTime(XmlDocument document, XmlNamespaceManager namespaceManager)
    {
        var metsHdrNode = document.DocumentElement?.SelectSingleNode("//mets:metsHdr", namespaceManager) as XmlElement;
        var createdDate = metsHdrNode?.GetAttribute("CREATEDATE");

        if (!DateTime.TryParse(createdDate, out var created))
        {
            throw new InvalidOperationException($"Cannot parse CREATEDATE {createdDate} to {nameof(DateTime)}.");
        }

        return created;
    }
}