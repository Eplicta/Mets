namespace Eplicta.Mets.Extensions
{
	public class MicrosoftHttpClientFactory : IHttpClientFactory
	{
		private readonly System.Net.Http.IHttpClientFactory inner;

		public MicrosoftHttpClientFactory(System.Net.Http.IHttpClientFactory inner)
		{
			this.inner = inner;
		}

		public HttpClient CreateClient(string name) => inner.CreateClient(name);
	}
}
