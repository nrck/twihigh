using PheasantTails.TwiHigh.Client.Extensions;
using PheasantTails.TwiHigh.Data.Model.Feeds;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class FeedHttpClient
    {
        private readonly string _apiUrlBase;
        private readonly string _apiUrlGetMyFeeds;
        private readonly string _apiUrlPutOpenedMyFeeds;
        private readonly HttpClient _httpClient;

        public FeedHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrlBase = $"{configuration["FeedApiUrl"]}";
            _apiUrlGetMyFeeds = $"{_apiUrlBase}/?since={{0}}&until={{1}}";
            _apiUrlPutOpenedMyFeeds = $"{_apiUrlBase}/";
        }

        public void SetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ResponseFeedsContext?> GetMyFeedsAsync(DateTimeOffset since, DateTimeOffset until)
        {
            if (until < since)
            {
                throw new ArgumentException("開始時刻と終了時刻が逆転しています。", nameof(since));
            }

            var url = string.Format(_apiUrlGetMyFeeds, HttpUtility.UrlEncode(since.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")), HttpUtility.UrlEncode(until.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")));
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.TwiHighReadFromJsonAsync<ResponseFeedsContext>();
            }

            response.EnsureSuccessStatusCode();
            return null;
        }

        public Task PutOpenedMyFeeds(PutUpdateMyFeedsContext context)
        {
            return _httpClient.PutAsJsonAsync(_apiUrlPutOpenedMyFeeds, context);
        }
    }
}
