namespace AdvancedWebRequest.Core
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogRequest(string method, string url, object body = null);
        void LogResponse(int statusCode, float duration, string body = null);
        void LogException(ApiException exception);
    }
}
