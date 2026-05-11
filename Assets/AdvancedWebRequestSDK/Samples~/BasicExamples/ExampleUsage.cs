using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    public class ExampleUsage : MonoBehaviour
    {
        private ApiClient _client;
        private CancellationTokenSource _cts;

        void Start()
        {
            InitializeClient();
            LoadDataExample().Forget();
        }

        void InitializeClient()
        {
            var config = ApiClientConfig.Create("https://jsonplaceholder.typicode.com");
            
            config.DefaultHeaders["X-App-Version"] = Application.version;
            
            var tokenProvider = new SimpleTokenProvider();
            
            _client = new ApiClient(config, tokenProvider);
        }

        async UniTask LoadDataExample()
        {
            _cts = new CancellationTokenSource();

            try
            {
                Debug.Log("Fetching user...");
                var user = await _client.Request("/users/1").Get().SendAsync<User>(_cts.Token);
                Debug.Log($"User loaded: {user.name} ({user.email})");

                Debug.Log("Fetching posts...");
                var posts = await _client.Request("/posts?userId=1").Get().SendAsync<Post[]>(_cts.Token);
                Debug.Log($"Loaded {posts.Length} posts");

                Debug.Log("Creating new post...");
                var newPost = new CreatePostRequest
                {
                    userId = 1,
                    title = "Test Post",
                    body = "This is a test post"
                };
                var createdPost = await _client.Request("/posts").Post().WithBody(newPost).SendAsync<Post>(_cts.Token);
                Debug.Log($"Created post with ID: {createdPost.id}");
            }
            catch (ApiException ex)
            {
                HandleApiException(ex);
            }
        }

        void HandleApiException(ApiException ex)
        {
            switch (ex.Category)
            {
                case ApiErrorCategory.Canceled:
                    Debug.Log("Request was canceled");
                    break;
                
                case ApiErrorCategory.Timeout:
                    Debug.LogWarning("Request timed out - check your connection");
                    break;
                
                case ApiErrorCategory.NetworkError:
                    Debug.LogError("Network error - no internet connection?");
                    break;
                
                case ApiErrorCategory.Unauthorized:
                    Debug.LogError("Unauthorized - need to login");
                    break;
                
                case ApiErrorCategory.ServerError:
                    Debug.LogError("Server error - try again later");
                    break;
                
                default:
                    Debug.LogError($"API Error: {ex}");
                    break;
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
            public string phone;
            public string website;
        }

        [Serializable]
        public class Post
        {
            public int id;
            public int userId;
            public string title;
            public string body;
        }

        [Serializable]
        public class CreatePostRequest
        {
            public int userId;
            public string title;
            public string body;
        }
    }
}
