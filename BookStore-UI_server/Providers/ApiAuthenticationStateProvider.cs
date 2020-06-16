using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookStore_UI_server.Providers
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _store;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        public ApiAuthenticationStateProvider(
                ILocalStorageService store
              , JwtSecurityTokenHandler tokenHandler
            )
        {
            _store = store;
            _tokenHandler = tokenHandler;
        }
        // checks for authentication state of user.
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // using token here
                var storedToken = await _store.GetItemAsync<string>("authToken");
                if (string.IsNullOrWhiteSpace(storedToken))
                {
                    return new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity()));
;               }
                var tokenContent = _tokenHandler.ReadJwtToken(storedToken);
                var expiry = tokenContent.ValidTo;
                // remove token if session is expired.
                if(expiry < DateTime.Now)
                {
                    await _store.RemoveItemAsync("authToken");
                    return new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Get claims from token and Build Authenticated user object
                var claims = ParseClaims(tokenContent);
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                // return authenticated person
                return new AuthenticationState(user);
            }
            catch (Exception)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task LoggedIn()
        {
            var storedToken = await _store.GetItemAsync<string>("authToken");
            var tokenContent = _tokenHandler.ReadJwtToken(storedToken);
            var claims = ParseClaims(tokenContent);
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authState = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authState);
        }

        public void LoggedOut()
        {
            var nobody = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(nobody));
            NotifyAuthenticationStateChanged(authState);
        }

        private IList<Claim> ParseClaims(JwtSecurityToken tokenContent)
        {
            var claims = tokenContent.Claims.ToList();
            claims.Add(new Claim(ClaimTypes.Name, tokenContent.Subject));
            return claims;
        } 
    }
}
