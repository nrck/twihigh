using PheasantTails.TwiHigh.Model.Timelines;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class TimelineHttpClient
    {
        private const string API_URL_TIMELINE = "http://localhost:5001/api/GetMyTimeline";
        private readonly HttpClient _httpClient;

        public TimelineHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6ImRlOTJiNGRkLWU3ZDEtNDRkYy1hZDE2LTdkM2Y3OGE3NWYzYSIsImRpc3BsYXlJZCI6Im5yX2NrIiwiZGlzcGxheU5hbWUiOiLjgZEifQ.EsKtX9J5EzQDQ0s4nusU4XG7QBdRqVlztcYwnGfFUDc");
        }

        public Task<ResponseTimelineContext?> GetMyTimelineAsync()
        {
            try
            {
                return _httpClient.GetFromJsonAsync<ResponseTimelineContext>(API_URL_TIMELINE);
            }
            catch (Exception)
            {
                return Task.FromResult<ResponseTimelineContext?>(new ResponseTimelineContext());
            }
        }
    }
}
