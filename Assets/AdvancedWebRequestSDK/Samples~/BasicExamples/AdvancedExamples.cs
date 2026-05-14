using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    public class AdvancedExamples : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _baseUrl = "https://api.example.com";
        [SerializeField] private string _jwtToken;

        private ApiService _api;

        void Awake()
        {
            var config = ApiClientConfig.Create(_baseUrl);
            config.DefaultHeaders["X-App-Version"] = Application.version;
            config.DefaultHeaders["X-Platform"]    = Application.platform.ToString();
            config.DefaultHeaders["X-Device-Id"]   = SystemInfo.deviceUniqueIdentifier;
            config.DefaultRetryPolicy = new RetryPolicy { MaxRetries = 3, BaseDelaySeconds = 1f, MaxDelaySeconds = 10f, UseJitter = true };

            _api = new ApiService(config, new SimpleTokenProvider(_jwtToken));
        }

        void OnDestroy() => _api.Dispose();

        public async UniTask Example_BasicRequest()
        {
            var user = await _api.Client.Request("/api/users/me").Get().SendAsync<UserDto>(_api.Token);
        }

        public async UniTask Example_PostWithBody()
        {
            var request = new LoginRequest { Email = "user@example.com", Password = "secure_password" };
            var response = await _api.Client
                .Request("/api/auth/login")
                .Post()
                .WithBody(request)
                .SendAsync<LoginResponse>(_api.Token);
        }

        public async UniTask Example_CustomTimeout()
        {
            var data = await _api.Client
                .Request("/api/data/large")
                .Get()
                .WithTimeout(60f)
                .SendAsync<LargeDataResponse>(_api.Token);
        }

        public async UniTask Example_NoRetry()
        {
            var result = await _api.Client
                .Request("/api/transactions")
                .Post()
                .WithBody(new { amount = 100, currency = "USD" })
                .NoRetry()
                .SendAsync<TransactionResponse>(_api.Token);
        }

        public async UniTask Example_ParallelRequests()
        {
            var (user, posts, comments) = await UniTask.WhenAll(
                _api.Client.Request("/api/users/1").Get().SendAsync<UserDto>(_api.Token),
                _api.Client.Request("/api/posts?userId=1").Get().SendAsync<PostDto[]>(_api.Token),
                _api.Client.Request("/api/comments?userId=1").Get().SendAsync<CommentDto[]>(_api.Token)
            );
        }

        public async UniTask Example_CancellationOnSceneChange()
        {
            var task = _api.Client.Request("/api/users/me").Get().SendAsync<UserDto>(_api.Token);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _api.Token);
            _api.Cancel();
            var result = await task;
        }

        [Serializable] public class UserDto           { public int id; public string username; public string email; public string avatarUrl; }
        [Serializable] public class LoginRequest       { public string Email; public string Password; }
        [Serializable] public class LoginResponse      { public string AccessToken; public string RefreshToken; public DateTime ExpiresAt; public UserDto User; }
        [Serializable] public class LargeDataResponse  { public object[] Items; public int TotalCount; }
        [Serializable] public class TransactionResponse{ public string TransactionId; public string Status; }
        [Serializable] public class PostDto            { public int id; public string title; public string content; }
        [Serializable] public class CommentDto         { public int id; public string text; }
    }
}
