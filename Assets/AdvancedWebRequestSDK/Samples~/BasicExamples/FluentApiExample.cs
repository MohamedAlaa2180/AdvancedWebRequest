using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    public class FluentApiExample : MonoBehaviour
    {
        private ApiClient _client;
        private CancellationTokenSource _cts;

        void Start()
        {
            var config = ApiClientConfig.Create("https://jsonplaceholder.typicode.com");
            _client = new ApiClient(config);
            
            DemoFluentApi().Forget();
        }

        async UniTask DemoFluentApi()
        {
            _cts = new CancellationTokenSource();

            try
            {
                var user = await _client
                    .Request("/users/1")
                    .Get()
                    .WithTimeout(10f)
                    .SendAsync<User>(_cts.Token);
                
                Debug.Log($"User: {user.name}");

                var newPost = await _client
                    .Request("/posts")
                    .Post()
                    .WithBody(new { title = "Test", body = "Content", userId = 1 })
                    .NoRetry()
                    .WithTimeout(15f)
                    .SendAsync<Post>(_cts.Token);
                
                Debug.Log($"Created post: {newPost.id}");

                await _client
                    .Request("/posts/1")
                    .Delete()
                    .WithHeader("X-Custom-Header", "value")
                    .SendAsync(_cts.Token);
                
                Debug.Log("Post deleted");

                var updatedPost = await _client
                    .Request("/posts/1")
                    .Put()
                    .WithBody(new { title = "Updated", body = "New content", userId = 1 })
                    .WithRetry(RetryPolicy.Aggressive)
                    .SendAsync<Post>(_cts.Token);
                
                Debug.Log($"Updated post: {updatedPost.title}");
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
        public class User
        {
            public int id;
            public string name;
            public string email;
        }

        [Serializable]
        public class Post
        {
            public int id;
            public string title;
            public string body;
            public int userId;
        }
    }
}
