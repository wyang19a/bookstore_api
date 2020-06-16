using Blazored.LocalStorage;
using BookStore_UI_server.Contracts;
using BookStore_UI_server.Models;
using BookStore_UI_server.Providers;
using BookStore_UI_server.Static;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BookStore_UI_server.Service
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IHttpClientFactory _client;
        private readonly ILocalStorageService _store;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        public AuthenticationRepository(IHttpClientFactory client,
            ILocalStorageService store,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _client = client;
            _store = store;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<bool> Login(LoginModel user)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
               Endpoints.LoginEndpoint);
            request.Content = new StringContent(JsonConvert.SerializeObject(user),
                Encoding.UTF8, "application/json");

            var client = _client.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<TokenResponse>(content);

            // Storing Token
            await _store.SetItemAsync("authToken", token.Token);

            // Change Authentication state
            await ((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedIn();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token.Token);

            return true;
        }

        public async Task Logout()
        {
            await _store.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedOut();
        }

        public async Task<bool> Register(RegistrationModel user)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                Endpoints.RegisterEndpoint);
            request.Content = new StringContent(JsonConvert.SerializeObject(user),
                Encoding.UTF8, "application/json");

            var client = _client.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
