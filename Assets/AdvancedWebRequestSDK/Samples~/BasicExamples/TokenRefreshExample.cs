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
        private readonly ApiService _authApi;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        public RefreshableTokenProvider(string baseUrl, string initialAccessToken = null, string refreshToken = null)
        {
            _accessToken = initialAccessToken;
            _refreshToken = refreshToken;
            _expiresAt = DateTime.UtcNow.AddHours(1);
            _authApi = new ApiService(baseUrl);
        }

        public async UniTask<string> GetAccessTokenAsync(CancellationToken ct = default)
        {
            if (DateTime.UtcNow < _expiresAt.AddMinutes(-5))
                return _accessToken;

            await _refreshLock.WaitAsync(ct);
            try
            {
                if (DateTime.UtcNow < _expiresAt.AddMinutes(-5))
                    return _accessToken;

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
            var request = new RefreshTokenRequest { RefreshToken = _refreshToken };
            var response = await _authApi.Client
                .Request("/api/auth/refresh")
                .Post()
                .WithBody(request)
                .SendAsync<RefreshTokenResponse>(ct);

            _accessToken  = response.AccessToken;
            _refreshToken = response.RefreshToken;
            _expiresAt    = response.ExpiresAt;
        }

        public void UpdateTokens(string accessToken, string refreshToken, DateTime expiresAt)
        {
            _accessToken  = accessToken;
            _refreshToken = refreshToken;
            _expiresAt    = expiresAt;
        }

        [Serializable] private class RefreshTokenRequest  { public string RefreshToken; }
        [Serializable] private class RefreshTokenResponse { public string AccessToken; public string RefreshToken; public DateTime ExpiresAt; }
    }

    public class TokenRefreshExample : MonoBehaviour
    {
        [SerializeField] private string _baseUrl = "https://api.example.com";

        private ApiService _api;
        private RefreshableTokenProvider _tokenProvider;

        void OnDestroy() => _api?.Dispose();

        async UniTask Start()
        {
            await LoginAndSetupClient();
            await MakeAuthenticatedRequests();
        }

        async UniTask LoginAndSetupClient()
        {
            using var tempApi = new ApiService(_baseUrl);
            var loginRequest = new LoginRequest { Email = "user@example.com", Password = "password" };

            var loginResponse = await tempApi.Client
                .Request("/api/auth/login")
                .Post()
                .WithBody(loginRequest)
                .SendAsync<LoginResponse>(tempApi.Token);

            _tokenProvider = new RefreshableTokenProvider(_baseUrl, loginResponse.AccessToken, loginResponse.RefreshToken);
            _api = new ApiService(ApiClientConfig.Create(_baseUrl), _tokenProvider);
        }

        async UniTask MakeAuthenticatedRequests()
        {
            var user = await _api.Client.Request("/api/users/me").Get().SendAsync<UserDto>(_api.Token);

            await UniTask.Delay(TimeSpan.FromHours(2), cancellationToken: _api.Token);

            var userAgain = await _api.Client.Request("/api/users/me").Get().SendAsync<UserDto>(_api.Token);
        }

        [Serializable] private class LoginRequest  { public string Email; public string Password; }
        [Serializable] private class LoginResponse { public string AccessToken; public string RefreshToken; public DateTime ExpiresAt; }
        [Serializable] private class UserDto       { public int id; public string username; }
    }
}
