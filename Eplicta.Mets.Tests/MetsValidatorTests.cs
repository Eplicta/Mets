using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests
{
    public class MetsValidatorTests
    {
        [Fact]
        public void Basic1()
        {
            //Arrange
            var document = new XmlDocument();
            document.Load(@"C:\dev\Eplicta\mets\Resources\a.xml");
            var schema = new XmlDocument();
            schema.Load(@"C:\dev\Eplicta\mets\Resources\a.xsd");
            var sut = new XmlValidator();

            //Act
            var result = sut.Validate(document, schema);

            //Assert
            result.Should().BeEmpty();
        }

        //[Fact]
        //public void Basic2()
        //{
        //    //Arrange
        //    var document = new XmlDocument();
        //    document.Load(@"C:\dev\Eplicta\mets\Resources\mods99042030_linkedDataAdded.xml"); //C:\dev\Eplicta\mets\Resources\mods99042030_linkedDataAdded.xml
        //    var schema = new XmlDocument();
        //    schema.Load(@"C:\dev\Eplicta\mets\Resources\a.xsd"); //C:\dev\Eplicta\mets\Resources\mods-3-5.xsd
        //    var sut = new XmlValidator();

        //    //Act
        //    var result = sut.Validate(document, schema);

        //    //Assert
        //    result.Should().BeEmpty();
        //}

        [Fact]
        public void Basic3()
        {
            //Arrange
            var document = new XmlDocument();
            document.Load(@"C:\dev\Eplicta\mets\Resources\mods99042030_linkedDataAdded.xml"); //C:\dev\Eplicta\mets\Resources\mods99042030_linkedDataAdded.xml
            var schema = new XmlDocument();
            schema.Load(@"C:\dev\Eplicta\mets\Resources\mods-3-5.xsd");
            var sut = new XmlValidator();

            //Act
            var result = sut.Validate(document, schema);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Basic4()
        {
            //Arrange
            var document = new XmlDocument();
            document.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_1.xml");
            var schema = new XmlDocument();
            schema.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_xml1_0.xsd");
            var sut = new XmlValidator();

            //Act
            var result = sut.Validate(document, schema);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Basic6()
        {
            //Arrange
            var document = new XmlDocument();
            document.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_2.xml");
            var schema = new XmlDocument();
            schema.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_xml1_0.xsd");
            var sut = new XmlValidator();

            //Act
            var result = sut.Validate(document, schema);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Basic7()
        {
            //Arrange
            var document = new XmlDocument();
            document.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_2.xml");
            var schema = new XmlDocument();
            schema.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_xml1_1.xsd");
            var sut = new XmlValidator();

            //Act
            var result = sut.Validate(document, schema);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Basic5()
        {
            //Arrange
            var document = new XmlDocument();
            document.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_2_xml1_1.xml");
            var schema = new XmlDocument();
            schema.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_xml1_1.xsd");
            var sut = new XmlValidator();

            //Act
            var result = sut.Validate(document, schema);

            //Assert
            result.Should().BeEmpty();
        }

        //[Fact]
        //public void Lab()
        //{
        //    ////var d = new XmlDocument();
        //    ////d.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_1.xml");
        //    ////var r = ProcessFetchXml(d.OuterXml);
        //    ////r.Message.Should().BeEmpty();
        //    //////r.Success.Should().BeTrue();

        //    ////xs.Serialize(xw, xmlData);

        //    //// Set the XDocument
        //    ////XDocument document = XDocument.Parse(sw.ToString());
        //    //var d = new XmlDocument();
        //    ////d.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_1.xml");
        //    //d.Load(@"C:\dev\Eplicta\mets\Resources\mods99042030_linkedDataAdded.xml");
        //    //XDocument document = XDocument.Parse(d.OuterXml);

        //    //// Set the xsd file path
        //    ////string xsdPath = "C:/xsd/MyTypes.xsd";
        //    ////string xsdPath = @"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_xml1_0.xsd";
        //    //string xsdPath = @"http://www.kb.se/namespace/mods/MODS_enligt_FGS-PUBL_xml1_0.xsd";


        //    //// Set the XmlSchemaSet with the xsd files
        //    //XmlSchemaSet xss = new XmlSchemaSet();
        //    ////string ns = "http://mynamespace.example.com";
        //    ////xss.Add(null, xsdPath);
        //    //xss.Add(@"http://www.loc.gov/mods/v3", xsdPath);
        //    ////xss.Add(@"http://www.loc.gov/METS/", xsdPath);
        //    ////xss.Add(@"http://www.w3.org/1999/xlink", xsdPath);
        //    ////xss.Add(@"http://www.w3.org/XML/1998/namespace", xsdPath);

        //    //document.Validate(xss, null);



        //    //Arrange
        //    var document = new XmlDocument();
        //    document.Load(@"C:\dev\Eplicta\mets\Resources\mods99042030_linkedDataAdded.xml");
        //    //document.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_1.xml");
        //    //document.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_2.xml");
        //    //document.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_2_xml1_1.xml");
        //    var schema = new XmlDocument();
        //    schema.Load(@"C:\dev\Eplicta\mets\Resources\mods-3-5.xsd");
        //    //schema.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_xml1_0.xsd");
        //    //schema.Load(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_xml1_1.xsd");
        //    //using var ss = new StringReader(schema.OuterXml);
        //    //var s = XmlSchema.Read(ss);

        //    //var sut = new MetsValidator();
        //    var sut = new XmlValidator();

        //    //Act
        //    //var result = sut.Validate(document, schema);
        //    //var rr3 = sut.IsValidXml1(@"C:\dev\Eplicta\mets\Resources\mods99042030_linkedDataAdded.xml", @"C:\dev\Eplicta\mets\Resources\mods-3-5.xsd", @"http://www.loc.gov/mods/v3");
        //    //var rr3 = sut.IsValidXml1(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_2_xml1_1.xml", @"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_xml1_1.xsd", @"http://www.loc.gov/mods/v3");
        //    //var rr3 = sut.IsValidXml1(@"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_exempel_1.xml", @"C:\dev\Eplicta\mets\Resources\MODS_enligt_FGS-PUBL_xml1_0.xsd", @"http://www.loc.gov/mods/v3");
        //    var rr3 = sut.IsValidXml1(@"C:\dev\Eplicta\mets\Resources\a.xml", @"C:\dev\Eplicta\mets\Resources\a.xsd", @"");
        //    rr3.Should().BeEmpty();

        //    //var result = sut.Validate(document, Version.MODS_FGS_PUBL_1_1).ToArray();


        //    //var rr = result.FirstOrDefault()?.XmlSchemaException;
        //    //Assert
        //    //rr.Should().BeNull();
        //    //result.Should().BeEmpty();
        //}
    }
}