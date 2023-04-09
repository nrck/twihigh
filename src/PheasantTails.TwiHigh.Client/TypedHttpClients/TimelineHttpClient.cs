using PheasantTails.TwiHigh.Data.Model.Timelines;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class TimelineHttpClient
    {
        private const string API_URL_BASE = "https://twihigh-dev-apim.azure-api.net/timelines";
        private const string API_URL_TIMELINE = $"{API_URL_BASE}/GetMyTimeline";
        private const string API_URL_TIMELINE_V2 = $"{API_URL_BASE}/timeline?since={{0}}&until={{1}}";
        private readonly HttpClient _httpClient;

        public TimelineHttpClient(HttpClient httpClient)
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

        public async Task<ResponseTimelineContext?> GetMyTimelineAsync()
        {
            return await _httpClient.GetFromJsonAsync<ResponseTimelineContext>(API_URL_TIMELINE);
        }

        public async Task<ResponseTimelineContext?> GetMyTimelineAsync(DateTimeOffset since, DateTimeOffset until)
        {
            if (until < since)
            {
                throw new ArgumentException("開始時刻と終了時刻が逆転しています。", nameof(since));
            }

            var url = string.Format(API_URL_TIMELINE_V2, HttpUtility.UrlEncode(since.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")), HttpUtility.UrlEncode(until.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")));
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<ResponseTimelineContext>();
            }

            response.EnsureSuccessStatusCode();
            var context = new ResponseTimelineContext { Latest = DateTimeOffset.UtcNow, Oldest = DateTimeOffset.UtcNow };
            return context;
        }
    }
}
