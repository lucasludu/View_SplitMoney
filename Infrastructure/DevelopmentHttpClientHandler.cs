namespace SplitMoney.Client.Infrastructure
{
    public class DevelopmentHttpClientHandler : HttpClientHandler
    {
        public DevelopmentHttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                // En desarrollo, permitimos certificados auto-firmados o con errores de nombre
                // Esto es crucial para emuladores Android conectando a 10.0.2.2 (localhost)
                #if DEBUG
                return true; 
                #else
                if (cert != null && (cert.Issuer.Contains("localhost") || cert.Subject.Contains("localhost") || cert.Issuer.Contains("CN=localhost")))
                    return true;

                return errors == SslPolicyErrors.None;
                #endif
            };
        }
    }
}
