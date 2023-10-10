using PheasantTails.TwiHigh.Data.Model.Tweets;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class TweetHttpClient
    {
        private readonly string _apiUrlBase;
        private readonly string _apiUrlTweet;
        private readonly string _apiUrlDeleteTweet;
        private readonly string _apiUrlGetTweet;
        private readonly string _apiUrlGetUserTweets;
        private readonly HttpClient _httpClient;

        public TweetHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrlBase = $"{configuration["TweetApiUrl"]}";
            _apiUrlTweet = $"{_apiUrlBase}/";
            _apiUrlDeleteTweet = $"{_apiUrlBase}/{{0}}";
            _apiUrlGetTweet = $"{_apiUrlBase}/{{0}}";
            _apiUrlGetUserTweets = $"{_apiUrlBase}/user/{{0}}";
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
                return await _httpClient.PostAsJsonAsync(_apiUrlTweet, context);
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
                var url = string.Format(_apiUrlDeleteTweet, id.ToString());
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
                var url = string.Format(_apiUrlGetTweet, id.ToString());
                return await _httpClient.GetAsync(url);
            }
            catch (Exception)
            {
                return await Task.FromResult<HttpResponseMessage?>(default);
            }
        }

        public async Task<HttpResponseMessage?> GetUserTweetsAsync(Guid id)
        {
            try
            {
                var url = string.Format(_apiUrlGetUserTweets, id.ToString());
                return await _httpClient.GetAsync(url);
            }
            catch (Exception)
            {
                return await Task.FromResult<HttpResponseMessage?>(default);
            }
        }
    }
}
