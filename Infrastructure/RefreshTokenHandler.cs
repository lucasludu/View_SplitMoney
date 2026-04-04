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
                    await authService.Logout();
                    var toastService = _serviceProvider.GetRequiredService<IToastService>();
                    var navigationManager = _serviceProvider.GetRequiredService<NavigationManager>();

                    toastService.ShowToast("Tu sesión ha expirado por inactividad.", ToastLevel.Warning);
                    navigationManager.NavigateTo("session-expired");
                }
            }

            return response;
        }
    }
}
