using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Helpers;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.IO;
using static Eplicta.Mets.Entities.MetsData;

namespace Eplicta.Mets;

public class Renderer
{
    private static readonly RecyclableMemoryStreamManager _recyclableMsManager = new();
    private readonly MetsData _metsData;
    private const string MetsNs = "http://www.loc.gov/METS/";
    private const string ModsNs = "http://www.loc.gov/mods/v3";
    private const string Xlink = "http://www.w3.org/1999/xlink";
    private const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";
    private const string Ext = "ExtensionMETS";

    public Renderer(MetsData metsData)
    {
        _metsData = metsData;
    }

    public XmlDocument Render(DateTime? now = null, MetsSchema schema = null)
    {
        schema ??= MetsSchema.Default;
        now ??= DateTime.UtcNow; //"yyyy-MM-ddTHH:mm:ssZ"

        var doc = new XmlDocument();

        var documentType = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(documentType);

        var root = doc.CreateElement("mets", "mets", MetsNs); //TODO:mmm testa
        doc.AppendChild(root);
        if (schema.Name != "CSPackageMETS.xsd") root.SetAttribute("xmlns", MetsNs);
        if (_metsData.Mods != null) root.SetAttribute("xmlns:mods", ModsNs); //TODO:mmm testa
        root.SetAttribute("xmlns:xlink", Xlink);
        root.SetAttribute("OBJID", null);

        if (schema.Name == "CSPackageMETS.xsd") //TODO:mmm testa
        {
            root.SetAttribute("xmlns:xsi", Xsi);
            root.SetAttribute("xmlns:ext", "ExtensionMETS");

            var schemaLoc = doc.CreateAttribute("xsi", "schemaLocation", Xsi);
            schemaLoc.Value = "http://www.loc.gov/METS/ http://xml.ra.se/e-arkiv/METS/CSPackageMETS.xsd " + "ExtensionMETS http://xml.ra.se/e-arkiv/METS/CSPackageExtensionMETS.xsd";
            root.Attributes.Append(schemaLoc);

            var archName = doc.CreateAttribute("ext", "ARCHIVALNAME", Ext);
            archName.Value = "E-Arkiv";
            root.Attributes.Append(archName);

            var appraisal = doc.CreateAttribute("ext", "APPRAISAL", Ext);
            appraisal.Value = "No";
            root.Attributes.Append(appraisal);

            var accessRestrict = doc.CreateAttribute("ext", "ACCESSRESTRICT", Ext);
            accessRestrict.Value = "PuL";
            root.Attributes.Append(accessRestrict);

            root.SetAttribute("TYPE", "ERMS");
        }
        else
        {
            root.SetAttribute("TYPE", "SIP");
        }

        if (schema.Name != "mets.xsd")
        {
            var profileUrl = schema.Name != "CSPackageMETS.xsd" ? "http://www.kb.se/namespace/mets/fgs/eARD_Paket_FGS-PUBL.xml" : "http://xml.ra.se/e-arkiv/METS/CommonSpecificationSwedenPackageProfile.xml";
            root.SetAttribute("PROFILE", profileUrl); //TODO:mmm testa
        }

        if (_metsData.Attributes != null && _metsData.Attributes.Length != 0)
        {
            foreach (var attribute in _metsData.Attributes)
            {
                root.SetAttribute(attribute.Name.ToString().ToUpper(), attribute.Value);
            }
        }

        ModsRenderer(doc, root, now.Value, schema);

        return doc;
    }

    private void ModsRenderer(XmlDocument doc, XmlElement root, DateTime now, MetsSchema schema)
    {
        // dynamic info, the date of creation with accordance to ISO 8601
        var dateNow = now.ToString("yyyy-MM-ddTHH:mm:ssZ");

        //Creates the metsHdr tag where agents and RecordID's will be
        var metshdr = doc.CreateElement("mets", "metsHdr", MetsNs); //TODO:mmm testa
        root.AppendChild(metshdr);
        metshdr.SetAttribute("CREATEDATE", dateNow);

        if (schema.Name == "CSPackageMETS.xsd")
        {
            metshdr.SetAttribute("RECORDSTATUS", "NEW");

            var oaisStatus = doc.CreateAttribute("ext", "OAISSTATUS", Ext);
            oaisStatus.Value = "SIP";
            metshdr.Attributes.Append(oaisStatus);
        }

        if (_metsData.MetsHdr?.Attributes != null)
        {
            foreach (var attribute in _metsData.MetsHdr.Attributes)
            {
                metshdr.SetAttribute(attribute.Name.ToString().ToUpper(), attribute.Value);
            }
        }

        foreach (var agent in _metsData.Agents ?? [])
        {
            var agentElement = doc.CreateElement("mets", "agent", MetsNs);
            agentElement.SetAttribute("ROLE", agent.Role.ToString().ToUpper());
            agentElement.SetAttribute("TYPE", agent.Type.ToString().ToUpper());

            metshdr.AppendChild(agentElement);

            var compName = doc.CreateElement("mets", "name", MetsNs);
            compName.InnerText = agent.Name;
            agentElement.AppendChild(compName);

            if (agent.Note != null)
            {
                var note = doc.CreateElement("mets", "note", MetsNs);
                note.InnerText = agent.Note;
                agentElement.AppendChild(note);
            }
        }

        //software section
        var companySoftware = doc.CreateElement("mets", "agent", MetsNs);

        if (_metsData.Software != null)
        {
            companySoftware.SetAttribute("ROLE", _metsData.Software.Role.ToString().ToUpper());
            companySoftware.SetAttribute("TYPE", _metsData.Software.Type.ToString().ToUpper());
            companySoftware.SetAttribute("OTHERTYPE", _metsData.Software.OtherType.ToString().ToUpper());
            metshdr.AppendChild(companySoftware);

            var softwareName = doc.CreateElement("mets", "name", MetsNs);
            softwareName.InnerText = _metsData.Software.Name;
            companySoftware.AppendChild(softwareName);

            if (_metsData.Software.Note != null)
            {
                var softwareNote = doc.CreateElement("mets", "note", MetsNs);
                softwareNote.InnerText = _metsData.Software.Note;
                companySoftware.AppendChild(softwareNote);
            }
        }

        if (_metsData.AltRecords != null && _metsData.AltRecords.Any())
        {
            foreach (var altRecord in _metsData.AltRecords)
            {
                var recordId1 = doc.CreateElement("mets", "altRecordID", MetsNs);
                recordId1.InnerText = altRecord.InnerText;
                if (altRecord.Type != null) recordId1.SetAttribute("TYPE", altRecord.Type?.ToString().ToUpper());
                metshdr.AppendChild(recordId1);
            }
        }

        var recordId2 = doc.CreateElement("mets", "altRecordID", MetsNs);
        recordId2.InnerText = "OK";
        recordId2.SetAttribute("TYPE", nameof(EAltRecordType.SubmissionAgreement).ToUpper());
        metshdr.AppendChild(recordId2);

        var recordId3 = doc.CreateElement("mets", "altRecordID", MetsNs);
        recordId3.InnerText = "SIP";
        metshdr.AppendChild(recordId3);

        var recordId4 = doc.CreateElement("mets", "altRecordID", MetsNs);
        recordId4.InnerText = "sip.xml";
        metshdr.AppendChild(recordId4);

        if (_metsData.Mods != null)
        {
            //start of dmdSec
            var dmdSec = doc.CreateElement("mets", "dmdSec", MetsNs);
            dmdSec.SetAttribute("ID", "ID1");
            root.AppendChild(dmdSec);

            var mdwrap = doc.CreateElement("mets", "mdWrap", MetsNs);
            mdwrap.SetAttribute("MDTYPE", "MODS");
            dmdSec.AppendChild(mdwrap);

            var xmldata = doc.CreateElement("mets", "xmlData", MetsNs);
            mdwrap.AppendChild(xmldata);

            var modsmods = doc.CreateElement("mods", "mods", ModsNs);
            xmldata.AppendChild(modsmods);

            if (!string.IsNullOrEmpty(_metsData.Mods.Identifier))
            {
                var modsidentifier = doc.CreateElement("mods", "identifier", ModsNs);
                modsidentifier.SetAttribute("type", "local");
                modsidentifier.InnerText = _metsData.Mods.Identifier;
                modsmods.AppendChild(modsidentifier);
            }

            var modslocation = doc.CreateElement("mods", "location", ModsNs);
            modsmods.AppendChild(modslocation);

            //Allowed values: physicalLocation, shelfLocator or url
            if (_metsData.Mods.Url != null)
            {
                var modsurl = doc.CreateElement("mods", "url", ModsNs);
                modsurl.InnerText = _metsData.Mods.Url.OriginalString;
                modslocation.AppendChild(modsurl);
            }

            //Allowed values: abstract, accessCondition, classification, extension, genre, identifier, language, location, name, note, originInfo, part, physicalDescription, recordInfo, relatedItem, subject, tableOfContents, targetAudience, titleInfo, typeOfResource
            var modsorigininfo = doc.CreateElement("mods", "originInfo", ModsNs);
            modsmods.AppendChild(modsorigininfo);

            //Allowed values: place, publisher, dateIssued, dateCreated, dateCaptured, dateValid, dateModified, copyrightDate, dateOther, edition, issuance, frequency
            var modsDateIssued = doc.CreateElement("mods", "dateIssued", ModsNs);
            modsDateIssued.SetAttribute("encoding", "w3cdtf");
            modsDateIssued.InnerText = _metsData.Mods.DateIssued.ToString("yyyy-MM-ddTHH:mm:ssZ");
            modsorigininfo.AppendChild(modsDateIssued);

            if (!string.IsNullOrEmpty(_metsData.Mods.Publisher))
            {
                var modstitle = doc.CreateElement("mods", "publisher", ModsNs);
                modstitle.InnerText = _metsData.Mods.Publisher;
                modsorigininfo.AppendChild(modstitle);
            }

            if (_metsData.Mods.Place != null)
            {
                var place = doc.CreateElement("mods", "place", ModsNs);
                var placeTerm = doc.CreateElement("mods", "placeTerm", ModsNs);
                placeTerm.InnerText = _metsData.Mods.Place.PlaceTerm;
                place.AppendChild(placeTerm);
                modsorigininfo.AppendChild(place);
            }

            if (!string.IsNullOrEmpty(_metsData.Mods.AccessCondition))
            {
                var modsaccesscondition = doc.CreateElement("mods", "accessCondition", ModsNs);
                modsaccesscondition.InnerText = _metsData.Mods.AccessCondition;
                modsmods.AppendChild(modsaccesscondition);
            }

            var modstitleinfo = doc.CreateElement("mods", "titleInfo", ModsNs);
            modsmods.AppendChild(modstitleinfo);

            if (!string.IsNullOrEmpty(_metsData.Mods.ModsTitle))
            {
                var modstitle = doc.CreateElement("mods", "title", ModsNs);
                modstitle.InnerText = _metsData.Mods.ModsTitle;
                modstitleinfo.AppendChild(modstitle);
            }

            var modsrelateditem = doc.CreateElement("mods", "relatedItem", ModsNs);
            modsrelateditem.SetAttribute("type", "host");
            modsmods.AppendChild(modsrelateditem);

            if (_metsData.Mods.Uri != null)
            {
                var modsidentifier2 = doc.CreateElement("mods", "identifier", ModsNs);
                modsidentifier2.SetAttribute("type", "uri");
                modsidentifier2.InnerText = _metsData.Mods.Uri.OriginalString;
                modsrelateditem.AppendChild(modsidentifier2);
            }

            if (!string.IsNullOrEmpty(_metsData.Mods.ModsTitleInfo))
            {
                var modsTitleInfo = doc.CreateElement("mods", "titleInfo", ModsNs);
                modsrelateditem.AppendChild(modsTitleInfo);

                var modstitle2 = doc.CreateElement("mods", "title", ModsNs);
                modstitle2.InnerText = _metsData.Mods.ModsTitleInfo;
                modsTitleInfo.AppendChild(modstitle2);
            }

            if (_metsData.Mods.Notes != null && _metsData.Mods.Notes.Length > 0)
            {
                foreach (var note in _metsData.Mods.Notes)
                {
                    var noteType = Regex.Replace(note.Type.ToString(), "([A-Z])", " $1").Trim().ToLower();

                    var noteNode = doc.CreateElement("mods", "note", ModsNs);
                    noteNode.SetAttribute("type", noteType);
                    noteNode.InnerText = note.InnerText;

                    if (!string.IsNullOrEmpty(note.Href))
                    {
                        noteNode.SetAttribute("href", Xlink, note.Href);
                    }

                    modsmods.AppendChild(noteNode);
                }
            }
        }

        //From heres are the file section

        var filesec = doc.CreateElement("mets", "fileSec", MetsNs);
        root.AppendChild(filesec);

        var filegrp = doc.CreateElement("mets", "fileGrp", MetsNs);
        filegrp.SetAttribute("USE", "FILES"); //TODO:mmm testa
        filesec.AppendChild(filegrp);

        if (_metsData.Files?.Any() == false && _metsData.Sources?.Any() == false)
        {
            var structMap = doc.CreateElement("mets", "structMap", MetsNs);
            structMap.SetAttribute("LABEL", "No structmap defined in this information package");
            root.AppendChild(structMap);

            var div = doc.CreateElement("div");
            structMap.AppendChild(div);
        }
        else
        {
            foreach (var item in _metsData.Files ?? [])
            {
                var file = doc.CreateElement("mets", "file", MetsNs);
                file.SetAttribute("ID", item.Id);
                file.SetAttribute("USE", item.Use);
                file.SetAttribute("MIMETYPE", item.MimeType);
                file.SetAttribute("SIZE", item.Size.ToString());
                file.SetAttribute("CREATED", dateNow);
                if (!string.IsNullOrEmpty(item.Checksum))
                {
                    file.SetAttribute("CHECKSUM", item.Checksum);
                    file.SetAttribute("CHECKSUMTYPE", item.ChecksumType.ToString().ToUpper().Replace("_", "-"));
                }

                var flocat = doc.CreateElement("mets", "FLocat", MetsNs);
                flocat.SetAttribute("LOCTYPE", item.LocType.ToString().ToUpper());

                var href = schema.Name == "eARD_Paket_FGS-PUBL_mets.xsd" ? $"file:{item.FileName}" : $"file:///{item.FileName}";
                flocat.SetAttribute("href", Xlink, href);
                flocat.SetAttribute("type", Xlink, "simple");

                file.AppendChild(flocat);

                filegrp.AppendChild(file);
            }

            foreach (var item in _metsData.Sources ?? [])
            {
                var file = doc.CreateElement("mets", "file", MetsNs);
                file.SetAttribute("ID", item.Id);
                file.SetAttribute("USE", item.Use);
                file.SetAttribute("MIMETYPE", item.MimeType);
                file.SetAttribute("SIZE", item.Size.ToString());
                file.SetAttribute("CREATED", dateNow);
                if (!string.IsNullOrEmpty(item.Checksum))
                {
                    file.SetAttribute("CHECKSUM", item.Checksum);
                    file.SetAttribute("CHECKSUMTYPE", item.ChecksumType.ToString().ToUpper().Replace("_", "-"));
                }

                var flocat = doc.CreateElement("mets", "FLocat", MetsNs);
                flocat.SetAttribute("LOCTYPE", nameof(ELocType.Url).ToUpper());

                var href = schema.Name == "eARD_Paket_FGS-PUBL_mets.xsd" ? $"file:{item.Name}" : $"file:///{item.Name}";
                flocat.SetAttribute("href", Xlink, href);
                flocat.SetAttribute("type", Xlink, "simple");

                file.AppendChild(flocat);

                filegrp.AppendChild(file);
            }

            var structMap = doc.CreateElement("mets", "structMap", MetsNs);
            structMap.SetAttribute("TYPE", "Package");
            root.AppendChild(structMap);

            var div2 = doc.CreateElement("mets", "div", MetsNs);
            div2.SetAttribute("TYPE", "DataFiles");
            structMap.AppendChild(div2);

            foreach (var item in _metsData.Files ?? [])
            {
                var fptr = doc.CreateElement("mets", "fptr", MetsNs);
                fptr.SetAttribute("FILEID", item.Id);
                div2.AppendChild(fptr);
            }

            foreach (var item in _metsData.Sources ?? [])
            {
                var fptr = doc.CreateElement("mets", "fptr", MetsNs);
                fptr.SetAttribute("FILEID", item.Id);
                div2.AppendChild(fptr);
            }
        }
    }

    public ArchiveStream GetArchiveStream(ArchiveFormat archiveFormat, string metsFileName = null, bool prettify = false, MetsSchema schema = null)
    {
        schema ??= MetsSchema.Default;
        ArchiveStream archive;
        switch (archiveFormat)
        {
            case ArchiveFormat.Zip:
                archive = GetZipArchiveStream(metsFileName, prettify, schema);
                break;
            case ArchiveFormat.Tar:
                archive = GetTarArchiveStream(metsFileName, prettify, schema);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(archiveFormat), archiveFormat, null);
        }

        archive.Stream.Seek(0, SeekOrigin.Begin);
        return archive;
    }

    private ArchiveStream GetZipArchiveStream(string metsFileName, bool prettify, MetsSchema schema)
    {
        var compressedFileStream = _recyclableMsManager.GetStream();
        using var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, true);
        var archiveStream = new ArchiveStream(compressedFileStream, null, zipArchive);

        foreach (var source in _metsData.Sources ?? [])
        {
            using var md5 = MD5.Create();
            var entry = zipArchive.CreateEntry(source.Name);

            using (var zipEntryStream = entry.Open())

            using (var hashingStream = new CryptoStream(zipEntryStream, md5, CryptoStreamMode.Write))
            {
                source.Stream.CopyTo(hashingStream);
            }

            source.Size ??= source.Stream.Length;

            var hash = BitConverter.ToString(md5.Hash!).Replace("-", "");
            if (string.IsNullOrEmpty(source.Id))
            {
                source.Id = $"ID{hash}";
            }

            if (string.IsNullOrEmpty(source.Checksum))
            {
                source.Checksum = hash;
                source.ChecksumType = EChecksumType.MD5;
            }
        }

        foreach (var resource in _metsData.Files ?? [])
        {
            AddFile(zipArchive, $"{resource.FileName}", resource.Data);
        }

        var xmlString = Render(null, schema).OuterXml;

        AddFile(zipArchive, metsFileName ?? "metadata.xml", prettify ? PrettifyXml(xmlString) : xmlString);

        return archiveStream;
    }

    private ArchiveStream GetTarArchiveStream(string metsFileName = null, bool prettify = false, MetsSchema schema = null)
    {
        var compressedFileStream = _recyclableMsManager.GetStream();
        var tarOutputStream = new TarOutputStream(compressedFileStream, Encoding.UTF8);
        var archiveStream = new ArchiveStream(compressedFileStream, tarOutputStream);

        foreach (var source in _metsData.Sources ?? [])
        {
            throw new NotImplementedException("Stream resources from sources has not yet been implemented for Tar-archives.");
        }

        foreach (var resource in _metsData.Files ?? [])
        {
            AddFile(tarOutputStream, $"{resource.FileName}", resource.Data);
        }

        var xmlString = Render(null, schema).OuterXml;

        AddFile(tarOutputStream, metsFileName ?? "metadata.xml", prettify ? PrettifyXml(xmlString) : xmlString);

        tarOutputStream.Finish();

        return archiveStream;
    }

    private static void AddFile(ZipArchive zipArchive, string entryName, string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        AddFile(zipArchive, entryName, bytes);
    }

    private static void AddFile(ZipArchive zipArchive, string entryName, byte[] data)
    {
        using var stream = new MemoryStream(data);
        AddFile(zipArchive, entryName, stream);
    }

    private static void AddFile(ZipArchive zipArchive, string entryName, MemoryStream stream)
    {
        var entry = zipArchive.CreateEntry(entryName);
        using var originalFileStream = stream;
        using var zipEntryStream = entry.Open();
        originalFileStream.CopyTo(zipEntryStream);
    }

    private static void AddFile(TarOutputStream tarStream, string entryName, string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        AddFile(tarStream, entryName, bytes);
    }

    private static void AddFile(TarOutputStream tarStream, string entryName, byte[] data)
    {
        var tarEntry = new TarEntry(new TarHeader
        {
            Size = data.Length,
            Name = entryName
        });

        tarStream.PutNextEntry(tarEntry);
        tarStream.Write(data, 0, data.Length);
        tarStream.CloseEntry();
    }

    private static string PrettifyXml(string xmlString)
    {
        var xmlDocument = new XmlDocument();

        xmlDocument.LoadXml(xmlString);

        var stringWriter = new Utf8StringWriter(new StringBuilder());
        var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented };
        xmlDocument.Save(xmlTextWriter);

        return stringWriter.ToString();
    }
}