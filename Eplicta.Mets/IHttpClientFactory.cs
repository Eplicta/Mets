using System.Net.Http;

namespace Eplicta.Mets;

public interface IHttpClientFactory
{
    HttpClient CreateClient(string name);
    public HttpClient Create() => CreateClient("");
}
