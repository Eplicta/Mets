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

public class Renderer
{
    private readonly MetsData _metsData;
    private static readonly RecyclableMemoryStreamManager _recyclableMsManager = new();
    public Renderer(MetsData metsData)
    {
        _metsData = metsData;
    }

    public XmlDocument Render(DateTime? now = null, MetsSchema schema = null)
    {
        schema ??= MetsSchema.Default;
        now ??= DateTime.UtcNow;

        var doc = new XmlDocument();

        var documentType = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(documentType);

        var root = doc.CreateElement("mets");
        doc.AppendChild(root);
        root.SetAttribute("xmlns", "http://www.loc.gov/METS/");
        root.SetAttribute("xmlns:mods", "http://www.loc.gov/mods/v3");
        root.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
        root.SetAttribute("OBJID", null);

        root.SetAttribute("TYPE", "SIP");
        root.SetAttribute("PROFILE", _metsData.MetsProfile);

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
        var dateNow = now.ToString("O");

        //Creates the metsHdr tag where agents and RecordID's will be
        var metshdr = doc.CreateElement("metsHdr");
        root.AppendChild(metshdr);
        metshdr.SetAttribute("CREATEDATE", dateNow);

        if (_metsData.MetsHdr?.Attributes != null)
        {
            foreach (var attribute in _metsData.MetsHdr.Attributes)
            {
                metshdr.SetAttribute(attribute.Name.ToString().ToUpper(), attribute.Value);
            }
        }

        if (_metsData.Agent != null)
        {
            var agentElement = doc.CreateElement("agent");
            agentElement.SetAttribute("ROLE", _metsData.Agent.Role.ToString().ToUpper());
            agentElement.SetAttribute("TYPE", _metsData.Agent.Type.ToString().ToUpper());

            metshdr.AppendChild(agentElement);

            var compName = doc.CreateElement("name");
            compName.InnerText = _metsData.Agent.Name;
            agentElement.AppendChild(compName);

            if (_metsData.Agent.Note != null)
            {
                var note = doc.CreateElement("note");
                note.InnerText = _metsData.Agent.Note;
                agentElement.AppendChild(note);
            }
        }

        //Static info of the company
        if (_metsData.Company != null)
        {
            var companyAgent = doc.CreateElement("agent");
            companyAgent.SetAttribute("ROLE", _metsData.Company.Role.ToString().ToUpper());
            companyAgent.SetAttribute("TYPE", _metsData.Company.Type.ToString().ToUpper());

            metshdr.AppendChild(companyAgent);

            var companyname = doc.CreateElement("name");
            companyname.InnerText = _metsData.Company.Name;
            companyAgent.AppendChild(companyname);

            if (_metsData.Company.Note != null)
            {
                var companynote = doc.CreateElement("note");
                companynote.InnerText = _metsData.Company.Note;
                companyAgent.AppendChild(companynote);
            }
        }

        //software section
        var companySoftware = doc.CreateElement("agent");

        if (_metsData.Software != null)
        {
            companySoftware.SetAttribute("ROLE", _metsData.Software.Role.ToString().ToUpper());
            companySoftware.SetAttribute("TYPE", _metsData.Software.Type.ToString().ToUpper());
            companySoftware.SetAttribute("OTHERTYPE", _metsData.Software.OtherType.ToString().ToUpper());
            metshdr.AppendChild(companySoftware);

            var softwareName = doc.CreateElement("name");
            softwareName.InnerText = _metsData.Software.Name;
            companySoftware.AppendChild(softwareName);

            if (_metsData.Software.Note != null)
            {
                var softwareNote = doc.CreateElement("note");
                softwareNote.InnerText = _metsData.Software.Note;
                companySoftware.AppendChild(softwareNote);
            }
        }

        if (_metsData.AltRecords != null && _metsData.AltRecords.Any())
        {
            foreach (var altRecord in _metsData.AltRecords)
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
        if (_metsData.Mods != null)
        {
            var modsmods = doc.CreateElement("mods", "mods", "http://www.loc.gov/mods/v3");
            modsmods.SetAttribute("xmlns", _metsData.Mods.Xmlns);
            xmldata.AppendChild(modsmods);

            if (!string.IsNullOrEmpty(_metsData.Mods.Identifier))
            {
                var modsidentifier = doc.CreateElement("mods", "identifier", "http://www.loc.gov/mods/v3");
                modsidentifier.SetAttribute("type", "local");
                modsidentifier.InnerText = _metsData.Mods.Identifier;
                modsmods.AppendChild(modsidentifier);
            }

            var modslocation = doc.CreateElement("mods", "location", "http://www.loc.gov/mods/v3");
            modsmods.AppendChild(modslocation);

            //Allowed values: physicalLocation, shelfLocator or url
            if (_metsData.Mods.Url != null)
            {
                var modsurl = doc.CreateElement("mods", "url", "http://www.loc.gov/mods/v3");
                modsurl.InnerText = _metsData.Mods.Url.OriginalString;
                modslocation.AppendChild(modsurl);
            }

            //Allowed values: abstract, accessCondition, classification, extension, genre, identifier, language, location, name, note, originInfo, part, physicalDescription, recordInfo, relatedItem, subject, tableOfContents, targetAudience, titleInfo, typeOfResource
            var modsorigininfo = doc.CreateElement("mods", "originInfo", "http://www.loc.gov/mods/v3");
            modsmods.AppendChild(modsorigininfo);

            //Allowed values: place, publisher, dateIssued, dateCreated, dateCaptured, dateValid, dateModified, copyrightDate, dateOther, edition, issuance, frequency
            var modsDateIssued = doc.CreateElement("mods", "dateIssued", "http://www.loc.gov/mods/v3");
            modsDateIssued.SetAttribute("encoding", "w3cdtf");
            modsDateIssued.InnerText = _metsData.Mods.DateIssued.ToString("O");
            modsorigininfo.AppendChild(modsDateIssued);

            if (!string.IsNullOrEmpty(_metsData.Mods.Publisher))
            {
                var modstitle = doc.CreateElement("mods", "publisher", "http://www.loc.gov/mods/v3");
                modstitle.InnerText = _metsData.Mods.Publisher;
                modsorigininfo.AppendChild(modstitle);
            }

            if (_metsData.Mods.Place != null)
            {
                var place = doc.CreateElement("mods", "place", "http://www.loc.gov/mods/v3");
                var placeTerm = doc.CreateElement("mods", "placeTerm", "http://www.loc.gov/mods/v3");
                placeTerm.InnerText = _metsData.Mods.Place.PlaceTerm;
                place.AppendChild(placeTerm);
                modsorigininfo.AppendChild(place);
            }

            if (!string.IsNullOrEmpty(_metsData.Mods.AccessCondition))
            {
                var modsaccesscondition = doc.CreateElement("mods", "accessCondition", "http://www.loc.gov/mods/v3");
                modsaccesscondition.InnerText = _metsData.Mods.AccessCondition;
                modsmods.AppendChild(modsaccesscondition);
            }

            var modstitleinfo = doc.CreateElement("mods", "titleInfo", "http://www.loc.gov/mods/v3");
            modsmods.AppendChild(modstitleinfo);

            if (!string.IsNullOrEmpty(_metsData.Mods.ModsTitle))
            {
                var modstitle = doc.CreateElement("mods", "title", "http://www.loc.gov/mods/v3");
                modstitle.InnerText = _metsData.Mods.ModsTitle;
                modstitleinfo.AppendChild(modstitle);
            }

            var modsrelateditem = doc.CreateElement("mods", "relatedItem", "http://www.loc.gov/mods/v3");
            modsrelateditem.SetAttribute("type", "host");
            modsmods.AppendChild(modsrelateditem);

            if (_metsData.Mods.Uri != null)
            {
                var modsidentifier2 = doc.CreateElement("mods", "identifier", "http://www.loc.gov/mods/v3");
                modsidentifier2.SetAttribute("type", "uri");
                modsidentifier2.InnerText = _metsData.Mods.Uri.OriginalString;
                modsrelateditem.AppendChild(modsidentifier2);
            }

            if (!string.IsNullOrEmpty(_metsData.Mods.ModsTitleInfo))
            {
                var modsTitleInfo = doc.CreateElement("mods", "titleInfo", "http://www.loc.gov/mods/v3");
                modsrelateditem.AppendChild(modsTitleInfo);

                var modstitle2 = doc.CreateElement("mods", "title", "http://www.loc.gov/mods/v3");
                modstitle2.InnerText = _metsData.Mods.ModsTitleInfo;
                modsTitleInfo.AppendChild(modstitle2);
            }

            if (_metsData.Mods.Notes != null && _metsData.Mods.Notes.Length > 0)
            {
                foreach (var note in _metsData.Mods.Notes)
                {
                    var noteType = Regex.Replace(note.Type.ToString(), "([A-Z])", " $1").Trim().ToLower();

                    var noteNode = doc.CreateElement("mods", "note", "http://www.loc.gov/mods/v3");
                    noteNode.SetAttribute("type", noteType);
                    noteNode.InnerText = note.InnerText;

                    if (!string.IsNullOrEmpty(note.Href))
                    {
                        noteNode.SetAttribute("href", "http://www.w3.org/1999/xlink", note.Href);
                    }

                    modsmods.AppendChild(noteNode);
                }
            }
        }

        //From heres are the file section

        var filesec = doc.CreateElement("fileSec");
        root.AppendChild(filesec);

        var filegrp = doc.CreateElement("fileGrp");
        filesec.AppendChild(filegrp);

        if (_metsData.Files != null && _metsData.Files.Any())
        {
            foreach (var item in _metsData.Files)
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
                flocat.SetAttribute("LOCTYPE", item.LocType.ToString().ToUpper());

                var href = schema.Name == "eARD_Paket_FGS-PUBL_mets.xsd" ? $"file:{item.FileName}" : $"file:///{item.FileName}";
                flocat.SetAttribute("href", "http://www.w3.org/1999/xlink", href);
                flocat.SetAttribute("type", "http://www.w3.org/1999/xlink", "simple");

                file.AppendChild(flocat);

                filegrp.AppendChild(file);
            }

            var structMap = doc.CreateElement("structMap");
            structMap.SetAttribute("TYPE", "physical");
            root.AppendChild(structMap);

            var div = doc.CreateElement("div");
            div.SetAttribute("TYPE", "files");
            structMap.AppendChild(div);

            var div2 = doc.CreateElement("div");
            div2.SetAttribute("TYPE", "publication");
            div.AppendChild(div2);

            foreach (var item in _metsData.Files)
            {
                var fptr = doc.CreateElement("fptr");
                fptr.SetAttribute("FILEID", item.Id);
                div2.AppendChild(fptr);
            }
        }
        else if (_metsData.Sources != null && _metsData.Sources.Any())
        {
            foreach (var item in _metsData.Sources)
            {
                var created = item.CreationTime ?? DateTime.MinValue;

                var file = doc.CreateElement("file");
                file.SetAttribute("ID", item.Id);
                file.SetAttribute("USE", item.Use);
                file.SetAttribute("MIMETYPE", item.MimeType);
                file.SetAttribute("SIZE", item.Size.ToString());
                file.SetAttribute("CREATED", created.ToString("O"));
                if (!string.IsNullOrEmpty(item.Checksum))
                {
                    file.SetAttribute("CHECKSUM", item.Checksum);
                    file.SetAttribute("CHECKSUMTYPE", item.ChecksumType.ToString().ToUpper().Replace("_", "-"));
                }

                var flocat = doc.CreateElement("FLocat");
                //flocat.SetAttribute("LOCTYPE", item.LocType.ToString().ToUpper());
                flocat.SetAttribute("LOCTYPE", MetsData.ELocType.Url.ToString().ToUpper());

                var href = schema.Name == "eARD_Paket_FGS-PUBL_mets.xsd" ? $"file:{item.Name}" : $"file:///{item.Name}";
                flocat.SetAttribute("href", "http://www.w3.org/1999/xlink", href);
                flocat.SetAttribute("type", "http://www.w3.org/1999/xlink", "simple");

                file.AppendChild(flocat);

                filegrp.AppendChild(file);
            }

            var structMap = doc.CreateElement("structMap");
            structMap.SetAttribute("TYPE", "physical");
            root.AppendChild(structMap);

            var div = doc.CreateElement("div");
            div.SetAttribute("TYPE", "files");
            structMap.AppendChild(div);

            var div2 = doc.CreateElement("div");
            div2.SetAttribute("TYPE", "publication");
            div.AppendChild(div2);

            foreach (var item in _metsData.Sources)
            {
                var fptr = doc.CreateElement("fptr");
                fptr.SetAttribute("FILEID", item.Id);
                div2.AppendChild(fptr);
            }
        }
        else
        {
            var structMap = doc.CreateElement("structMap");
            structMap.SetAttribute("LABEL", "No structmap defined in this information package");
            root.AppendChild(structMap);

            var div = doc.CreateElement("div");
            structMap.AppendChild(div);
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
                source.ChecksumType = MetsData.EChecksumType.MD5;
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
        var compressedFileStream = new MemoryStream();
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