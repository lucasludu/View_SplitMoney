using Microsoft.AspNetCore.Components.Authorization;
using SplitMoney.Client.Infrastructure;
using SplitMoney.Client.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SplitMoney.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly Blazored.LocalStorage.ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient,
                           AuthenticationStateProvider authenticationStateProvider,
                           Blazored.LocalStorage.ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }

        public async Task<Response<LoginResponse>> Login(LoginRequest loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/login", loginRequest);
            var result = await response.Content.ReadFromJsonAsync<Response<LoginResponse>>();

            if (response.IsSuccessStatusCode && result!.Succeeded)
            {
                await SecureStorage.Default.SetAsync("authToken", result.Data.Token);
                await SecureStorage.Default.SetAsync("refreshToken", result.Data.RefreshToken);

                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(result.Data.Token);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Data.Token);
            }

            return result!;
        }

        public async Task Logout()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("authToken");
                var refreshToken = await SecureStorage.Default.GetAsync("refreshToken");

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _httpClient.PostAsJsonAsync("api/v1/Auth/logout", new RefreshTokenRequest { Token = token, RefreshToken = refreshToken });
                }
            }
            catch
            {
                // Silence logout errors to ensure client-side logout completes
            }
            finally
            {
                SecureStorage.Default.Remove("authToken");
                SecureStorage.Default.Remove("refreshToken");
                
                // Clear any simulated premium state on logout
                await _localStorage.RemoveItemAsync("is_simulated_premium");

                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();

                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<Response<string>> Register(RegisterUserRequest registerRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/register", registerRequest);
            var result = await response.Content.ReadFromJsonAsync<Response<string>>();
            return result!;
        }

        public async Task<string> RefreshToken()
        {
            var token = await SecureStorage.Default.GetAsync("authToken");
            var refreshToken = await SecureStorage.Default.GetAsync("refreshToken");

            var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/refresh-token", new RefreshTokenRequest { Token = token, RefreshToken = refreshToken });
            var result = await response.Content.ReadFromJsonAsync<Response<LoginResponse>>();

            if (response.IsSuccessStatusCode && result!.Succeeded)
            {
                await SecureStorage.Default.SetAsync("authToken", result.Data!.Token);
                await SecureStorage.Default.SetAsync("refreshToken", result.Data.RefreshToken);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Data.Token);

                return result.Data.Token;
            }

            return string.Empty;
        }
        public async Task<Response<UserDto>> GetProfile()
        {
            var response = await _httpClient.GetAsync("api/v1/User/me");
            var result = await response.Content.ReadFromJsonAsync<Response<UserDto>>();
            return result!;
        }

        public async Task<Response<string>> UpdateProfile(UserDto userUpdate)
        {
            var response = await _httpClient.PutAsJsonAsync("api/v1/Auth/profile", userUpdate);
            var result = await response.Content.ReadFromJsonAsync<Response<string>>();
            return result!;
        }
    }
}
