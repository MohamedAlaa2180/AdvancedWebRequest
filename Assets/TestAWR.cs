using AdvancedWebRequest.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TestAWR : MonoBehaviour
{
    [SerializeField] private string _baseUrl = "https://reqbin.com";
    [SerializeField] private string _endpoint = "/echo/get/json";

    private ApiService _api;

    private void Awake()
    {
        _api = new ApiService(_baseUrl);
    }

    private void Start()
    {
        MakeGetRequest().Forget();
    }

    private void OnDestroy()
    {
        _api.Dispose();
    }

    private async UniTask MakeGetRequest()
    {
        var response = await _api.Client
                .Request(_endpoint)
                .Post()
                .SendAsync<EchoResponse>(_api.Token);
    }

    public class EchoResponse
    { public string success; }
}