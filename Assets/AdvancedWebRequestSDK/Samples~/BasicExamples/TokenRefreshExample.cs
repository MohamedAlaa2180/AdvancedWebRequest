using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    public class RefreshableTokenProvider : ITokenProvider
    {
        private string _accessToken;
        private string _refreshToken;
        private DateTime _expiresAt;
        private readonly ApiClient _authClient;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        public RefreshableTokenProvider(string baseUrl, string initialAccessToken = null, string refreshToken = null)
        {
            _accessToken = initialAccessToken;
            _refreshToken = refreshToken;
            _expiresAt = DateTime.UtcNow.AddHours(1);
            
            var config = ApiClientConfig.Create(baseUrl);
            _authClient = new ApiClient(config);
        }

        public async UniTask<string> GetAccessTokenAsync(CancellationToken ct = default)
        {
            if (DateTime.UtcNow < _expiresAt.AddMinutes(-5))
            {
                return _accessToken;
            }

            await _refreshLock.WaitAsync(ct);
            try
            {
                if (DateTime.UtcNow < _expiresAt.AddMinutes(-5))
                {
                    return _accessToken;
                }

                await RefreshTokenAsync(ct);
                return _accessToken;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private async UniTask RefreshTokenAsync(CancellationToken ct)
        {
            try
            {
                var request = new RefreshTokenRequest { RefreshToken = _refreshToken };
                var response = await _authClient
                    .Request("/api/auth/refresh")
                    .Post()
                    .WithBody(request)
                    .SendAsync<RefreshTokenResponse>(ct);

                _accessToken = response.AccessToken;
                _refreshToken = response.RefreshToken;
                _expiresAt = response.ExpiresAt;

                Debug.Log("Token refreshed successfully");
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Token refresh failed: {ex.Message}");
                throw;
            }
        }

        public void UpdateTokens(string accessToken, string refreshToken, DateTime expiresAt)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _expiresAt = expiresAt;
        }

        [Serializable]
        private class RefreshTokenRequest
        {
            public string RefreshToken;
        }

        [Serializable]
        private class RefreshTokenResponse
        {
            public string AccessToken;
            public string RefreshToken;
            public DateTime ExpiresAt;
        }
    }

    public class TokenRefreshExample : MonoBehaviour
    {
        [SerializeField] private string _baseUrl = "https://api.example.com";
        
        private ApiClient _client;
        private RefreshableTokenProvider _tokenProvider;
        private CancellationTokenSource _cts;

        async UniTask Start()
        {
            await LoginAndSetupClient();
            await MakeAuthenticatedRequests();
        }

        async UniTask LoginAndSetupClient()
        {
            _cts = new CancellationTokenSource();
            
            var tempConfig = ApiClientConfig.Create(_baseUrl);
            var tempClient = new ApiClient(tempConfig);

            var loginRequest = new LoginRequest
            {
                Email = "user@example.com",
                Password = "password"
            };

            try
            {
                var loginResponse = await tempClient
                    .Request("/api/auth/login")
                    .Post()
                    .WithBody(loginRequest)
                    .SendAsync<LoginResponse>(_cts.Token);

                _tokenProvider = new RefreshableTokenProvider(
                    _baseUrl,
                    loginResponse.AccessToken,
                    loginResponse.RefreshToken
                );

                var config = ApiClientConfig.Create(_baseUrl);
                _client = new ApiClient(config, _tokenProvider);

                Debug.Log("Client setup with refreshable token provider");
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Login failed: {ex}");
            }
        }

        async UniTask MakeAuthenticatedRequests()
        {
            try
            {
                var user = await _client.Request("/api/users/me").Get().SendAsync<UserDto>(_cts.Token);
                Debug.Log($"User: {user.username}");

                await UniTask.Delay(TimeSpan.FromHours(2), cancellationToken: _cts.Token);

                var userAgain = await _client.Request("/api/users/me").Get().SendAsync<UserDto>(_cts.Token);
                Debug.Log($"User (after token refresh): {userAgain.username}");
            }
            catch (ApiException ex)
            {
                Debug.LogError($"Request failed: {ex}");
            }
        }

        void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        [Serializable]
        private class LoginRequest
        {
            public string Email;
            public string Password;
        }

        [Serializable]
        private class LoginResponse
        {
            public string AccessToken;
            public string RefreshToken;
            public DateTime ExpiresAt;
        }

        [Serializable]
        private class UserDto
        {
            public int id;
            public string username;
        }
    }
}
