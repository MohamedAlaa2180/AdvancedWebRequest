using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    public class TestGetRequest : MonoBehaviour
    {
        [Header("API Configuration")]
        [SerializeField] private string _baseUrl = "https://ark.genesiscreations.co";
        [SerializeField] private string _endpoint = "/api/ark-caps/user-id";
        [SerializeField] private string _email = "myji@mailinator.com";

        [Header("Logging")]
        [SerializeField] private LogLevel _logLevel = LogLevel.Detailed;
        [SerializeField] private bool _logRequestBody = false;
        [SerializeField] private bool _logResponseBody = true;

        [Header("Options")]
        [SerializeField] private float _timeout = 30f;

        private ApiClient _client;
        private CancellationTokenSource _cts;

        void Start()
        {
            InitializeClient();
            MakeGetRequest().Forget();
        }

        void InitializeClient()
        {
            var config = ApiClientConfig.Create(_baseUrl);
            
            config.LogLevel = _logLevel;
            config.LogRequestBody = _logRequestBody;
            config.LogResponseBody = _logResponseBody;
            
            _client = new ApiClient(config);
        }

        async UniTask MakeGetRequest()
        {
            _cts = new CancellationTokenSource();

            try
            {
                string fullPath = $"{_endpoint}?email={Uri.EscapeDataString(_email)}";

                var options = RequestOptions.WithTimeout(_timeout);

                var response = await _client.SendJsonAsync<UserIdResponse>(
                    fullPath,
                    "GET",
                    null,
                    _cts.Token,
                    options
                );

                Debug.Log($"<color=green>✓ Success! User ID: {response.userId}</color>");
            }
            catch (ApiException ex)
            {
                Debug.LogError($"<color=red>Request failed - check logs above for details</color>");
            }
        }

        [ContextMenu("Test Request Now")]
        public void TestRequestNow()
        {
            MakeGetRequest().Forget();
        }

        void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        [Serializable]
        public class UserIdResponse
        {
            public string userId;
        }
    }
}
