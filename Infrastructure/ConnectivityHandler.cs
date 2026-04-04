using SplitMoney.Client.Services;
using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace SplitMoney.Client.Infrastructure
{
    public class ConnectivityHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public ConnectivityHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Avoid handling if the request is for authentication (login, register, logout)
            if (request.RequestUri!.AbsolutePath.Contains("/api/v1/Auth/"))
            {
                return await base.SendAsync(request, cancellationToken);
            }
            
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                if (ex is TaskCanceledException && cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                // Get current AuthState to see if we should actually Logout
                var authStateProvider = _serviceProvider.GetRequiredService<AuthenticationStateProvider>();
                var authState = await authStateProvider.GetAuthenticationStateAsync();
                
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    var authService = _serviceProvider.GetRequiredService<IAuthService>();
                    var toastService = _serviceProvider.GetRequiredService<IToastService>();
                    var navigationManager = _serviceProvider.GetRequiredService<NavigationManager>();

                    toastService.ShowToast("Se perdió la conexión con el servidor.", ToastLevel.Error);
                    await authService.Logout();
                    navigationManager.NavigateTo("server-error");
                }
                
                throw;
            }
        }
    }
}
