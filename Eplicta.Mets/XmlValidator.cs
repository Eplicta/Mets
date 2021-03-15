using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets
{
    public class XmlValidator
    {
        //public IEnumerable<XmlValidatorResult> IsValidXml1(string xmlFilePath, string xsdFilePath, string namespaceName)
        //{
        //    XDocument xdoc = null;
        //    var settings = new XmlReaderSettings();
        //    settings.DtdProcessing = DtdProcessing.Ignore;
        //    var responses = new List<XmlValidatorResult>();

        //    try
        //    {
        //        using (XmlReader xr = XmlReader.Create(xmlFilePath, settings))
        //        {
        //            xdoc = XDocument.Load(xr);
        //            var schemas = new XmlSchemaSet();
        //            schemas.Add(namespaceName, xsdFilePath);
        //            using (var fs = File.OpenRead(@"C:\dev\Eplicta\mets\Resources\xmldsig-core-schema.xsd"))
        //            using (var reader = XmlReader.Create(fs, new XmlReaderSettings()
        //            {
        //                DtdProcessing = DtdProcessing.Ignore // important
        //            }))
        //            {
        //                schemas.Add(@"http://www.w3.org/2000/09/xmldsig#", reader);
        //                //schemas.Add(@"http://www.loc.gov/mods/v3", reader);
        //            }

        //            xdoc.Validate(schemas, (_, e) =>
        //            {
        //                responses.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception));
        //            });
        //        }
        //    }
        //    catch (XmlSchemaValidationException e)
        //    {
        //        responses.Add(new XmlValidatorResult(e.Message, XmlSeverityType.Error, new XmlSchemaException(e.Message, e)));
        //    }

        //    return responses;
        //}

        public IEnumerable<XmlValidatorResult> Validate(XmlDocument document, XmlDocument schema)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            const XmlSchemaValidationFlags validationFlags =
                XmlSchemaValidationFlags.ProcessInlineSchema |
                XmlSchemaValidationFlags.ProcessSchemaLocation |
                XmlSchemaValidationFlags.ReportValidationWarnings |
                XmlSchemaValidationFlags.AllowXmlAttributes;

            XDocument xdoc = null;
            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                ValidationFlags = validationFlags,
                //DtdProcessing = DtdProcessing.Parse
                DtdProcessing = DtdProcessing.Ignore,
            };
            var responses = new List<XmlValidatorResult>();

            try
            {
                var xmlns = document.FirstChild?.Attributes?["xmlns"]?.InnerText;
                //var xmlns = "";

                using var ss1 = new StringReader(document.OuterXml);
                using (XmlReader xr = XmlReader.Create(ss1, settings))
                {
                    xdoc = XDocument.Load(xr);
                    var schemas = new XmlSchemaSet();
                    using var ss2 = new StringReader(schema.OuterXml);
                    schemas.Add(xmlns, XmlReader.Create(ss2));
                    using (var fs = File.OpenRead(@"C:\dev\Eplicta\mets\Resources\xmldsig-core-schema.xsd"))
                    using (var reader = XmlReader.Create(fs, new XmlReaderSettings()
                    {
                        DtdProcessing = DtdProcessing.Ignore // important
                    }))
                    {
                        schemas.Add(@"http://www.w3.org/2000/09/xmldsig#", reader);
                    }

                    using (var fs1 = File.OpenRead(@"C:\dev\Eplicta\mets\Resources\xml.xsd"))
                    using (var reader1 = XmlReader.Create(fs1, new XmlReaderSettings()
                    {
                        DtdProcessing = DtdProcessing.Ignore // important
                    }))
                    {
                        schemas.Add(@"http://www.w3.org/XML/1998/namespace", reader1);
                    }

                    using (var fs2 = File.OpenRead(@"C:\dev\Eplicta\mets\Resources\xlink.xsd"))
                    using (var reader2 = XmlReader.Create(fs2, new XmlReaderSettings()
                    {
                        DtdProcessing = DtdProcessing.Ignore // important
                    }))
                    {
                        schemas.Add(@"http://www.w3.org/1999/xlink", reader2);
                    }

                    xdoc.Validate(schemas, (_, e) =>
                    {
                        responses.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception));
                    });
                }
            }
            catch (XmlSchemaValidationException e)
            {
                responses.Add(new XmlValidatorResult(e.Message, XmlSeverityType.Error, new XmlSchemaException(e.Message, e)));
            }

            return responses;

            //var responses = new List<XmlValidatorResult>();

            //try
            //{
            //    //var schemaSet = new XmlSchemaSet();
            //    //using var schemaStream = new StringReader(schema.OuterXml);

            //    //var rrr = XmlSchema.Read(schemaStream, (_, e) =>
            //    //{
            //    //    responses.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception));
            //    //});

            //    ////schemaSet.ValidationEventHandler += new ValidationEventHandler(ShowCompileError);
            //    //schemaSet.ValidationEventHandler += (s, e) =>
            //    //{
            //    //    responses.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception));
            //    //};
            //    //schemaSet.Add(rrr);
            //    //schemaSet.Compile();

            //    //XmlSchema compiledSchema = null;

            //    //foreach (XmlSchema schema1 in schemaSet.Schemas())
            //    //{
            //    //    compiledSchema = schema1;
            //    //}

            //    //var schem = compiledSchema;

            //    //if (schem.IsCompiled)
            //    //{
            //    //    // Schema is successfully compiled.
            //    //    // Do something with it here.
            //    //    //document.Validate(schemaSet, (s, e) =>
            //    //    //{

            //    //    //});

            //    //    using var documentStream = new StringReader(document.OuterXml);
            //    //    using var rd = XmlReader.Create(documentStream);
            //    //    var doc = XDocument.Load(rd);

            //    //    doc.Validate(schemaSet, (_, e) =>
            //    //    {
            //    //        responses.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception));
            //    //    });

            //    //}

            //    var schemaSet = new XmlSchemaSet();
            //    using var schemaStream = new StringReader(schema.OuterXml);
            //    //var xmlns = (string)null;
            //    //var xmlns = document.FirstChild?.Attributes?["xmlns"]?.InnerText;
            //    var xmlns = @"http://www.loc.gov/mods/v3";
            //    //var xmlns = @"http://www.loc.gov/METS/";
            //    //var xmlns = @"http://www.loc.gov/mods/v3";
            //    //var xmlns = @"http://www.w3.org/1999/xlink";
            //    using var sd = XmlReader.Create(schemaStream);
            //    schemaSet.Add(xmlns, sd);

            //    using var documentStream = new StringReader(document.OuterXml);
            //    using var rd = XmlReader.Create(documentStream);
            //    var doc = XDocument.Load(rd);

            //    doc.Validate(schemaSet, (_, e) =>
            //    {
            //        responses.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception));
            //    });
            //}
            //catch (Exception e)
            //{
            //    responses.Add(new XmlValidatorResult(e.Message, XmlSeverityType.Error, new XmlSchemaException(e.Message, e)));
            //}

            //foreach (var response in responses)
            //{
            //    yield return response;
            //}
        }

        private static void ShowCompileError(object sender, ValidationEventArgs e)
        {
            Console.WriteLine("Validation Error: {0}", e.Message);
        }
    }
}