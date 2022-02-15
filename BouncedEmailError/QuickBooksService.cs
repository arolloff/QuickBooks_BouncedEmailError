using System.Net;
using System.Web;

using Newtonsoft.Json.Linq;

namespace BouncedEmailError
{
    internal class QuickBooksService
    {
        // TODO: Set access token, company id
        private const string AccessToken = "";
        private const string CompanyId = "";
        private const string ContentType_Json = "application/json";

        private readonly HttpClient _httpClient;

        internal QuickBooksService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (string.IsNullOrWhiteSpace(AccessToken))
                throw new InvalidOperationException("Access Token is required");
            if (string.IsNullOrWhiteSpace(CompanyId))
                throw new InvalidOperationException("Company ID is required");
        }

        public async Task<int> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                return 1;

            if (!int.TryParse(invoiceNumber.Trim(), out var invoiceNumberInt))
                return 1;

            _ = await GetInvoiceQueryResponse(invoiceNumberInt);
            return 0;
        }

        private async Task<InvoiceQueryResponse?> GetInvoiceQueryResponse(int invoiceNumber)
        {
            var httpRequestMessage = CreateQueryMessage(invoiceNumber);
            var result = await ExecuteHttpRequestAsync(httpRequestMessage);

            return result?.QueryResponse?.Invoice?.Any() == true
                ? result.QueryResponse
                : default;
        }

        private static HttpRequestMessage CreateQueryMessage(int invoiceNumber)
        {
            var query = $"select * from Invoice where docNumber = '{HttpUtility.UrlEncode(invoiceNumber.ToString())}'";
            var url = $"https://quickbooks.api.intuit.com/v3/company/{CompanyId}/query?query={query}&minorversion=63";
            var uri = new Uri(url);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri
            };

            httpRequestMessage.Headers.Add(HttpRequestHeader.Accept.ToString(), ContentType_Json);
            httpRequestMessage.Headers.Add(HttpRequestHeader.Authorization.ToString(), $"bearer {AccessToken}");
            httpRequestMessage.Headers.Add(HttpRequestHeader.ContentType.ToString(), ContentType_Json);

            return httpRequestMessage;
        }

        private async Task<GetInvoiceResult?> ExecuteHttpRequestAsync(HttpRequestMessage request)
        {
            var responseMessage = await _httpClient.SendAsync(request);
            if (responseMessage == default)
                return default;

            var jsonObject = await ParseHttpResponseMessageAsync(responseMessage);
            return jsonObject.ToObject<GetInvoiceResult>();
        }

        private static async Task<JObject> ParseHttpResponseMessageAsync(HttpResponseMessage responseMessage)
        {
            if (responseMessage?.IsSuccessStatusCode != true || responseMessage?.Content == default)
                return new JObject();

            var content = await responseMessage.Content.ReadAsStreamAsync();
            if (content == null)
                return new JObject();

            var rawJson = await new StreamReader(content).ReadToEndAsync();
            return JObject.Parse(rawJson);
        }
    }

    internal class GetInvoiceResult
    {
        public InvoiceQueryResponse? QueryResponse { get; set; }
    }

    internal class InvoiceQueryResponse : Intuit.Ipp.Data.QueryResponse
    {
        public IEnumerable<Intuit.Ipp.Data.Invoice>? Invoice { get; set; }
    }
}
