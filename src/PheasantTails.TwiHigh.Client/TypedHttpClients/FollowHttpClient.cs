using System.Net.Http.Headers;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class FollowHttpClient
    {
        private readonly string _apiUrlBase;
        private readonly string _apiUrlAddFollow;
        private readonly string _apiUrlRemoveFollow;
        private readonly HttpClient _httpClient;

        public FollowHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrlBase = $"{configuration["FollowApiUrl"]}";
            _apiUrlAddFollow = $"{_apiUrlBase}/{{0}}";
            _apiUrlRemoveFollow = $"{_apiUrlBase}/{{0}}";
        }

        public void SetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public Task<HttpResponseMessage> PutNewFolloweeAsync(string followeeUserId)
        {
            var url = string.Format(_apiUrlAddFollow, followeeUserId);
            return _httpClient.PutAsync(url, null);
        }

        public Task<HttpResponseMessage> DeleteFolloweeAsync(string followeeUserId)
        {
            var url = string.Format(_apiUrlRemoveFollow, followeeUserId);
            return _httpClient.DeleteAsync(url);
        }
    }
}
