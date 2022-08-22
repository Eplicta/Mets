using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets;

public class Renderer
{
    private readonly ModsData _modsData;

    public Renderer(ModsData metsData)
    {
        _modsData = metsData;
    }

    public XmlDocument Render(DateTime? now = null)
    {
        now ??= DateTime.UtcNow;

        var doc = new XmlDocument();

        var documentType = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(documentType);

        var root = doc.CreateElement("mets");
        doc.AppendChild(root);
        root.SetAttribute("xmlns:mets", "http://www.loc.gov/METS/");
        root.SetAttribute("xmlns:mods", "http://www.loc.gov/mods/v3");
        root.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
        if (_modsData.Mods != null)
        {
            root.SetAttribute("OBJID", _modsData.Mods.ObjId);
        }

        root.SetAttribute("TYPE", "SIP");
        root.SetAttribute("PROFILE", "http://www.kb.se/namespace/mets/fgs/eARD_Paket_FGS-PUBL.xml");

        ModsRenderer(doc, root, now.Value);

        return doc;
    }

    private void ModsRenderer(XmlDocument doc, XmlElement root, DateTime now)
    {
        // dynamic info, the create date with accordance to ISO 8601
        var dateNow = now.ToString("O");

        //Creates the metsHdr tag where agents and RecordID's will be
        var metshdr = doc.CreateElement("metsHdr");
        root.AppendChild(metshdr);
        metshdr.SetAttribute("CREATEDATE", dateNow);

        if (_modsData.Agent != null)
        {
            var agentElement = doc.CreateElement("agent");
            agentElement.SetAttribute("ROLE", _modsData.Agent.Role.ToString().ToUpper());
            agentElement.SetAttribute("TYPE", _modsData.Agent.Type.ToString().ToUpper());

            metshdr.AppendChild(agentElement);

            var compName = doc.CreateElement("name");
            compName.InnerText = _modsData.Agent.Name;
            agentElement.AppendChild(compName);

            var note = doc.CreateElement("note");
            note.InnerText = _modsData.Agent.Note;
            agentElement.AppendChild(note);
        }

        //Static info of the company
        if (_modsData.Company != null)
        {
            var companyAgent = doc.CreateElement("agent");
            companyAgent.SetAttribute("ROLE", _modsData.Company.Role.ToString().ToUpper());
            companyAgent.SetAttribute("TYPE", _modsData.Company.Type.ToString().ToUpper());

            metshdr.AppendChild(companyAgent);

            var companyname = doc.CreateElement("name");
            companyname.InnerText = _modsData.Company.Name;
            companyAgent.AppendChild(companyname);

            var companynote = doc.CreateElement("note");
            companynote.InnerText = _modsData.Company.Note;
            companyAgent.AppendChild(companynote);
        }

        //software section
        var companysoftware = doc.CreateElement("agent");

        if (_modsData.Software != null)
        {
            companysoftware.SetAttribute("ROLE", _modsData.Software.Role.ToString().ToUpper());
            companysoftware.SetAttribute("TYPE", _modsData.Software.Type.ToString().ToUpper());
            companysoftware.SetAttribute("OTHERTYPE", _modsData.Software.OtherType.ToString().ToUpper());
            metshdr.AppendChild(companysoftware);

            var softwarename = doc.CreateElement("name");
            softwarename.InnerText = _modsData.Software.Name;
            companysoftware.AppendChild(softwarename);
        }

        if (_modsData.AltRecords != null && _modsData.AltRecords.Any())
        {
            foreach (var altRecord in _modsData.AltRecords)
            {
                var recordId1 = doc.CreateElement("altRecordID");
                recordId1.InnerText = altRecord.InnerText;
                recordId1.SetAttribute("TYPE", altRecord.Type.ToString().ToUpper());
                metshdr.AppendChild(recordId1);
            }
        }

        //start of dmdSec
        var dmdSec = doc.CreateElement("dmdSec");
        dmdSec.SetAttribute("ID", "ID1");
        root.AppendChild(dmdSec);

        var mdwrap = doc.CreateElement("mdWrap");
        mdwrap.SetAttribute("MDTYPE", "MODS");
        dmdSec.AppendChild(mdwrap);

        var xmldata = doc.CreateElement("xmlData");
        mdwrap.AppendChild(xmldata);

        //mods:mods
        if (_modsData.Mods != null)
        {
            var modsmods = doc.CreateElement("mods", "mods", "http://www.loc.gov/mods/v3");
            modsmods.SetAttribute("xmlns", _modsData.Mods.Xmlns);
            xmldata.AppendChild(modsmods);

            if (!string.IsNullOrEmpty(_modsData.Mods.Identifier))
            {
                var modsidentifier = doc.CreateElement("mods", "identifier", "http://www.loc.gov/mods/v3");
                modsidentifier.SetAttribute("type", "local");
                modsidentifier.InnerText = _modsData.Mods.Identifier;
                modsmods.AppendChild(modsidentifier);
            }

            var modslocation = doc.CreateElement("mods", "location", "http://www.loc.gov/mods/v3");
            modsmods.AppendChild(modslocation);

            //Allowed values: physicalLocation, shelfLocator or url
            if (_modsData.Mods.Url != null)
            {
                var modsurl = doc.CreateElement("mods", "url", "http://www.loc.gov/mods/v3");
                modsurl.InnerText = _modsData.Mods.Url.OriginalString;
                modslocation.AppendChild(modsurl);
            }

            //Allowed values: abstract, accessCondition, classification, extension, genre, identifier, language, location, name, note, originInfo, part, physicalDescription, recordInfo, relatedItem, subject, tableOfContents, targetAudience, titleInfo, typeOfResource
            var modsorigininfo = doc.CreateElement("mods", "originInfo", "http://www.loc.gov/mods/v3");
            modsmods.AppendChild(modsorigininfo);

            //Allowed values: place, publisher, dateIssued, dateCreated, dateCaptured, dateValid, dateModified, copyrightDate, dateOther, edition, issuance, frequency
            var modsDateIssued = doc.CreateElement("mods", "dateIssued", "http://www.loc.gov/mods/v3");
            modsDateIssued.SetAttribute("encoding", "w3cdtf");
            modsDateIssued.InnerText = _modsData.Mods.DateIssued.ToString("O");
            modsorigininfo.AppendChild(modsDateIssued);

            var modsaccesscondition = doc.CreateElement("mods", "accessCondition", "http://www.loc.gov/mods/v3");
            modsaccesscondition.InnerText = _modsData.Mods.AccessCondition;
            modsmods.AppendChild(modsaccesscondition);

            var modstitleinfo = doc.CreateElement("mods", "titleInfo", "http://www.loc.gov/mods/v3");
            modsmods.AppendChild(modstitleinfo);

            if (!string.IsNullOrEmpty(_modsData.Mods.ModsTitle))
            {
                var modstitle = doc.CreateElement("mods", "title", "http://www.loc.gov/mods/v3");
                modstitle.InnerText = _modsData.Mods.ModsTitle;
                modstitleinfo.AppendChild(modstitle);
            }

            var modsrelateditem = doc.CreateElement("mods", "relatedItem", "http://www.loc.gov/mods/v3");
            modsrelateditem.SetAttribute("type", "host");
            modsmods.AppendChild(modsrelateditem);

            if (_modsData.Mods.Uri != null)
            {
                var modsidentifier2 = doc.CreateElement("mods", "identifier", "http://www.loc.gov/mods/v3");
                modsidentifier2.SetAttribute("type", "uri");
                modsidentifier2.InnerText = _modsData.Mods.Uri.OriginalString;
                modsrelateditem.AppendChild(modsidentifier2);
            }

            if (!string.IsNullOrEmpty(_modsData.Mods.ModsTitleInfo))
            {
                var modsTitleInfo = doc.CreateElement("mods", "titleInfo", "http://www.loc.gov/mods/v3");
                modsrelateditem.AppendChild(modsTitleInfo);

                var modstitle2 = doc.CreateElement("mods", "title", "http://www.loc.gov/mods/v3");
                modstitle2.InnerText = _modsData.Mods.ModsTitleInfo;
                modsTitleInfo.AppendChild(modstitle2);
            }
        }

        //From heres are the file section

        var filesec = doc.CreateElement("fileSec");
        root.AppendChild(filesec);

        var filegrp = doc.CreateElement("fileGrp");
        filesec.AppendChild(filegrp);

        if (_modsData.Files != null && _modsData.Files.Any())
        {
            foreach (var item in _modsData.Files)
            {
                var file = doc.CreateElement("file");
                file.SetAttribute("ID", item.Id);
                file.SetAttribute("USE", item.Use);
                file.SetAttribute("MIMETYPE", item.MimeType);
                file.SetAttribute("SIZE", item.Size.ToString());
                file.SetAttribute("CREATED", item.Created.ToString("O"));
                if (!string.IsNullOrEmpty(item.Checksum))
                {
                    file.SetAttribute("CHECKSUM", item.Checksum);
                    file.SetAttribute("CHECKSUMTYPE", item.ChecksumType.ToString().ToUpper().Replace("_", "-"));
                }

                var flocat = doc.CreateElement("FLocat");
                //flocat.SetAttribute("ns2:type", item.Ns2Type);
                //if (!string.IsNullOrEmpty(item.Ns2Href))
                //{
                //    flocat.SetAttribute("ns2:href", item.Ns2Href);
                //}
                flocat.SetAttribute("LOCTYPE", item.LocType.ToString().ToUpper());
                flocat.SetAttribute("href", "http://www.w3.org/1999/xlink", $"file:///{item.FileName}");
                flocat.SetAttribute("type", "simple");

                file.AppendChild(flocat);

                filegrp.AppendChild(file);
            }

            var struktmap = doc.CreateElement("structMap");
            struktmap.SetAttribute("TYPE", "physical");
            root.AppendChild(struktmap);

            var div = doc.CreateElement("div");
            div.SetAttribute("TYPE", "files");
            struktmap.AppendChild(div);

            var div2 = doc.CreateElement("div");
            div2.SetAttribute("TYPE", "publication");
            div.AppendChild(div2);

            foreach (var item in _modsData.Files)
            {
                var fptr = doc.CreateElement("fptr");
                fptr.SetAttribute("FILEID", item.Id);
                div2.AppendChild(fptr);
            }
        }
    }

    //private void AppendOriginInfo(XmlDocument doc, XmlElement root)
    //{
    //    var originInfo = doc.CreateElement("originInfo");
    //    root.AppendChild(originInfo);
    //    originInfo.SetAttribute("eventType", "publication");

    //    //var place = doc.CreateElement("place");
    //    //originInfo.AppendChild(place);
    //    //var placeTerm = doc.CreateElement("placeTerm");
    //    //place.AppendChild(placeTerm);
    //    //placeTerm.SetAttribute("authority", "marccountry");
    //    //placeTerm.SetAttribute("type", "code");
    //    //placeTerm.SetAttribute("authorityURI", "http://id.loc.gov/vocabulary/countries");
    //    //placeTerm.SetAttribute("valueURI", "http://id.loc.gov/vocabulary/countries/nyu");
    //    //placeTerm.InnerText = "nyu";

    //    //var placeA = doc.CreateElement("place");
    //    //originInfo.AppendChild(placeA);
    //    //var placeTermA = doc.CreateElement("placeTerm");
    //    //placeA.AppendChild(placeTermA);
    //    //placeTermA.InnerText = "Ithaca, N.Y";
    //    //placeTermA.SetAttribute("type", "text");

    //    var publisher = doc.CreateElement("publisher");
    //    originInfo.AppendChild(publisher);
    //    publisher.InnerText = _modsData.Creator;

    //    //var dateIssued = doc.CreateElement("dateIssued");
    //    //originInfo.AppendChild(dateIssued);
    //    //dateIssued.InnerText = "c1999";

    //    //var dateIssuedX = doc.CreateElement("dateIssued");
    //    //originInfo.AppendChild(dateIssuedX);
    //    //dateIssuedX.InnerText = "1999";
    //    //dateIssuedX.SetAttribute("encoding", "marc");

    //    //var issuance = doc.CreateElement("issuance");
    //    //originInfo.AppendChild(issuance);
    //    //issuance.InnerText = "monographic";
    //}

    //private static void AppendGenre(XmlDocument doc, XmlElement root)
    //{
    //    var genre = doc.CreateElement("genre");
    //    root.AppendChild(genre);
    //    genre.InnerText = "bibliography";
    //    genre.SetAttribute("authority", "marcgt");
    //}

    //private static void AppendTypeOfResource(XmlDocument doc, XmlElement root)
    //{
    //    var typeOfResource = doc.CreateElement("typeOfResource");
    //    root.AppendChild(typeOfResource);
    //    typeOfResource.InnerText = "text";
    //}

    //private void AppendName(XmlDocument doc, XmlElement root)
    //{
    //    var name = doc.CreateElement("name");
    //    root.AppendChild(name);

    //    name.SetAttribute("type", "personal");
    //    name.SetAttribute("authorityURI", "http://id.loc.gov/authorities/names");
    //    name.SetAttribute("valueURI", "http://id.loc.gov/authorities/names/n92101908");

    //    if (!string.IsNullOrEmpty(_modsData.Name?.NamePart))
    //    {
    //        var title = doc.CreateElement("namePart");
    //        name.AppendChild(title);
    //        title.InnerText = _modsData.Name.NamePart;

    //        var role = doc.CreateElement("role");
    //        name.AppendChild(role);

    //        var roleTerm = doc.CreateElement("roleTerm");
    //        role.AppendChild(roleTerm);
    //        roleTerm.InnerText = "creator";
    //        roleTerm.SetAttribute("type", "text");
    //    }
    //}

    //private void AppendTitleInfo(XmlDocument doc, XmlElement root)
    //{
    //    var titleInfo = doc.CreateElement("titleInfo");
    //    root.AppendChild(titleInfo);

    //    if (!string.IsNullOrEmpty(_modsData.TitleInfo?.Title))
    //    {
    //        var title = doc.CreateElement("title");
    //        titleInfo.AppendChild(title);
    //        title.InnerText = _modsData.TitleInfo.Title;
    //    }

    //    if (!string.IsNullOrEmpty(_modsData.TitleInfo?.SubTitle))
    //    {
    //        var title = doc.CreateElement("subTitle");
    //        titleInfo.AppendChild(title);
    //        title.InnerText = _modsData.TitleInfo.SubTitle;
    //    }
    //}

    public MemoryStream GetArchiveStream(string name = null)
    {
        using var compressedFileStream = new MemoryStream();
        using var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, false);

        AddFile(zipArchive, name ?? "metadata.xml", Render().OuterXml);

        if (_modsData.Files != null)
        {
            foreach (var resource in _modsData.Files)
            {
                AddFile(zipArchive, $"{resource.FileName}", resource.Data);
            }
        }

        return compressedFileStream;
    }

    private static void AddFile(ZipArchive zipArchive, string entryName, string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        AddFile(zipArchive, entryName, bytes);
    }

    private static void AddFile(ZipArchive zipArchive, string entryName, byte[] data)
    {
        var stream = new MemoryStream(data);
        AddFile(zipArchive, entryName, stream);
    }

    private static void AddFile(ZipArchive zipArchive, string entryName, MemoryStream stream)
    {
        var entry = zipArchive.CreateEntry(entryName);
        using var originalFileStream = stream;
        using var zipEntryStream = entry.Open();
        originalFileStream.CopyTo(zipEntryStream);
    }
}