using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class AppUserHttpClient
    {
        private readonly string _apiUrlBase;
        private readonly string _apiUrlLogin;
        private readonly string _apiUrlRefresh;
        private readonly string _apiUrlSignin;
        private readonly string _apiUrlPatchTwihighUser;
        private readonly string _apiUrlGetTwihighUser;
        private readonly string _apiUrlGetTwihighUserFollows;
        private readonly string _apiUrlGetTwihighUserFollowes;
        private readonly HttpClient _httpClient;

        public AppUserHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrlBase = $"{configuration["AppUserApiUrl"]}";
            _apiUrlLogin = $"{_apiUrlBase}/Login";
            _apiUrlRefresh = $"{_apiUrlBase}/Refresh";
            _apiUrlSignin = $"{_apiUrlBase}/SignUp";
            _apiUrlPatchTwihighUser = $"{_apiUrlBase}/TwiHighUser/";
            _apiUrlGetTwihighUser = $"{_apiUrlBase}/TwiHighUser/{{0}}";
            _apiUrlGetTwihighUserFollows = $"{_apiUrlBase}/TwiHighUser/{{0}}/Follows";
            _apiUrlGetTwihighUserFollowes = $"{_apiUrlBase}/TwiHighUser/{{0}}/Followers";
        }

        public void SetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ResponseJwtContext?> LoginAsync(PostAuthorizationContext authorizationContext)
        {
            try
            {
                var res = await _httpClient.PostAsJsonAsync(_apiUrlLogin, authorizationContext);
                return await res.Content.ReadFromJsonAsync<ResponseJwtContext>();
            }
            catch (Exception)
            {
                return await Task.FromResult<ResponseJwtContext?>(new ResponseJwtContext { Token = string.Empty });
            }
        }

        public async Task<ResponseJwtContext?> RefreshAsync()
        {
            try
            {
                var res = await _httpClient.GetAsync(_apiUrlRefresh);
                res.EnsureSuccessStatusCode();

                return await res.Content.ReadFromJsonAsync<ResponseJwtContext>();
            }
            catch (Exception)
            {
                return await Task.FromResult<ResponseJwtContext?>(new ResponseJwtContext { Token = string.Empty });
            }
        }

        public async Task<ResponseJwtContext?> SignUpAsync(AddTwiHighUserContext context)
        {
            try
            {
                var res = await _httpClient.PostAsJsonAsync(_apiUrlSignin, context);
                res.EnsureSuccessStatusCode();
                var login = new PostAuthorizationContext
                {
                    DisplayId = context.DisplayId,
                    PlanePassword = context.Password
                };

                return await LoginAsync(login);
            }
            catch (Exception)
            {
                return await Task.FromResult<ResponseJwtContext?>(new ResponseJwtContext { Token = string.Empty });
            }
        }

        public async Task<ResponseTwiHighUserContext?> GetTwiHighUserAsync(string id)
        {
            try
            {
                var url = string.Format(_apiUrlGetTwihighUser, id);
                return await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext>(url);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<ResponseTwiHighUserContext[]?> GetTwiHighUserFollowsAsync(string id)
        {
            try
            {
                var url = string.Format(_apiUrlGetTwihighUserFollows, id);
                return await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext[]>(url);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ResponseTwiHighUserContext[]?> GetTwiHighUserFollowersAsync(string id)
        {
            try
            {
                var url = string.Format(_apiUrlGetTwihighUserFollowes, id);
                return await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext[]>(url);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ResponseTwiHighUserContext?> PatchTwiHighUserAsync(PatchTwiHighUserContext patchTwiHighUserContext)
        {
            try
            {
                var res = await _httpClient.PatchAsync(_apiUrlPatchTwihighUser, JsonContent.Create(patchTwiHighUserContext));
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadFromJsonAsync<ResponseTwiHighUserContext>();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
