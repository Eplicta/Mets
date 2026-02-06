using System.Net.Http;

namespace Eplicta.Mets.Console;

internal sealed class DefaultHttpClientFactory : IHttpClientFactory
{
    private static readonly HttpClient _client = new();

    public HttpClient CreateClient(string name)
    {
        return _client;
    }
}