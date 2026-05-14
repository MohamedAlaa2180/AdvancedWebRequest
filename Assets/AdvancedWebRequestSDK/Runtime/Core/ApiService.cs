using System;
using System.Threading;

namespace AdvancedWebRequest.Core
{
    /// <summary>
    /// Convenience wrapper that owns an ApiClient and a CancellationTokenSource.
    /// Compose this into your MonoBehaviour instead of managing the two separately.
    /// </summary>
    public class ApiService : IDisposable
    {
        public ApiClient Client { get; }
        public CancellationTokenSource Cts { get; } = new CancellationTokenSource();

        public ApiService(string baseUrl, ITokenProvider tokenProvider = null, ILogger logger = null)
        {
            Client = new ApiClient(ApiClientConfig.Create(baseUrl), tokenProvider, logger);
        }

        public ApiService(ApiClientConfig config, ITokenProvider tokenProvider = null, ILogger logger = null)
        {
            Client = new ApiClient(config, tokenProvider, logger);
        }

        public CancellationToken Token => Cts.Token;

        public void Cancel() => Cts.Cancel();

        public void Dispose()
        {
            Cts.Cancel();
            Cts.Dispose();
        }
    }
}
