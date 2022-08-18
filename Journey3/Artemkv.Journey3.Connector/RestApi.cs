using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Artemkv.Journey3.Connector
{
    public class RestApi : IRestApi
    {
        private static readonly Lazy<HttpClient> HttpClient = new Lazy<HttpClient>(() =>
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TIMEOUT;
            return client;
        });

        private static readonly string JOURNEY_BASE_URL = "https://journey3-ingest.artemkv.net:8060";
        private static readonly TimeSpan TIMEOUT = new TimeSpan(0, 0, 30);

        public async Task PostSessionHeaderAsync(SessionHeader header)
        {
            var url = new Uri($"{JOURNEY_BASE_URL}/session_head");
            var content = CreateContent(header);
            var response = await HttpClient.Value.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                var node = JsonNode.Parse(errorResponse);
                throw new HttpRequestException(
                    $"Error sending session header to Journey: POST returned {(int)response.StatusCode} {response.ReasonPhrase}: {node["err"] ?? ""}");
            }
        }

        public async Task PostSessionAsync(Session session)
        {
            var url = new Uri($"{JOURNEY_BASE_URL}/session_tail");
            var content = CreateContent(session);
            var response = await HttpClient.Value.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                var node = JsonNode.Parse(errorResponse);
                throw new HttpRequestException(
                    $"Error sending session to Journey: POST returned {(int)response.StatusCode} {response.ReasonPhrase}: {node["err"] ?? ""}");
            }
        }

        private static HttpContent CreateContent(object obj)
        {
            var bodyText = JsonConvert.SerializeObject(obj);
            var content = new StringContent(bodyText, Encoding.UTF8, "application/json");
            content.Headers.Remove("Content-Type");
            content.Headers.Add("Content-Type", "application/json");
            return content;
        }
    }
}
