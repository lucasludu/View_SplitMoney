using SplitMoney.Client.Services;
using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Sockets;

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
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                
                // Si el servidor responde con un error 500+ pero NO es un error de autorización (que maneja el RefreshTokenHandler)
                if (((int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.ServiceUnavailable))
                {
                    var toastService = _serviceProvider.GetService<IToastService>();
                    toastService?.ShowToast("El servidor encontró un error interno. Intente nuevamente más tarde.", ToastLevel.Error);
                    
                    // No cerramos sesión automáticamente en errores 500 para evitar expulsar al usuario por un bug puntual
                }

                return response;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is SocketException)
            {
                if (ex is TaskCanceledException && cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                var toastService = _serviceProvider.GetService<IToastService>();
                toastService?.ShowToast("No se pudo establecer conexión con el servidor. Verifique su internet.", ToastLevel.Error);
                
                // Solo redirigimos a error si el usuario ya estaba autenticado y es un error crítico de conexión
                // pero NO cerramos sesión automáticamente.
                
                throw;
            }
        }
    }
}
