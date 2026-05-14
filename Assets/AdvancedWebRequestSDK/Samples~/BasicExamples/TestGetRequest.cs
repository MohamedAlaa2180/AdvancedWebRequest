using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    public class TestGetRequest : MonoBehaviour
    {
        [Header("API Configuration")]
        [SerializeField] private string _baseUrl = "https://reqbin.com";
        [SerializeField] private string _endpoint = "/echo/get/json";

        [Header("Options")]
        [SerializeField] private float _timeout = 30f;

        private ApiService _api;

        void Awake() => _api = new ApiService(_baseUrl);
        void OnDestroy() => _api.Dispose();

        void Start() => MakeGetRequest().Forget();

        async UniTask MakeGetRequest()
        {
            var response = await _api.Client
                .Request(_endpoint)
                .Get()
                .WithTimeout(_timeout)
                .SendAsync<EchoResponse>(_api.Token);
        }

        [ContextMenu("Test Request Now")]
        public void TestRequestNow() => MakeGetRequest().Forget();

        [Serializable]
        public class EchoResponse { public string success; }
    }
}
