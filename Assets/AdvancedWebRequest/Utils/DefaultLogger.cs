using UnityEngine;

namespace AdvancedWebRequest.Core
{
    public class DefaultLogger : ILogger
    {
        private readonly string _prefix;
        private readonly bool _useColors;

        public DefaultLogger(string prefix = "[API]", bool useColors = true)
        {
            _prefix = prefix;
            _useColors = useColors;
        }

        public void LogInfo(string message)
        {
            if (_useColors)
                Debug.Log($"<color=cyan>{_prefix}</color> {message}");
            else
                Debug.Log($"{_prefix} {message}");
        }

        public void LogWarning(string message)
        {
            if (_useColors)
                Debug.LogWarning($"<color=yellow>{_prefix}</color> {message}");
            else
                Debug.LogWarning($"{_prefix} {message}");
        }

        public void LogError(string message)
        {
            if (_useColors)
                Debug.LogError($"<color=red>{_prefix}</color> {message}");
            else
                Debug.LogError($"{_prefix} {message}");
        }

        public void LogRequest(string method, string url, object body = null)
        {
            if (_useColors)
                Debug.Log($"<color=cyan>{_prefix}</color> <color=yellow>→</color> {method} {url}");
            else
                Debug.Log($"{_prefix} → {method} {url}");

            if (body != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(body, Newtonsoft.Json.Formatting.Indented);
                if (_useColors)
                    Debug.Log($"<color=cyan>{_prefix}</color> <color=grey>Request Body:</color>\n{json}");
                else
                    Debug.Log($"{_prefix} Request Body:\n{json}");
            }
        }

        public void LogResponse(int statusCode, float duration, string body = null)
        {
            var statusColor = statusCode >= 200 && statusCode < 300 ? "green" : "red";
            
            if (_useColors)
                Debug.Log($"<color=cyan>{_prefix}</color> <color={statusColor}>←</color> {statusCode} in {duration:F2}s");
            else
                Debug.Log($"{_prefix} ← {statusCode} in {duration:F2}s");

            if (body != null)
            {
                if (_useColors)
                    Debug.Log($"<color=cyan>{_prefix}</color> <color=grey>Response Body:</color>\n{body}");
                else
                    Debug.Log($"{_prefix} Response Body:\n{body}");
            }
        }

        public void LogException(ApiException exception)
        {
            var icon = "✗";
            if (_useColors)
                Debug.LogError($"<color=red>{_prefix} {icon} {exception.Category}</color>: {exception.Message}");
            else
                Debug.LogError($"{_prefix} {icon} {exception.Category}: {exception.Message}");

            if (exception.StatusCode.HasValue)
            {
                Debug.LogError($"{_prefix}   Status Code: {exception.StatusCode}");
            }

            if (!string.IsNullOrEmpty(exception.Url))
            {
                Debug.LogError($"{_prefix}   URL: {exception.Method} {exception.Url}");
            }

            if (exception.StructuredError != null)
            {
                Debug.LogError($"{_prefix}   Error Code: {exception.StructuredError.Code}");
                Debug.LogError($"{_prefix}   Error Message: {exception.StructuredError.Message}");
            }
            else if (!string.IsNullOrEmpty(exception.RawResponseBody))
            {
                Debug.LogError($"{_prefix}   Response: {exception.RawResponseBody}");
            }
        }
    }
}
