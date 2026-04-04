using System.Net.Http.Headers;

namespace SplitMoney.Client.Infrastructure
{
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        public AuthenticationHeaderHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization == null)
            {
                var token = await SecureStorage.Default.GetAsync("authToken");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
