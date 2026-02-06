using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets;

internal class ValidatorService : IValidatorService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ValidatorService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IEnumerable<ValidatorResult> Validate(XmlDocument document)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        // Interface is sync, but fetching schemas is naturally async.
        return ValidateInternalAsync(document).GetAwaiter().GetResult();
    }

    private async Task<IEnumerable<ValidatorResult>> ValidateInternalAsync(XmlDocument document)
    {
        var results = new List<ValidatorResult>();

        var schemaLocations = GetSchemaLocations(document, results);

        // 1) Preflight: try to reach schema URLs and report "Information" + best-effort version
        await PreflightSchemasAsync(schemaLocations, results);

        // 2) Actual validation: validate using schemaLocation (like your XmlReader sample)
        ValidateWithXmlReader(document, results);

        return results;
    }

    private List<SchemaLocation> GetSchemaLocations(XmlDocument document, List<ValidatorResult> results)
    {
        var root = document.DocumentElement;
        if (root == null)
        {
            results.Add(Info("Document has no root element.", SeverityType.Error));
            return new List<SchemaLocation>();
        }

        var xsiNs = "http://www.w3.org/2001/XMLSchema-instance";
        var schemaLocationValue = root.GetAttribute("schemaLocation", xsiNs);

        if (string.IsNullOrWhiteSpace(schemaLocationValue))
        {
            results.Add(Info("No xsi:schemaLocation found in the document. Online schema loading cannot run.", SeverityType.Warning));
            return new List<SchemaLocation>();
        }

        var tokens = schemaLocationValue
            .Split((char[])null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tokens.Length % 2 != 0)
        {
            results.Add(Info("xsi:schemaLocation is malformed (must contain pairs: 'namespace schemaUrl').", SeverityType.Error));
        }

        var list = new List<SchemaLocation>();

        for (var i = 0; i + 1 < tokens.Length; i += 2)
        {
            var ns = tokens[i];
            var url = tokens[i + 1];

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                results.Add(Info($"Schema URL is not absolute or invalid: '{url}' (namespace '{ns}').", SeverityType.Error));
                continue;
            }

            list.Add(new SchemaLocation(ns, uri));
        }

        return list;
    }

    private async Task PreflightSchemasAsync(List<SchemaLocation> schemaLocations, List<ValidatorResult> results)
    {
        if (schemaLocations.Count == 0)
        {
            return;
        }

        using var client = _httpClientFactory.CreateClient();

        foreach (var sl in schemaLocations)
        {
            var (ok, info) = await TryFetchSchemaInfoAsync(client, sl);
            results.Add(info);

            if (!ok)
            {
                // Nothing else to do here – validation step will also emit "Cannot load schema..." warnings.
            }
        }
    }

    private async Task<(bool ok, ValidatorResult info)> TryFetchSchemaInfoAsync(HttpClient client, SchemaLocation sl)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, sl.SchemaUri);
            request.Headers.UserAgent.ParseAdd("ValidatorService/1.0");

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                return (false, Info($"Schema not reachable: '{sl.SchemaUri}' for namespace '{sl.NamespaceUri}'. ({(int)response.StatusCode} {response.ReasonPhrase})", SeverityType.Warning));
            }

            await using var stream = await response.Content.ReadAsStreamAsync();

            // Read schema to extract targetNamespace/version when available
            using var xr = XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit
            }, sl.SchemaUri.ToString());

            var schema = XmlSchema.Read(xr, null);

            var targetNs = !string.IsNullOrWhiteSpace(schema?.TargetNamespace) ? schema!.TargetNamespace! : sl.NamespaceUri;
            var version = !string.IsNullOrWhiteSpace(schema?.Version) ? schema!.Version : InferVersionFromUrl(sl.SchemaUri.ToString());
            var label = GetWellKnownLabel(targetNs);

            if (!string.IsNullOrWhiteSpace(version))
            {
                return (true, Info($"Schema reachable: {label} namespace='{targetNs}', version='{version}', url='{sl.SchemaUri}'.", SeverityType.Information));
            }

            return (true, Info($"Schema reachable: {label} namespace='{targetNs}', url='{sl.SchemaUri}' (version not declared/inferable).", SeverityType.Information));
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is XmlException || ex is XmlSchemaException)
        {
            return (false, Info($"Schema not reachable: '{sl.SchemaUri}' for namespace '{sl.NamespaceUri}'. ({ex.Message})", SeverityType.Error));
        }
    }

    private void ValidateWithXmlReader(XmlDocument document, List<ValidatorResult> results)
    {
        var resolver = new HttpClientXmlResolver(_httpClientFactory);

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            ValidationType = ValidationType.Schema,
            ValidationFlags =
                XmlSchemaValidationFlags.ProcessSchemaLocation |
                XmlSchemaValidationFlags.ProcessInlineSchema |
                XmlSchemaValidationFlags.ReportValidationWarnings,
            XmlResolver = resolver
        };

        settings.ValidationEventHandler += (_, e) =>
        {
            results.Add(Xml(e.Message, e.Severity, e.Exception));
        };

        try
        {
            using var sr = new StringReader(document.OuterXml);
            using var reader = XmlReader.Create(sr, settings);

            while (reader.Read())
            {
            }
        }
        catch (Exception ex)
        {
            // Keep the pattern: return structured result instead of throwing
            results.Add(Xml("FAILED: " + ex.Message, XmlSeverityType.Error, ex as XmlSchemaException));
        }
    }

    private static string InferVersionFromUrl(string schemaUrl)
    {
        var file = Path.GetFileName(schemaUrl);
        if (string.IsNullOrWhiteSpace(file))
        {
            return null;
        }

        // MODS common pattern: mods-3-7.xsd => 3.7
        var idx = file.IndexOf("mods-", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var tail = file[(idx + "mods-".Length)..]; // .NET 8 ok
            var dot = tail.IndexOf(".xsd", StringComparison.OrdinalIgnoreCase);
            if (dot > 0)
            {
                tail = tail[..dot];
            }

            var parts = tail.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 2 && int.TryParse(parts[0], out _) && int.TryParse(parts[1], out _))
            {
                return $"{parts[0]}.{parts[1]}";
            }
        }

        return null;
    }

    private static string GetWellKnownLabel(string ns)
    {
        if (string.Equals(ns, "ExtensionMETS", StringComparison.OrdinalIgnoreCase))
        {
            return "ExtensionMETS";
        }

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

    private static ValidatorResult Info(string message, SeverityType severity)
    {
        return new ValidatorResult
        {
            Severity = severity,
            Information = message,
            XmlReslut = null
        };
    }

    private static ValidatorResult Xml(string message, XmlSeverityType severity, XmlSchemaException exception)
    {
        return new ValidatorResult
        {
            Severity = (SeverityType)severity,
            Information = null,
            XmlReslut = new XmlValidatorResult(message, severity, exception)
        };
    }

    private sealed record SchemaLocation(string NamespaceUri, Uri SchemaUri);

    private sealed class HttpClientXmlResolver : XmlResolver
    {
        private readonly IHttpClientFactory _factory;

        public HttpClientXmlResolver(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public override ICredentials Credentials
        {
            set { }
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (absoluteUri == null)
            {
                throw new ArgumentNullException(nameof(absoluteUri));
            }

            if (!string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new XmlException($"Only http/https schemas are allowed. Uri: '{absoluteUri}'.");
            }

            using var client = _factory.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, absoluteUri);
            request.Headers.UserAgent.ParseAdd("ValidatorService/1.0");

            using var response = client.Send(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            return new MemoryStream(bytes);
        }
    }
}