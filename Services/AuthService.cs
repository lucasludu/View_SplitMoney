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
        private readonly ICacheService _cacheService;

        public AuthService(HttpClient httpClient,
                           AuthenticationStateProvider authenticationStateProvider,
                           Blazored.LocalStorage.ILocalStorageService localStorage,
                           ICacheService cacheService)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
            _cacheService = cacheService;
        }

        public async Task<ApiResult<LoginResponse>> Login(LoginRequest loginRequest)
        {
            try 
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/login", loginRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<LoginResponse>>();
                    if (result != null && result.Succeeded)
                    {
                        await SecureStorage.Default.SetAsync("authToken", result.Data!.Token);
                        await SecureStorage.Default.SetAsync("refreshToken", result.Data.RefreshToken);

                        ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(result.Data.Token);

                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Data.Token);
                        return result;
                    }
                    return result ?? new ApiResult<LoginResponse> { Succeeded = false, Message = "Respuesta vacía del servidor." };
                }
                else 
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResult<LoginResponse>>();
                    return errorResult ?? new ApiResult<LoginResponse> { Succeeded = false, Message = $"Error del servidor: {response.StatusCode}" };
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<LoginResponse> { Succeeded = false, Message = $"Error de conexión: {ex.Message}" };
            }
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
                
                // Clear any simulated premium state and cache on logout
                try
                {
                    await _localStorage.RemoveItemAsync("is_simulated_premium");
                    await _cacheService.ClearAllAsync();
                }
                catch (InvalidOperationException)
                {
                    // Cannot invoke JS outside of a WebView context
                }

                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();

                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ApiResult<string>> Register(RegisterUserRequest registerRequest)
        {
            try 
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/register", registerRequest);
                var result = await response.Content.ReadFromJsonAsync<ApiResult<string>>();
                return result ?? new ApiResult<string> { Succeeded = false, Message = "Error al procesar el registro." };
            }
            catch (Exception ex)
            {
                return new ApiResult<string> { Succeeded = false, Message = $"Error de red: {ex.Message}" };
            }
        }

        public async Task<string> RefreshToken()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("authToken");
                var refreshToken = await SecureStorage.Default.GetAsync("refreshToken");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                {
                    await Logout();
                    return string.Empty;
                }

                var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/refresh-token", new RefreshTokenRequest { Token = token, RefreshToken = refreshToken });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<LoginResponse>>();
                    if (result != null && result.Succeeded)
                    {
                        await SecureStorage.Default.SetAsync("authToken", result.Data!.Token);
                        await SecureStorage.Default.SetAsync("refreshToken", result.Data.RefreshToken);

                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Data.Token);

                        return result.Data.Token;
                    }
                }
            }
            catch (Exception)
            {
                // If there's an error in the process or deserialization, we fail gracefully
            }

            await Logout();
            return string.Empty;
        }
        public async Task<ApiResult<UserDto>> GetProfile()
        {
            try 
            {
                var response = await _httpClient.GetAsync("api/v1/User/me");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResult<UserDto>>();
                    return result ?? new ApiResult<UserDto> { Succeeded = false, Message = "Perfil no encontrado." };
                }
                return new ApiResult<UserDto> { Succeeded = false, Message = "Error al obtener el perfil." };
            }
            catch (Exception ex)
            {
                return new ApiResult<UserDto> { Succeeded = false, Message = $"Error de red: {ex.Message}" };
            }
        }

        public async Task<ApiResult<string>> UpdateProfile(UserDto userUpdate)
        {
            try 
            {
                // NOTA: El endpoint api/v1/Auth/profile no existe en la API actual.
                // Se intenta usar api/v1/User/me si fuera un PUT para actualizar, 
                // o se deja la ruta actual por si se añade en el futuro.
                var response = await _httpClient.PutAsJsonAsync("api/v1/User/me", userUpdate);
                var result = await response.Content.ReadFromJsonAsync<ApiResult<string>>();
                return result ?? new ApiResult<string> { Succeeded = false, Message = "Error al actualizar el perfil." };
            }
            catch (Exception ex)
            {
                return new ApiResult<string> { Succeeded = false, Message = $"Error de red: {ex.Message}" };
            }
        }

        public async Task<bool> IsPremiumAsync()
        {
            try
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                
                // Real check
                if (user.Identity?.IsAuthenticated == true && user.IsInRole("PremiumUser"))
                    return true;

                // Simulation check
                return await _localStorage.GetItemAsync<bool>("is_simulated_premium");
            }
            catch
            {
                return false;
            }
        }

        public async Task SimulatePremiumAsync()
        {
            try
            {
                await _localStorage.SetItemAsync("is_simulated_premium", true);
            }
            catch
            {
                // Ignore JS errors
            }
        }
        public async Task<ApiResult<string>> ForgotPassword(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/forgot-password", new ForgotPasswordRequest { Email = email });
                var result = await response.Content.ReadFromJsonAsync<ApiResult<string>>();
                return result ?? new ApiResult<string> { Succeeded = false, Message = "Error al procesar la solicitud." };
            }
            catch (Exception ex)
            {
                return new ApiResult<string> { Succeeded = false, Message = $"Error de red: {ex.Message}" };
            }
        }
    
        public async Task<ApiResult<string>> ResetPassword(ResetPasswordRequest resetRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/reset-password", resetRequest);
                var result = await response.Content.ReadFromJsonAsync<ApiResult<string>>();
                return result ?? new ApiResult<string> { Succeeded = false, Message = "Error al restablecer la contraseña." };
            }
            catch (Exception ex)
            {
                return new ApiResult<string> { Succeeded = false, Message = $"Error de red: {ex.Message}" };
            }
        }
    }
}
