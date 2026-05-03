using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    public class LoggingExample : MonoBehaviour
    {
        [Header("Test Different Log Levels")]
        [SerializeField] private bool _testNone = false;
        [SerializeField] private bool _testErrors = false;
        [SerializeField] private bool _testBasic = true;
        [SerializeField] private bool _testDetailed = false;
        [SerializeField] private bool _testVerbose = false;

        private CancellationTokenSource _cts;

        void Start()
        {
            RunLoggingTests().Forget();
        }

        async UniTask RunLoggingTests()
        {
            _cts = new CancellationTokenSource();

            if (_testNone)
            {
                Debug.Log("\n<color=yellow>=== Testing LogLevel.None (No logs) ===</color>");
                await TestWithLogLevel(LogLevel.None);
            }

            if (_testErrors)
            {
                Debug.Log("\n<color=yellow>=== Testing LogLevel.Errors (Only errors) ===</color>");
                await TestWithLogLevel(LogLevel.Errors);
            }

            if (_testBasic)
            {
                Debug.Log("\n<color=yellow>=== Testing LogLevel.Basic (Request/Response summary) ===</color>");
                await TestWithLogLevel(LogLevel.Basic);
            }

            if (_testDetailed)
            {
                Debug.Log("\n<color=yellow>=== Testing LogLevel.Detailed (With body) ===</color>");
                await TestWithLogLevel(LogLevel.Detailed);
            }

            if (_testVerbose)
            {
                Debug.Log("\n<color=yellow>=== Testing LogLevel.Verbose (Everything) ===</color>");
                await TestWithLogLevel(LogLevel.Verbose);
            }
        }

        async UniTask TestWithLogLevel(LogLevel logLevel)
        {
            var config = ApiClientConfig.Create("https://jsonplaceholder.typicode.com");
            config.LogLevel = logLevel;
            config.LogRequestBody = true;
            config.LogResponseBody = true;
            config.MaxLogBodyLength = 200;

            var client = new ApiClient(config);

            try
            {
                Debug.Log($"  Test 1: Successful GET request");
                var user = await client.GetAsync<User>("/users/1", _cts.Token);
                Debug.Log($"  <color=green>Got user: {user.name}</color>");
                
                await UniTask.Delay(500);

                Debug.Log($"\n  Test 2: Successful POST request");
                var newPost = new CreatePost { userId = 1, title = "Test", body = "Content" };
                var post = await client.PostAsync<Post>("/posts", newPost, _cts.Token);
                Debug.Log($"  <color=green>Created post: {post.id}</color>");

                await UniTask.Delay(500);

                Debug.Log($"\n  Test 3: Error request (404)");
                try
                {
                    await client.GetAsync<User>("/users/99999", _cts.Token);
                }
                catch (ApiException ex)
                {
                    Debug.Log($"  <color=orange>Caught expected error: {ex.Category}</color>");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"  Unexpected error: {ex.Message}");
            }

            await UniTask.Delay(1000);
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
            public int userId;
            public string title;
            public string body;
        }

        [Serializable]
        public class CreatePost
        {
            public int userId;
            public string title;
            public string body;
        }
    }
}
