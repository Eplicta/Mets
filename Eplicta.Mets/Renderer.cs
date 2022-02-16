using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using Eplicta.Mets.Entities;
using System.Globalization;
using System;

namespace Eplicta.Mets
{

    public class Renderer
    {
        private readonly ModsData _modsData;

        public Renderer(ModsData metsData)
        {
            _modsData = metsData;
        }

        public XmlDocument Render()
        {
            var doc = new XmlDocument();

            var documentType = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(documentType);

            var root = doc.CreateElement("mods");
            doc.AppendChild(root);
            root.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
            root.SetAttribute("version", "3.5");

            root.SetAttribute("xmlns", "http://www.loc.gov/mods/v3");

            var element = doc.DocumentElement;
            var attr = doc.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            attr.Value = "http://www.loc.gov/mods/v3 http://www.loc.gov/standards/mods/v3/mods-3-5.xsd";
            element?.Attributes.Append(attr);

            root.SetAttribute("xml:lang", "SE");

            AppendTitleInfo(doc, root);
            //AppendName(doc, root);
            //AppendTypeOfResource(doc, root);
            //AppendGenre(doc, root);
            AppendOriginInfo(doc, root);
            ModsRenderer(doc, root);

            



            /*
            <language>
                <languageTerm authority="iso639-2b" type="code"
                  authorityURI="http://id.loc.gov/vocabulary/iso639-2"
                  valueURI="http://id.loc.gov/vocabulary/iso639-2/eng">eng</languageTerm>
              </language>

              <physicalDescription>
                <form authority="marcform">print</form>
                <extent>vii, 322 p. ; 23 cm.</extent>
              </physicalDescription>

              <note type="statement of responsibility">Eric Alterman.</note>
              <note>Includes bibliographical references (p. 291-312) and index.</note>

              <subject authority="lcsh" authorityURI="http://id.loc.gov/authorities/subjects">
                <topic valueURI="http://id.loc.gov/authorities/subjects/sh85070736">Journalism</topic>
                <topic valueURI="http://id.loc.gov/authorities/subjects/sh00005651">Political aspects</topic>
                <geographic valueURI="http://id.loc.gov/authorities/names/n78095330">United States</geographic>
              </subject>

              <subject authority="lcsh" authorityURI="http://id.loc.gov/authorities/subjects">
                <geographic valueURI="http://id.loc.gov/authorities/names/n78095330">United States</geographic>
                <topic valueURI="http://id.loc.gov/authorities/subjects/sh2002011436">Politics and
                  government</topic>
                <temporal valueURI="http://id.loc.gov/authorities/subjects/sh2002012476">20th century</temporal>
              </subject>

              <subject authority="lcsh" authorityURI="http://id.loc.gov/authorities/subjects"
                valueURI="http://id.loc.gov/authorities/subjects/sh2008107507">
                <topic valueURI="http://id.loc.gov/authorities/subjects/sh85081863">Mass media</topic>
                <topic valueURI="http://id.loc.gov/authorities/subjects/sh00005651">Political aspects</topic>
                <geographic valueURI="http://id.loc.gov/authorities/names/n78095330">United States</geographic>
              </subject>

              <subject authority="lcsh" authorityURI="http://id.loc.gov/authorities/subjects"
                valueURI="http://id.loc.gov/authorities/subjects/sh2010115992">
                <topic valueURI="http://id.loc.gov/authorities/subjects/sh85133490">Television and
                  politics</topic>
                <geographic valueURI="http://id.loc.gov/authorities/names/n78095330">United States</geographic>
              </subject>

              <subject authority="lcsh" authorityURI="http://id.loc.gov/authorities/subjects"
                valueURI="http://id.loc.gov/authorities/subjects/sh2008109555">
                <topic valueURI="http://id.loc.gov/authorities/subjects/sh85106514">Press and politics</topic>
                <geographic valueURI="http://id.loc.gov/authorities/names/n78095330">United States</geographic>
              </subject>

              <subject authority="lcsh" authorityURI="http://id.loc.gov/authorities/subjects" valueURI="http://id.loc.gov/authorities/subjects/sh2010115993.html">
                <topic valueURI="http://id.loc.gov/authorities/subjects/sh2006004518.html">Television talk shows</topic>
                <geographic valueURI="http://id.loc.gov/authorities/names/n78095330">United States</geographic>
              </subject>

              <classification authority="lcc">PN4888.P6 A48 1999</classification>
              <classification edition="21" authority="ddc">071/.3</classification>

              <identifier type="isbn">0801486394 (pbk. : acid-free, recycled paper)</identifier>
              <identifier type="lccn">99042030</identifier>

              <recordInfo>
                <descriptionStandard>aacr</descriptionStandard>
                <recordContentSource>DLC</recordContentSource>
                <recordCreationDate encoding="marc">990730</recordCreationDate>
                <recordChangeDate encoding="iso8601">20000406144503.0</recordChangeDate>
                <recordIdentifier>11761548</recordIdentifier>
                <recordOrigin>Converted from MARCXML to MODS version 3.4 using MARC21slim2MODS3-4.xsl (Revision
                  1.74), updated to MODS 3.5 with valueURIs and authorityURIs added by hand</recordOrigin>
              </recordInfo>
            */

            return doc;
        }

        public void ModsRenderer(XmlDocument doc,XmlElement root)
        {
            // dynamic info, the create date with accordance to ISO 8601
            DateTime TodaysDate = DateTime.Now;
            string DateNow = TodaysDate.ToString("O");

            //Creates the metsHdr tag where agents and RecordID's will be
            var metshdr = doc.CreateElement("metsHdr");
            root.AppendChild(metshdr);
            metshdr.SetAttribute("CREATEDATE", DateNow);

        //dynamic info, the needed information
            var AgentElement = doc.CreateElement("agent");
            metshdr.AppendChild(AgentElement);
            AgentElement.SetAttribute("ROLE", _modsData.agent.Role);
            AgentElement.SetAttribute("TYPE", _modsData.agent.Type);
            AgentElement.SetAttribute("OTHERTYPE", _modsData.agent.OtherType);

            var CompName = doc.CreateElement("name");
            CompName.InnerText = _modsData.agent.name;
            AgentElement.AppendChild(CompName);

            var note = doc.CreateElement("note");
            note.InnerText = _modsData.agent.note;
            AgentElement.AppendChild(note);


        //Static info of the company 
            var Companyagent = doc.CreateElement("agent");
            Companyagent.SetAttribute("ROLE", _modsData.eplicta.Role);
            Companyagent.SetAttribute("TYPE", _modsData.eplicta.Type);
                    metshdr.AppendChild(Companyagent);

            var companyname = doc.CreateElement("name");
            companyname.InnerText = _modsData.eplicta.name;
                Companyagent.AppendChild(companyname);

            var companynote = doc.CreateElement("name");
            companynote.InnerText = _modsData.eplicta.note;
            Companyagent.AppendChild(companynote);

                
            var recordID1 = doc.CreateElement("AltRecordID");
            recordID1.InnerText = "Deposit";                //Will make it later to an array that holds data for all 3 RecordsID innertext
            recordID1.SetAttribute("type", _modsData.records.type1);    //same for types
            metshdr.AppendChild(recordID1);


            var recordID2 = doc.CreateElement("AltRecordID");
            recordID2.InnerText = "Deposit";
            recordID2.SetAttribute("type", _modsData.records.type2);
            metshdr.AppendChild(recordID2);

            var recordID3 = doc.CreateElement("AltRecordID");
            recordID3.InnerText = "Deposit";
            recordID3.SetAttribute("type", _modsData.records.type3);
            metshdr.AppendChild(recordID3);

            //Here is metsHdr element ending

            //start of dmdSec
            var dmdSec = doc.CreateElement("dmdSec");
            dmdSec.SetAttribute("ID", "ID1");
            root.AppendChild(dmdSec);

            var mdwrap = doc.CreateElement("mdWrap");
            mdwrap.SetAttribute("MDTYPE", "MODS");
            dmdSec.AppendChild(mdwrap);

            var xmldata = doc.CreateElement("xmlData");
            dmdSec.AppendChild(xmldata);

            
            //mods:mods
            var modsmods = doc.CreateElement("mods:mods");
            modsmods.SetAttribute("xmlns", "http://www.w3.org/1999/xlink");
            dmdSec.AppendChild(modsmods);

            //mods:identifier
            var modslocation = doc.CreateElement("mods:location");
            modslocation.SetAttribute("type", "local");
            modslocation.InnerText = "C5385FBC5FC559E7C43AB6700DB28EF3"; //is supposed to be dynamic
            modsmods.AppendChild(modslocation);

            var modsurl = doc.CreateElement("mods:URL");
            modsurl.InnerText = "https://www.alingsas.se/utbildning-och-barnomsorg/vuxenutbildning/jag-vill-studera/program-i-alingsas/moln-och-virtualiseringspecialist/";
            modslocation.AppendChild(modsurl);

            var modsorigininfo = doc.CreateElement("mods:origininfo");
            modsmods.AppendChild(modsorigininfo);

            var modsDateIssued = doc.CreateElement("mods:DateIssued");
            modsDateIssued.SetAttribute("encoding", "w3cdtf");
            modsorigininfo.AppendChild(modsDateIssued);





        }

        private void AppendOriginInfo(XmlDocument doc, XmlElement root)
        {
            var originInfo = doc.CreateElement("originInfo");
            root.AppendChild(originInfo);
            originInfo.SetAttribute("eventType", "publication");

            //var place = doc.CreateElement("place");
            //originInfo.AppendChild(place);
            //var placeTerm = doc.CreateElement("placeTerm");
            //place.AppendChild(placeTerm);
            //placeTerm.SetAttribute("authority", "marccountry");
            //placeTerm.SetAttribute("type", "code");
            //placeTerm.SetAttribute("authorityURI", "http://id.loc.gov/vocabulary/countries");
            //placeTerm.SetAttribute("valueURI", "http://id.loc.gov/vocabulary/countries/nyu");
            //placeTerm.InnerText = "nyu";

            //var placeA = doc.CreateElement("place");
            //originInfo.AppendChild(placeA);
            //var placeTermA = doc.CreateElement("placeTerm");
            //placeA.AppendChild(placeTermA);
            //placeTermA.InnerText = "Ithaca, N.Y";
            //placeTermA.SetAttribute("type", "text");

            var publisher = doc.CreateElement("publisher");
            originInfo.AppendChild(publisher);
            publisher.InnerText = _modsData.Creator;

            //var dateIssued = doc.CreateElement("dateIssued");
            //originInfo.AppendChild(dateIssued);
            //dateIssued.InnerText = "c1999";

            //var dateIssuedX = doc.CreateElement("dateIssued");
            //originInfo.AppendChild(dateIssuedX);
            //dateIssuedX.InnerText = "1999";
            //dateIssuedX.SetAttribute("encoding", "marc");

            //var issuance = doc.CreateElement("issuance");
            //originInfo.AppendChild(issuance);
            //issuance.InnerText = "monographic";
        }

        private static void AppendGenre(XmlDocument doc, XmlElement root)
        {
            var genre = doc.CreateElement("genre");
            root.AppendChild(genre);
            genre.InnerText = "bibliography";
            genre.SetAttribute("authority", "marcgt");
        }

        private static void AppendTypeOfResource(XmlDocument doc, XmlElement root)
        {
            var typeOfResource = doc.CreateElement("typeOfResource");
            root.AppendChild(typeOfResource);
            typeOfResource.InnerText = "text";
        }

        private void AppendName(XmlDocument doc, XmlElement root)
        {
            var name = doc.CreateElement("name");
            root.AppendChild(name);

            name.SetAttribute("type", "personal");
            name.SetAttribute("authorityURI", "http://id.loc.gov/authorities/names");
            name.SetAttribute("valueURI", "http://id.loc.gov/authorities/names/n92101908");

            if (!string.IsNullOrEmpty(_modsData.Name?.NamePart))
            {
                var title = doc.CreateElement("namePart");
                name.AppendChild(title);
                title.InnerText = _modsData.Name.NamePart;

                var role = doc.CreateElement("role");
                name.AppendChild(role);

                var roleTerm = doc.CreateElement("roleTerm");
                role.AppendChild(roleTerm);
                roleTerm.InnerText = "creator";
                roleTerm.SetAttribute("type", "text");
            }
        }

        private void AppendTitleInfo(XmlDocument doc, XmlElement root)
        {
            var titleInfo = doc.CreateElement("titleInfo");
            root.AppendChild(titleInfo);

            if (!string.IsNullOrEmpty(_modsData.TitleInfo?.Title))
            {
                var title = doc.CreateElement("title");
                titleInfo.AppendChild(title);
                title.InnerText = _modsData.TitleInfo.Title;
            }

            if (!string.IsNullOrEmpty(_modsData.TitleInfo?.SubTitle))
            {
                var title = doc.CreateElement("subTitle");
                titleInfo.AppendChild(title);
                title.InnerText = _modsData.TitleInfo.SubTitle;
            }
        }

        public MemoryStream GetArchiveStream()
        {
            using var compressedFileStream = new MemoryStream();
            using var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, false);

            AddFile(zipArchive, "metadata.xml", Render().OuterXml);

            if (_modsData.Resources != null)
            {
                foreach (var resource in _modsData.Resources)
                {
                    AddFile(zipArchive, $"data/{resource.Name}", resource.Data);
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
}