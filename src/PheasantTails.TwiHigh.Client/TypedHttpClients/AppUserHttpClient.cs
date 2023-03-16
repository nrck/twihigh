﻿using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.TypedHttpClients
{
    public class AppUserHttpClient
    {
        //private const string API_URL_BASE = "http://localhost:5002/api";
        private const string API_URL_BASE = "https://twihigh-dev-apim.azure-api.net/twihighusers";
        private const string API_URL_LOGIN = $"{API_URL_BASE}/Login";
        private const string API_URL_REFRESH = $"{API_URL_BASE}/Refresh";
        private const string API_URL_SIGNIN = $"{API_URL_BASE}/SignUp";
        private const string API_URL_GET_TWIHIGH_USER = $"{API_URL_BASE}/TwiHighUser/{{0}}";
        private readonly HttpClient _httpClient;

        public AppUserHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResponseJwtContext?> LoginAsync(PostAuthorizationContext authorizationContext)
        {
            try
            {
                var res = await _httpClient.PostAsJsonAsync(API_URL_LOGIN, authorizationContext);
                return await res.Content.ReadFromJsonAsync<ResponseJwtContext>();
            }
            catch (Exception)
            {
                return await Task.FromResult<ResponseJwtContext?>(new ResponseJwtContext { Token = string.Empty });
            }
        }

        public async Task<ResponseJwtContext?> RefreshAsync(string token)
        {
            try
            {
                var mes = new HttpRequestMessage(HttpMethod.Get, API_URL_REFRESH);
                mes.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                var res = await _httpClient.SendAsync(mes);
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
                var res = await _httpClient.PostAsJsonAsync(API_URL_SIGNIN, context);
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
                var url = string.Format(API_URL_GET_TWIHIGH_USER, id);
                return await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext>(url);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
