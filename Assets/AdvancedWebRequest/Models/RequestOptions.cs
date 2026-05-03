using System.Collections.Generic;

namespace AdvancedWebRequest.Core
{
    public class RequestOptions
    {
        public float TimeoutSeconds { get; set; } = 30f;
        public RetryPolicy RetryPolicy { get; set; }
        public Dictionary<string, string> AdditionalHeaders { get; set; }

        public static RequestOptions Default => new RequestOptions
        {
            TimeoutSeconds = 30f,
            RetryPolicy = RetryPolicy.Default
        };

        public static RequestOptions WithTimeout(float timeoutSeconds)
        {
            return new RequestOptions { TimeoutSeconds = timeoutSeconds };
        }

        public static RequestOptions NoRetry => new RequestOptions
        {
            RetryPolicy = RetryPolicy.NoRetry
        };
    }
}
