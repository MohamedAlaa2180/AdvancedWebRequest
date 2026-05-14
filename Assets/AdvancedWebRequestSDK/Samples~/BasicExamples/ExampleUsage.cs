using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    public class ExampleUsage : MonoBehaviour
    {
        private ApiService _api;

        void Awake()
        {
            var config = ApiClientConfig.Create("https://jsonplaceholder.typicode.com");
            config.DefaultHeaders["X-App-Version"] = Application.version;
            _api = new ApiService(config, new SimpleTokenProvider());
        }

        void OnDestroy() => _api.Dispose();

        void Start() => LoadDataExample().Forget();

        async UniTask LoadDataExample()
        {
            var user = await _api.Client.Request("/users/1").Get().SendAsync<User>(_api.Token);

            var posts = await _api.Client.Request("/posts?userId=1").Get().SendAsync<Post[]>(_api.Token);

            var newPost = new CreatePostRequest { userId = 1, title = "Test Post", body = "This is a test post" };
            var createdPost = await _api.Client.Request("/posts").Post().WithBody(newPost).SendAsync<Post>(_api.Token);
        }

        [Serializable] public class User             { public int id; public string name; public string email; public string phone; public string website; }
        [Serializable] public class Post             { public int id; public int userId; public string title; public string body; }
        [Serializable] public class CreatePostRequest{ public int userId; public string title; public string body; }
    }
}
