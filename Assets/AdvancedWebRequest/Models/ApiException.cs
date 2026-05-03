using System;

namespace AdvancedWebRequest.Core
{
    public enum ApiErrorCategory
    {
        Canceled,
        Timeout,
        NetworkError,
        HttpError,
        BadRequest,
        Unauthorized,
        Forbidden,
        NotFound,
        RateLimited,
        ServerError,
        JsonParseError,
        Unknown
    }

    public class ApiException : Exception
    {
        public ApiErrorCategory Category { get; }
        public int? StatusCode { get; set; }
        public ApiErrorResponse StructuredError { get; set; }
        public string RawResponseBody { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }

        public ApiException(ApiErrorCategory category, string message) 
            : base(message)
        {
            Category = category;
        }

        public ApiException(ApiErrorCategory category, string message, Exception innerException) 
            : base(message, innerException)
        {
            Category = category;
        }

        public bool IsRetryable => Category switch
        {
            ApiErrorCategory.Timeout => true,
            ApiErrorCategory.NetworkError => true,
            ApiErrorCategory.RateLimited => true,
            ApiErrorCategory.ServerError => true,
            _ => false
        };

        public override string ToString()
        {
            var details = $"[{Category}] {Message}";
            
            if (StatusCode.HasValue)
                details += $" (HTTP {StatusCode})";
            
            if (!string.IsNullOrEmpty(Url))
                details += $"\nURL: {Method} {Url}";
            
            if (StructuredError != null)
                details += $"\nError Code: {StructuredError.Code}";
            
            if (!string.IsNullOrEmpty(RawResponseBody))
                details += $"\nResponse: {RawResponseBody}";
            
            return details;
        }
    }
}
