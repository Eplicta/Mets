using Eplicta.Mets.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Eplicta.Mets;

public class XmlValidator
{
    public IEnumerable<XmlValidatorResult> Validate(XmlDocument document)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        var results = new List<XmlValidatorResult>();

        // 1) Read schemaLocation pairs from the instance document
        var schemaLocations = GetSchemaLocations(document, results);

        // 2) Try to download and compile the online schemas (warn if unavailable)
        var schemas = new XmlSchemaSet();
        var loaded = new List<LoadedSchemaInfo>();

        foreach (var sl in schemaLocations)
        {
            if (!TryAddOnlineSchema(schemas, sl, loaded, out var warning))
            {
                results.Add(new XmlValidatorResult(warning, XmlSeverityType.Warning, null));
            }
        }

        try
        {
            schemas.Compile();
        }
        catch (XmlSchemaException e)
        {
            results.Add(new XmlValidatorResult($"Schema compilation error: {e.Message}", XmlSeverityType.Error, e));
            return results;
        }

        // 3) Report detected "versions" (best-effort)
        foreach (var info in loaded)
        {
            var version = !string.IsNullOrWhiteSpace(info.SchemaVersion)
                ? info.SchemaVersion
                : InferVersionFromUrl(info.SchemaUri);

            var label = GetWellKnownLabel(info.NamespaceUri);
            var msg = version != null
                ? $"Schema loaded: {label} namespace='{info.NamespaceUri}', version='{version}', url='{info.SchemaUri}'."
                : $"Schema loaded: {label} namespace='{info.NamespaceUri}', url='{info.SchemaUri}' (version not declared/inferable).";

            results.Add(new XmlValidatorResult(msg, XmlSeverityType.Warning, null));
        }

        // 4) Validate instance XML against the downloaded schema set
        try
        {
            using var sr = new StringReader(document.OuterXml);
            using var xr = XmlReader.Create(sr, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit
            });

            var xdoc = XDocument.Load(xr, LoadOptions.SetLineInfo);

            xdoc.Validate(
                schemas,
                (_, e) => results.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception)),
                addSchemaInfo: true);
        }
        catch (Exception e) when (e is XmlException || e is XmlSchemaException)
        {
            var schemaEx = e as XmlSchemaException ?? new XmlSchemaException(e.Message, e);
            results.Add(new XmlValidatorResult(e.Message, XmlSeverityType.Error, schemaEx));
        }

        return results;
    }

    private static List<SchemaLocation> GetSchemaLocations(XmlDocument document, List<XmlValidatorResult> results)
    {
        var root = document.DocumentElement;
        if (root == null)
        {
            results.Add(new XmlValidatorResult("Document has no root element.", XmlSeverityType.Error, null));
            return [];
        }

        var xsiNs = "http://www.w3.org/2001/XMLSchema-instance";
        var schemaLocationValue = root.GetAttribute("schemaLocation", xsiNs);

        if (string.IsNullOrWhiteSpace(schemaLocationValue))
        {
            results.Add(new XmlValidatorResult(
                "No xsi:schemaLocation found in the document. Online schema loading cannot run.",
                XmlSeverityType.Warning,
                null));

            return [];
        }

        var tokens = schemaLocationValue
            .Split((char[])null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length % 2 != 0)
        {
            results.Add(new XmlValidatorResult(
                "xsi:schemaLocation is malformed (must contain pairs: 'namespace schemaUrl').",
                XmlSeverityType.Warning,
                null));
        }

        var list = new List<SchemaLocation>();

        for (var i = 0; i + 1 < tokens.Length; i += 2)
        {
            var ns = tokens[i];
            var url = tokens[i + 1];

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                results.Add(new XmlValidatorResult(
                    $"Schema URL is not absolute or invalid: '{url}' (namespace '{ns}').",
                    XmlSeverityType.Warning,
                    null));
                continue;
            }

            list.Add(new SchemaLocation(ns, uri));
        }

        return list;
    }

    private static bool TryAddOnlineSchema(
        XmlSchemaSet schemas,
        SchemaLocation schemaLocation,
        List<LoadedSchemaInfo> loaded,
        out string warning)
    {
        warning = "";

        try
        {
            using var wc = new WebClient();
            wc.Headers.Add(HttpRequestHeader.UserAgent, "XmlValidator/1.0");

            // Download as bytes so we can set a base URI for includes/imports if needed
            var data = wc.DownloadData(schemaLocation.SchemaUri);

            using var ms = new MemoryStream(data);
            using var xr = XmlReader.Create(ms, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            });

            var schema = XmlSchema.Read(xr, null);
            if (schema == null)
            {
                warning = $"Could not parse schema from '{schemaLocation.SchemaUri}'.";
                return false;
            }

            // Ensure target namespace aligns with the declared schemaLocation namespace.
            // If the schema has empty targetNamespace but schemaLocation provides one, keep the declared one.
            var schemaNs = !string.IsNullOrWhiteSpace(schema.TargetNamespace)
                ? schema.TargetNamespace
                : schemaLocation.NamespaceUri;

            //schemas.Add(schemaNs, schema);
            schemas.Add(schema);

            loaded.Add(new LoadedSchemaInfo(schemaNs, schemaLocation.SchemaUri.ToString(), schema.Version));

            return true;
        }
        catch (WebException e)
        {
            warning = $"Schema not reachable: '{schemaLocation.SchemaUri}' for namespace '{schemaLocation.NamespaceUri}'. ({e.Message})";
            return false;
        }
        catch (Exception e) when (e is XmlSchemaException || e is XmlException || e is NotSupportedException)
        {
            warning = $"Schema could not be loaded: '{schemaLocation.SchemaUri}' for namespace '{schemaLocation.NamespaceUri}'. ({e.Message})";
            return false;
        }
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

        var parts = tail.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && int.TryParse(parts[0], out _) && int.TryParse(parts[1], out _))
        {
            return parts[0] + "." + parts[1];
        }

        return null;
    }

    private static string GetWellKnownLabel(string ns)
    {
        return ns switch
        {
            "http://www.loc.gov/METS/" => "METS",
            "http://www.loc.gov/mods/v3" => "MODS",
            "http://www.w3.org/1999/xlink" => "XLINK",
            "http://www.w3.org/XML/1998/namespace" => "XML",
            "http://www.w3.org/2000/09/xmldsig#" => "XMLDSIG",
            _ => "Schema"
        };
    }

    private readonly struct SchemaLocation
    {
        public SchemaLocation(string namespaceUri, Uri schemaUri)
        {
            NamespaceUri = namespaceUri;
            SchemaUri = schemaUri;
        }

        public string NamespaceUri { get; }
        public Uri SchemaUri { get; }
    }

    private readonly struct LoadedSchemaInfo
    {
        public LoadedSchemaInfo(string namespaceUri, string schemaUri, string schemaVersion)
        {
            NamespaceUri = namespaceUri;
            SchemaUri = schemaUri;
            SchemaVersion = schemaVersion;
        }

        public string NamespaceUri { get; }
        public string SchemaUri { get; }
        public string SchemaVersion { get; }
    }
}