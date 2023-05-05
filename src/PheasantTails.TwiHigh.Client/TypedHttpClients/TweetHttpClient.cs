using PheasantTails.TwiHigh.Data.Model;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class TweetHttpClient
    {
        private const string API_URL_BASE = "https://twihigh-dev-apim.azure-api.net/tweets";
        private const string API_URL_TWEET = $"{API_URL_BASE}/tweets";
        private const string API_URL_DELETE_TWEET = $"{API_URL_BASE}/tweets/{{0}}";
        private const string API_URL_GET_TWEET = $"{API_URL_BASE}/tweets/{{0}}";
        private readonly HttpClient _httpClient;

        public TweetHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<HttpResponseMessage?> PostTweetAsync(PostTweetContext context)
        {
            try
            {
                return await _httpClient.PostAsJsonAsync(API_URL_TWEET, context);
            }
            catch (Exception)
            {
                return await Task.FromResult<HttpResponseMessage?>(default);
            }
        }

        public async Task<HttpResponseMessage?> DeleteTweetAsync(Guid id)
        {
            try
            {
                var url = string.Format(API_URL_DELETE_TWEET, id.ToString());
                return await _httpClient.DeleteAsync(url);
            }
            catch (Exception)
            {
                return await Task.FromResult<HttpResponseMessage?>(default);
            }
        }

        public async Task<HttpResponseMessage?> GetTweetAsync(Guid id)
        {
            try
            {
                var url = string.Format(API_URL_GET_TWEET, id.ToString());
                return await _httpClient.GetAsync(url);
            }
            catch (Exception)
            {
                return await Task.FromResult<HttpResponseMessage?>(default);
            }
        }
    }
}
