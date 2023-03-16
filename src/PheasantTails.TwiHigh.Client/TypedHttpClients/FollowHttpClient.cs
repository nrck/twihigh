using System.Net.Http.Headers;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class FollowHttpClient
    {
        private const string API_URL_BASE = "https://twihigh-dev-apim.azure-api.net/follows";
        private const string API_URL_ADD_FOLLOW = $"{API_URL_BASE}/{{0}}";
        private readonly HttpClient _httpClient;

        public FollowHttpClient(HttpClient httpClient)
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

        public Task<HttpResponseMessage> PutNewFollowee(string followeeUserId)
        {
            var url = string.Format(API_URL_ADD_FOLLOW, followeeUserId);
            return _httpClient.PutAsync(url, null);
        }
    }
}
