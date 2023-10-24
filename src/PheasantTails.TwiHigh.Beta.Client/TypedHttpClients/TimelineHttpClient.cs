using PheasantTails.TwiHigh.Beta.Client.Extensions;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using System.Net;
using System.Net.Http.Headers;
using System.Web;

namespace PheasantTails.TwiHigh.Beta.Client.TypedHttpClients
{
    public class TimelineHttpClient
    {
        private readonly string _apiUrlBase;
        private readonly string _apiUrlTimeline;
        private readonly HttpClient _httpClient;

        public TimelineHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrlBase = $"{configuration["TimelineApiUrl"]}";
            _apiUrlTimeline = $"{_apiUrlBase}/?since={{0}}&until={{1}}";
        }

        public void SetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ResponseTimelineContext?> GetMyTimelineAsync(DateTimeOffset since, DateTimeOffset until)
        {
            if (until < since)
            {
                throw new ArgumentException("開始時刻と終了時刻が逆転しています。", nameof(since));
            }

            var url = string.Format(_apiUrlTimeline, HttpUtility.UrlEncode(since.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")), HttpUtility.UrlEncode(until.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")));
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.TwiHighReadFromJsonAsync<ResponseTimelineContext>();
            }

            response.EnsureSuccessStatusCode();
            var context = new ResponseTimelineContext { Latest = DateTimeOffset.UtcNow, Oldest = DateTimeOffset.UtcNow };
            return context;
        }
    }
}
