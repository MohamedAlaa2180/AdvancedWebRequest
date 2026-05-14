using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    public class FluentApiExample : MonoBehaviour
    {
        private ApiService _api;

        void Awake() => _api = new ApiService("https://jsonplaceholder.typicode.com");
        void OnDestroy() => _api.Dispose();

        void Start() => DemoFluentApi().Forget();

        async UniTask DemoFluentApi()
        {
            var user = await _api.Client
                .Request("/users/1")
                .Get()
                .WithTimeout(10f)
                .SendAsync<User>(_api.Token);

            var newPost = await _api.Client
                .Request("/posts")
                .Post()
                .WithBody(new { title = "Test", body = "Content", userId = 1 })
                .NoRetry()
                .WithTimeout(15f)
                .SendAsync<Post>(_api.Token);

            await _api.Client
                .Request("/posts/1")
                .Delete()
                .WithHeader("X-Custom-Header", "value")
                .SendAsync(_api.Token);

            var updatedPost = await _api.Client
                .Request("/posts/1")
                .Put()
                .WithBody(new { title = "Updated", body = "New content", userId = 1 })
                .WithRetry(RetryPolicy.Aggressive)
                .SendAsync<Post>(_api.Token);
        }

        [System.Serializable] public class User { public int id; public string name; public string email; }
        [System.Serializable] public class Post { public int id; public string title; public string body; public int userId; }
    }
}
