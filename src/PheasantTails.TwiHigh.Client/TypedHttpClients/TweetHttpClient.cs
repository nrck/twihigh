using PheasantTails.TwiHigh.Model;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class TweetHttpClient
    {
        private const string API_URL_TWEET = "http://localhost:5003/api/Tweets";
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
    }
}
