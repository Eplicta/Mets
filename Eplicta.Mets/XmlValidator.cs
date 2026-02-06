using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets;

[Obsolete("Use IValidatorService instead.")]
public class XmlValidator
{
    [Obsolete("Use IValidatorService instead.")]
    public IEnumerable<XmlValidatorResult> Validate(XmlDocument document)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        var results = new List<XmlValidatorResult>();

        var schemaLocations = GetSchemaLocations(document, results);

        // Preflight: try to fetch schemas and report "Info" + version
        PreflightOnlineSchemas(schemaLocations, results);

        // Actual validation: mimic your XmlReader-based validator to get the same warnings/errors
        ValidateWithXmlReader(document, results);

        return results;
    }

    private static void PreflightOnlineSchemas(List<SchemaLocation> schemaLocations, List<XmlValidatorResult> results)
    {
        foreach (var sl in schemaLocations)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "XmlValidator/1.0");

                    var data = wc.DownloadData(sl.SchemaUri);

                    using (var ms = new MemoryStream(data))
                    using (var xr = XmlReader.Create(ms, new XmlReaderSettings
                           {
                               DtdProcessing = DtdProcessing.Prohibit
                           }, sl.SchemaUri.ToString()))
                    {
                        var schema = XmlSchema.Read(xr, null);

                        var targetNs = schema != null && !string.IsNullOrWhiteSpace(schema.TargetNamespace)
                            ? schema.TargetNamespace
                            : sl.NamespaceUri;

                        var version = schema != null && !string.IsNullOrWhiteSpace(schema.Version)
                            ? schema.Version
                            : InferVersionFromUrl(sl.SchemaUri.ToString());

                        var label = GetWellKnownLabel(targetNs);

                        var msg = version != null
                            ? $"Info: Schema reachable: {label} namespace='{targetNs}', version='{version}', url='{sl.SchemaUri}'."
                            : $"Info: Schema reachable: {label} namespace='{targetNs}', url='{sl.SchemaUri}' (version not declared/inferable).";

                        // We store "Info" as Warning severity because XmlSeverityType has no Information.
                        results.Add(new XmlValidatorResult(msg, XmlSeverityType.Warning, null));
                    }
                }
            }
            catch (WebException e)
            {
                results.Add(new XmlValidatorResult(
                    $"Warning: Schema not reachable: '{sl.SchemaUri}' for namespace '{sl.NamespaceUri}'. ({e.Message})",
                    XmlSeverityType.Warning,
                    null));
            }
            catch (Exception e) when (e is XmlException || e is XmlSchemaException)
            {
                var schemaEx = e as XmlSchemaException ?? new XmlSchemaException(e.Message, e);
                results.Add(new XmlValidatorResult(
                    $"Warning: Schema could not be read: '{sl.SchemaUri}' for namespace '{sl.NamespaceUri}'. ({e.Message})",
                    XmlSeverityType.Warning,
                    schemaEx));
            }
        }
    }

    private static void ValidateWithXmlReader(XmlDocument document, List<XmlValidatorResult> results)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            ValidationType = ValidationType.Schema,
            ValidationFlags =
                XmlSchemaValidationFlags.ProcessSchemaLocation |
                XmlSchemaValidationFlags.ProcessInlineSchema |
                XmlSchemaValidationFlags.ReportValidationWarnings,
            XmlResolver = new XmlUrlResolver()
        };

        settings.ValidationEventHandler += (_, e) =>
        {
            // Keep the same severity the framework reports
            var ex = e.Exception;
            results.Add(new XmlValidatorResult(e.Message, e.Severity, ex));
        };

        try
        {
            using var sr = new StringReader(document.OuterXml);
            using var reader = XmlReader.Create(sr, settings);
            while (reader.Read())
            {
            }
        }
        catch (Exception e)
        {
            var schemaEx = e as XmlSchemaException ?? new XmlSchemaException(e.Message, e);
            results.Add(new XmlValidatorResult("FAILED: " + e.Message, XmlSeverityType.Error, schemaEx));
        }
    }

    private static List<SchemaLocation> GetSchemaLocations(XmlDocument document, List<XmlValidatorResult> results)
    {
        var root = document.DocumentElement;
        if (root == null)
        {
            results.Add(new XmlValidatorResult("Document has no root element.", XmlSeverityType.Error, null));
            return new List<SchemaLocation>();
        }

        var xsiNs = "http://www.w3.org/2001/XMLSchema-instance";
        var schemaLocationValue = root.GetAttribute("schemaLocation", xsiNs);

        if (string.IsNullOrWhiteSpace(schemaLocationValue))
        {
            results.Add(new XmlValidatorResult(
                "Warning: No xsi:schemaLocation found in the document. Online schema loading cannot run.",
                XmlSeverityType.Warning,
                null));

            return new List<SchemaLocation>();
        }

        var tokens = schemaLocationValue.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length % 2 != 0)
        {
            results.Add(new XmlValidatorResult(
                "Warning: xsi:schemaLocation is malformed (must contain pairs: 'namespace schemaUrl').",
                XmlSeverityType.Warning,
                null));
        }

        var list = new List<SchemaLocation>();

        for (var i = 0; i + 1 < tokens.Length; i += 2)
        {
            var ns = tokens[i].Trim();
            var url = tokens[i + 1].Trim();

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                results.Add(new XmlValidatorResult(
                    $"Warning: Schema URL is not absolute or invalid: '{url}' (namespace '{ns}').",
                    XmlSeverityType.Warning,
                    null));
                continue;
            }

            list.Add(new SchemaLocation(ns, uri));
        }

        return list;
    }

    private static string InferVersionFromUrl(string schemaUrl)
    {
        var file = Path.GetFileName(schemaUrl);
        if (string.IsNullOrWhiteSpace(file))
        {
            return null;
        }

        var idx = file.IndexOf("mods-", StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return null;
        }

        var tail = file.Substring(idx + "mods-".Length);

        var dot = tail.IndexOf(".xsd", StringComparison.OrdinalIgnoreCase);
        if (dot > 0)
        {
            tail = tail.Substring(0, dot);
        }

        var parts = tail.Split(['-'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && int.TryParse(parts[0], out _) && int.TryParse(parts[1], out _))
        {
            return parts[0] + "." + parts[1];
        }

        return null;
    }

    private static string GetWellKnownLabel(string ns)
    {
        if (string.Equals(ns, "ExtensionMETS", StringComparison.OrdinalIgnoreCase))
        {
            return "ExtensionMETS";
        }

        switch (ns)
        {
            case "http://www.loc.gov/METS/":
                return "METS";
            case "http://www.loc.gov/mods/v3":
                return "MODS";
            case "http://www.w3.org/1999/xlink":
                return "XLINK";
            case "http://www.w3.org/XML/1998/namespace":
                return "XML";
            case "http://www.w3.org/2000/09/xmldsig#":
                return "XMLDSIG";
            default:
                return "Schema";
        }
    }

    private struct SchemaLocation
    {
        public SchemaLocation(string namespaceUri, Uri schemaUri)
        {
            NamespaceUri = namespaceUri;
            SchemaUri = schemaUri;
        }

        public string NamespaceUri { get; }
        public Uri SchemaUri { get; }
    }
}