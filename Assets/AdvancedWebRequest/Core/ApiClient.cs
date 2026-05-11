using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AdvancedWebRequest.Core
{
    public class ApiClient
    {
        private readonly ApiClientConfig _config;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger _logger;

        public ApiClient(ApiClientConfig config, ITokenProvider tokenProvider = null, ILogger logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _tokenProvider = tokenProvider;
            _logger = logger ?? new DefaultLogger();
        }

        internal async UniTask<T> SendJsonAsync<T>(
            string path,
            string method,
            object body = null,
            CancellationToken ct = default,
            RequestOptions options = null)
        {
            var requestSpec = new RequestSpec
            {
                Url = BuildUrl(path),
                Method = method,
                Body = body,
                Options = options ?? _config.DefaultRequestOptions
            };

            return await ExecuteWithRetryAsync<T>(requestSpec, ct);
        }

        private async UniTask<T> ExecuteWithRetryAsync<T>(RequestSpec spec, CancellationToken ct)
        {
            var retryPolicy = spec.Options.RetryPolicy ?? _config.DefaultRetryPolicy;
            var attempt = 0;
            Exception lastException = null;

            while (attempt <= retryPolicy.MaxRetries)
            {
                try
                {
                    return await ExecuteRequestAsync<T>(spec, ct);
                }
                catch (ApiException ex) when (retryPolicy.ShouldRetry(ex, attempt))
                {
                    lastException = ex;
                    attempt++;

                    if (attempt <= retryPolicy.MaxRetries)
                    {
                        var delay = retryPolicy.GetDelay(attempt);
                        
                        if (_config.LogLevel >= LogLevel.Basic)
                        {
                            _logger.LogWarning($"Request failed (attempt {attempt}), retrying in {delay.TotalMilliseconds}ms: {ex.Message}");
                        }
                        
                        await UniTask.Delay(delay, cancellationToken: ct);
                    }
                }
                catch (Exception ex)
                {
                    var apiEx = new ApiException(ApiErrorCategory.Unknown, "Unexpected error occurred", ex)
                    {
                        Url = spec.Url,
                        Method = spec.Method
                    };
                    
                    if (_config.LogLevel >= LogLevel.Errors)
                    {
                        _logger.LogException(apiEx);
                    }
                    
                    throw apiEx;
                }
            }

            throw lastException ?? new ApiException(ApiErrorCategory.Unknown, "Request failed after retries");
        }

        private async UniTask<T> ExecuteRequestAsync<T>(RequestSpec spec, CancellationToken ct)
        {
            var startTime = Time.realtimeSinceStartup;
            UnityWebRequest request = null;

            try
            {
                request = await BuildRequestAsync(spec, ct);
                
                if (_config.LogLevel >= LogLevel.Basic)
                {
                    var bodyToLog = (_config.LogLevel >= LogLevel.Detailed && _config.LogRequestBody) ? spec.Body : null;
                    _logger.LogRequest(spec.Method, spec.Url, bodyToLog);
                }

                var timeoutCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

                var timeoutTask = UniTask.Delay(
                    TimeSpan.FromSeconds(spec.Options.TimeoutSeconds),
                    cancellationToken: linkedCts.Token
                );

                var requestTask = request.SendWebRequest().ToUniTask(cancellationToken: linkedCts.Token);

                bool hasRequestCompleted;
                try
                {
                    (hasRequestCompleted, _) = await UniTask.WhenAny(requestTask, timeoutTask);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (ApiException)
                {
                    throw;
                }
                catch (Exception)
                {
                    timeoutCts.Cancel();
                    var duration = Time.realtimeSinceStartup - startTime;
                    return await HandleResponseAsync<T>(request, spec, duration);
                }

                if (!hasRequestCompleted)
                {
                    request.Abort();
                    
                    var timeoutEx = new ApiException(ApiErrorCategory.Timeout, $"Request timed out after {spec.Options.TimeoutSeconds}s")
                    {
                        Url = spec.Url,
                        Method = spec.Method
                    };
                    
                    if (_config.LogLevel >= LogLevel.Errors)
                    {
                        _logger.LogException(timeoutEx);
                    }
                    
                    throw timeoutEx;
                }

                timeoutCts.Cancel();

                var duration2 = Time.realtimeSinceStartup - startTime;
                return await HandleResponseAsync<T>(request, spec, duration2);
            }
            catch (OperationCanceledException)
            {
                var cancelEx = new ApiException(ApiErrorCategory.Canceled, "Request was canceled")
                {
                    Url = spec.Url,
                    Method = spec.Method
                };
                
                if (_config.LogLevel >= LogLevel.Basic)
                {
                    _logger.LogWarning($"Request canceled: {spec.Method} {spec.Url}");
                }
                
                throw cancelEx;
            }
            finally
            {
                request?.Dispose();
            }
        }

        private async UniTask<UnityWebRequest> BuildRequestAsync(RequestSpec spec, CancellationToken ct)
        {
            UnityWebRequest request;

            if (spec.Body != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(spec.Body);
                var bodyBytes = System.Text.Encoding.UTF8.GetBytes(json);
                request = new UnityWebRequest(spec.Url, spec.Method);
                request.uploadHandler = new UploadHandlerRaw(bodyBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
            }
            else
            {
                request = new UnityWebRequest(spec.Url, spec.Method);
                request.downloadHandler = new DownloadHandlerBuffer();
            }

            foreach (var header in _config.DefaultHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            if (_tokenProvider != null)
            {
                var token = await _tokenProvider.GetAccessTokenAsync(ct);
                if (!string.IsNullOrEmpty(token))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {token}");
                }
            }

            if (spec.Options.AdditionalHeaders != null)
            {
                foreach (var header in spec.Options.AdditionalHeaders)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            return request;
        }

        private async UniTask<T> HandleResponseAsync<T>(UnityWebRequest request, RequestSpec spec, float duration)
        {
            var statusCode = (int)request.responseCode;
            var responseBody = request.downloadHandler?.text ?? string.Empty;

            if (_config.LogLevel >= LogLevel.Basic)
            {
                var bodyToLog = (_config.LogLevel >= LogLevel.Detailed && _config.LogResponseBody) 
                    ? TruncateBody(responseBody, _config.MaxLogBodyLength) 
                    : null;
                    
                _logger.LogResponse(statusCode, duration, bodyToLog);
            }

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                if (statusCode >= 200 && statusCode < 300)
                {
                    return await ParseResponseAsync<T>(responseBody, spec);
                }

                var httpError = CreateHttpErrorException(statusCode, responseBody, spec);
                
                if (_config.LogLevel >= LogLevel.Errors)
                {
                    _logger.LogException(httpError);
                }
                
                throw httpError;
            }

            if (request.result == UnityWebRequest.Result.DataProcessingError)
            {
                var dataError = new ApiException(ApiErrorCategory.NetworkError, $"Data processing error: {request.error}")
                {
                    Url = spec.Url,
                    Method = spec.Method,
                    RawResponseBody = responseBody
                };
                
                if (_config.LogLevel >= LogLevel.Errors)
                {
                    _logger.LogException(dataError);
                }
                
                throw dataError;
            }

            return await ParseResponseAsync<T>(responseBody, spec);
        }

        private async UniTask<T> ParseResponseAsync<T>(string responseBody, RequestSpec spec)
        {
            if (string.IsNullOrEmpty(responseBody))
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)string.Empty;
                }
                return default;
            }

            try
            {
                await UniTask.SwitchToThreadPool();
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseBody);
                await UniTask.SwitchToMainThread();
                return result;
            }
            catch (Exception ex)
            {
                await UniTask.SwitchToMainThread();
                
                var parseError = new ApiException(ApiErrorCategory.JsonParseError, "Failed to parse response JSON", ex)
                {
                    Url = spec.Url,
                    Method = spec.Method,
                    RawResponseBody = TruncateBody(responseBody)
                };
                
                if (_config.LogLevel >= LogLevel.Errors)
                {
                    _logger.LogException(parseError);
                }
                
                throw parseError;
            }
        }

        private ApiException CreateHttpErrorException(int statusCode, string responseBody, RequestSpec spec)
        {
            ApiErrorResponse structuredError = null;

            if (!string.IsNullOrEmpty(responseBody))
            {
                try
                {
                    structuredError = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiErrorResponse>(responseBody);
                }
                catch
                {
                }
            }

            var category = statusCode switch
            {
                401 => ApiErrorCategory.Unauthorized,
                403 => ApiErrorCategory.Forbidden,
                404 => ApiErrorCategory.NotFound,
                429 => ApiErrorCategory.RateLimited,
                >= 500 => ApiErrorCategory.ServerError,
                >= 400 => ApiErrorCategory.BadRequest,
                _ => ApiErrorCategory.HttpError
            };

            var message = structuredError?.Message ?? $"HTTP {statusCode} error";

            return new ApiException(category, message)
            {
                StatusCode = statusCode,
                StructuredError = structuredError,
                RawResponseBody = TruncateBody(responseBody),
                Url = spec.Url,
                Method = spec.Method
            };
        }

        private string BuildUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
                return _config.BaseUrl;

            if (path.StartsWith("http://") || path.StartsWith("https://"))
                return path;

            var baseUrl = _config.BaseUrl.TrimEnd('/');
            var pathPart = path.TrimStart('/');
            return $"{baseUrl}/{pathPart}";
        }

        private string TruncateBody(string body, int maxLength = 1000)
        {
            if (string.IsNullOrEmpty(body) || body.Length <= maxLength)
                return body;

            return body.Substring(0, maxLength) + "... (truncated)";
        }
    }
}
