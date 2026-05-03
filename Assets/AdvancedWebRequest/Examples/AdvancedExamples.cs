using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;
using ILogger = AdvancedWebRequest.Core.ILogger;

namespace AdvancedWebRequest.Examples
{
    public class AdvancedExamples : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _baseUrl = "https://api.example.com";
        [SerializeField] private string _jwtToken;

        private ApiClient _client;
        private CancellationTokenSource _cts;

        void Start()
        {
            InitializeClient();
        }

        void InitializeClient()
        {
            var config = ApiClientConfig.Create(_baseUrl);
            
            config.DefaultHeaders["X-App-Version"] = Application.version;
            config.DefaultHeaders["X-Platform"] = Application.platform.ToString();
            config.DefaultHeaders["X-Device-Id"] = SystemInfo.deviceUniqueIdentifier;
            
            config.DefaultRetryPolicy = new RetryPolicy
            {
                MaxRetries = 3,
                BaseDelaySeconds = 1f,
                MaxDelaySeconds = 10f,
                UseJitter = true
            };
            
            config.DefaultRequestOptions = new RequestOptions
            {
                TimeoutSeconds = 30f
            };
            
            var tokenProvider = new SimpleTokenProvider(_jwtToken);
            
            _client = new ApiClient(config, tokenProvider, new CustomLogger());
        }

        public async UniTask Example_BasicRequest()
        {
            _cts = new CancellationTokenSource();
            
            try
            {
                var user = await _client.GetAsync<UserDto>("/api/users/me", _cts.Token);
                Debug.Log($"User: {user.username}");
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Failed: {ex}");
            }
        }

        public async UniTask Example_PostWithBody()
        {
            _cts = new CancellationTokenSource();
            
            var request = new LoginRequest
            {
                Email = "user@example.com",
                Password = "secure_password"
            };
            
            try
            {
                var response = await _client.PostAsync<LoginResponse>("/api/auth/login", request, _cts.Token);
                
                if (_client is ApiClient client)
                {
                    var tokenProvider = new SimpleTokenProvider(response.AccessToken);
                }
                
                Debug.Log($"Login successful! Token expires at: {response.ExpiresAt}");
            }
            catch (ApiException ex) when (ex.Category == ApiErrorCategory.Unauthorized)
            {
                Debug.LogError("Invalid credentials");
            }
        }

        public async UniTask Example_CustomTimeout()
        {
            _cts = new CancellationTokenSource();
            
            var options = new RequestOptions
            {
                TimeoutSeconds = 60f
            };
            
            try
            {
                var data = await _client.SendJsonAsync<LargeDataResponse>(
                    "/api/data/large",
                    "GET",
                    null,
                    _cts.Token,
                    options
                );
                
                Debug.Log($"Loaded {data.Items.Length} items");
            }
            catch (ApiException ex) when (ex.Category == ApiErrorCategory.Timeout)
            {
                Debug.LogWarning("Request took too long, retrying with longer timeout...");
            }
        }

        public async UniTask Example_NoRetry()
        {
            _cts = new CancellationTokenSource();
            
            try
            {
                var result = await _client.SendJsonAsync<TransactionResponse>(
                    "/api/transactions",
                    "POST",
                    new { amount = 100, currency = "USD" },
                    _cts.Token,
                    RequestOptions.NoRetry
                );
                
                Debug.Log($"Transaction ID: {result.TransactionId}");
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Transaction failed: {ex.Message}");
            }
        }

        public async UniTask Example_StructuredErrorHandling()
        {
            _cts = new CancellationTokenSource();
            
            try
            {
                await _client.PostAsync<object>("/api/users", new { username = "" }, _cts.Token);
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Category: {ex.Category}");
                Debug.LogError($"Status: {ex.StatusCode}");
                
                if (ex.StructuredError != null)
                {
                    Debug.LogError($"Error Code: {ex.StructuredError.Code}");
                    Debug.LogError($"Message: {ex.StructuredError.Message}");
                    
                    if (ex.StructuredError.Details != null)
                    {
                        foreach (var detail in ex.StructuredError.Details)
                        {
                            Debug.LogError($"  {detail.Key}: {detail.Value}");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Raw response: {ex.RawResponseBody}");
                }
            }
        }

        public async UniTask Example_ParallelRequests()
        {
            _cts = new CancellationTokenSource();
            
            try
            {
                var (user, posts, comments) = await UniTask.WhenAll(
                    _client.GetAsync<UserDto>("/api/users/1", _cts.Token),
                    _client.GetAsync<PostDto[]>("/api/posts?userId=1", _cts.Token),
                    _client.GetAsync<CommentDto[]>("/api/comments?userId=1", _cts.Token)
                );
                
                Debug.Log($"Loaded user with {posts.Length} posts and {comments.Length} comments");
            }
            catch (ApiException ex)
            {
                Debug.LogError($"One of the requests failed: {ex}");
            }
        }

        public async UniTask Example_CancellationOnSceneChange()
        {
            _cts = new CancellationTokenSource();
            
            try
            {
                var task = _client.GetAsync<UserDto>("/api/users/me", _cts.Token);
                
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _cts.Token);
                
                _cts.Cancel();
                
                var result = await task;
            }
            catch (ApiException ex) when (ex.Category == ApiErrorCategory.Canceled)
            {
                Debug.Log("Request was properly canceled");
            }
        }

        void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        [Serializable]
        public class UserDto
        {
            public int id;
            public string username;
            public string email;
            public string avatarUrl;
        }

        [Serializable]
        public class LoginRequest
        {
            public string Email;
            public string Password;
        }

        [Serializable]
        public class LoginResponse
        {
            public string AccessToken;
            public string RefreshToken;
            public DateTime ExpiresAt;
            public UserDto User;
        }

        [Serializable]
        public class LargeDataResponse
        {
            public object[] Items;
            public int TotalCount;
        }

        [Serializable]
        public class TransactionResponse
        {
            public string TransactionId;
            public string Status;
        }

        [Serializable]
        public class PostDto
        {
            public int id;
            public string title;
            public string content;
        }

        [Serializable]
        public class CommentDto
        {
            public int id;
            public string text;
        }

        public class CustomLogger : ILogger
        {
            public void LogInfo(string message)
            {
                Debug.Log($"<color=cyan>[API]</color> {message}");
            }

            public void LogWarning(string message)
            {
                Debug.LogWarning($"[API] {message}");
            }

            public void LogError(string message)
            {
                Debug.LogError($"[API] {message}");
            }

            public void LogRequest(string method, string url, object body = null)
            {
                Debug.Log($"<color=cyan>[API]</color> <color=yellow>→</color> {method} {url}");
            }

            public void LogResponse(int statusCode, float duration, string body = null)
            {
                var color = statusCode >= 200 && statusCode < 300 ? "green" : "red";
                Debug.Log($"<color=cyan>[API]</color> <color={color}>←</color> {statusCode} in {duration:F2}s");
            }

            public void LogException(ApiException exception)
            {
                Debug.LogError($"<color=red>[API] ✗ {exception.Category}</color>: {exception.Message}");
            }
        }
    }
}
