using System.Net.Security;

namespace SplitMoney.Client.Infrastructure
{
    public class DevelopmentHttpClientHandler : HttpClientHandler
    {
        public DevelopmentHttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert != null && (cert.Issuer.Contains("localhost") || cert.Subject.Contains("localhost") || cert.Issuer.Contains("CN=localhost")))
                    return true;

                return errors == SslPolicyErrors.None;
            };
        }
    }
}
