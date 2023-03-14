using PheasantTails.TwiHigh.Data.Model.Timelines;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class TimelineHttpClient
    {
        private const string API_URL_BASE = "https://twihigh-dev-apim.azure-api.net/timelines";
        private const string API_URL_TIMELINE = $"{API_URL_BASE}/GetMyTimeline";
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
            try
            {
                return await _httpClient.GetFromJsonAsync<ResponseTimelineContext>(API_URL_TIMELINE);
            }
            catch (Exception)
            {
                return await Task.FromResult<ResponseTimelineContext?>(new ResponseTimelineContext());
            }
        }
    }
}
