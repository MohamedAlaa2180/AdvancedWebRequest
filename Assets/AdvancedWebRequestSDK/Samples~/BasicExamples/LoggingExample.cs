using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Examples
{
    /// <summary>
    /// Demonstrates API requests with logging driven by Project Settings.
    /// Configure log level via Edit > Project Settings > Advanced Web Request.
    /// </summary>
    public class LoggingExample : MonoBehaviour
    {
        private ApiService _api;

        void Awake() => _api = new ApiService("https://jsonplaceholder.typicode.com");
        void OnDestroy() => _api.Dispose();

        void Start() => RunRequests().Forget();

        async UniTask RunRequests()
        {
            var user = await _api.Client.Request("/users/1").Get().SendAsync<User>(_api.Token);

            var newPost = new CreatePost { userId = 1, title = "Test", body = "Content" };
            var post = await _api.Client.Request("/posts").Post().WithBody(newPost).SendAsync<Post>(_api.Token);

            await UniTask.Delay(500);

            // Intentional 404 — observe error logging in the console
            try
            {
                await _api.Client.Request("/users/99999").Get().SendAsync<User>(_api.Token);
            }
            catch (ApiException)
            {
                throw;
            }
        }

        [System.Serializable] public class User       { public int id; public string name; public string email; }
        [System.Serializable] public class Post       { public int id; public int userId; public string title; public string body; }
        [System.Serializable] public class CreatePost { public int userId; public string title; public string body; }
    }
}
