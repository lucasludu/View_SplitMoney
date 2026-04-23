using SplitMoney.Client.Services;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace SplitMoney.Client.Infrastructure
{
    public class RefreshTokenHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public RefreshTokenHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized && !request.RequestUri!.AbsolutePath.Contains("/api/v1/Auth/"))
                {
                    var authService = _serviceProvider.GetRequiredService<IAuthService>();
                    var newToken = await authService.RefreshToken();

                    if (!string.IsNullOrEmpty(newToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("bearer", newToken);
                        response = await base.SendAsync(request, cancellationToken);
                    }
                    else
                    {
                        await HandleSessionExpired();
                    }
                }

                return response;
            }
            catch (HttpRequestException)
            {
                // El servidor no responde (API apagada)
                await HandleConnectionError();
                throw;
            }
        }

        private async Task HandleSessionExpired()
        {
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            await authService.Logout();
            var toastService = _serviceProvider.GetRequiredService<IToastService>();
            var navigationManager = _serviceProvider.GetRequiredService<NavigationManager>();

            toastService.ShowToast("Tu sesión ha expirado por inactividad.", ToastLevel.Warning);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Task.Delay(100);
                navigationManager.NavigateTo("session-expired");
            });
        }

        private async Task HandleConnectionError()
        {
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            await authService.Logout();
            var navigationManager = _serviceProvider.GetRequiredService<NavigationManager>();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                navigationManager.NavigateTo("login");
            });
        }
    }
}
