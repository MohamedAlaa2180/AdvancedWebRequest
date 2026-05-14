using System.Collections.Generic;

namespace AdvancedWebRequest.Core
{
    public enum LogLevel
    {
        None = 0,
        Errors = 1,
        Basic = 2,
        Detailed = 3,
        Verbose = 4
    }

    public class ApiClientConfig
    {
        public string BaseUrl { get; set; }
        public Dictionary<string, string> DefaultHeaders { get; set; } = new Dictionary<string, string>();
        public RetryPolicy DefaultRetryPolicy { get; set; } = RetryPolicy.Default;
        public RequestOptions DefaultRequestOptions { get; set; } = RequestOptions.Default;

        public LogLevel LogLevel { get; set; } = LogLevel.Basic;
        public bool LogRequestBody { get; set; } = false;
        public bool LogResponseBody { get; set; } = false;
        public int MaxLogBodyLength { get; set; } = 500;

        /// <summary>
        /// Assigned by the Editor assembly to inject editor preferences into new configs.
        /// </summary>
        public static System.Action<ApiClientConfig> OnCreate;

        public static ApiClientConfig Create(string baseUrl)
        {
            var config = new ApiClientConfig
            {
                BaseUrl = baseUrl,
                DefaultHeaders = new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "User-Agent", $"Unity/{UnityEngine.Application.version}" }
                }
            };

            OnCreate?.Invoke(config);

            return config;
        }
    }
}
