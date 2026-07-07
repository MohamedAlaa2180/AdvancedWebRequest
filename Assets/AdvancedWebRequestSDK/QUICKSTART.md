# Quick Start Guide

## Installation

### Via UPM (recommended)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL…**
3. Paste:

```text
https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequestSDK#release/latest
```

4. Click **Add**

UniTask and Newtonsoft.Json are installed automatically as package dependencies.

### Import samples

In Package Manager → **Advanced Web Request** → **Samples** → import **Basic Examples**.

## 5-Minute Setup

### Step 1: Configure logging

Open **Edit → Project Settings → Advanced Web Request** and set your preferred log level and default timeout. These apply to every `ApiClientConfig.Create()` call.

### Step 2: Create an API service (MonoBehaviour)

```csharp
using UnityEngine;
using System.Threading;
using AdvancedWebRequest.Core;
using Cysharp.Threading.Tasks;

public class ApiManager : MonoBehaviour
{
    private static ApiManager _instance;
    public static ApiManager Instance => _instance;

    private ApiService _api;
    private SimpleTokenProvider _tokenProvider;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _tokenProvider = new SimpleTokenProvider();
            _api = new ApiService(ApiClientConfig.Create("https://your-api.com"), _tokenProvider);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy() => _api?.Dispose();

    public ApiClient Client => _api.Client;
    public CancellationToken Token => _api.Token;

    public void SetToken(string token) => _tokenProvider.SetToken(token);
}
```

### Step 3: Use in your game code

```csharp
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;
using System;

public class GameController : MonoBehaviour
{
    async void Start()
    {
        await LoadPlayerData();
    }

    async UniTask LoadPlayerData()
    {
        var player = await ApiManager.Instance.Client
            .Request("/api/player")
            .Get()
            .SendAsync<PlayerData>(ApiManager.Instance.Token);
    }

    [Serializable]
    public class PlayerData
    {
        public int id;
        public string name;
        public int level;
        public int coins;
    }
}
```

### Step 4: Handle login

```csharp
public class LoginManager : MonoBehaviour
{
    public async UniTask<bool> Login(string email, string password)
    {
        var request = new LoginRequest { email = email, password = password };

        try
        {
            var response = await ApiManager.Instance.Client
                .Request("/api/auth/login")
                .Post()
                .WithBody(request)
                .SendAsync<LoginResponse>(ApiManager.Instance.Token);

            ApiManager.Instance.SetToken(response.token);
            return true;
        }
        catch (ApiException ex) when (ex.Category == ApiErrorCategory.Unauthorized)
        {
            return false;
        }
    }

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class LoginResponse
    {
        public string token;
        public string userId;
    }
}
```

## Automatic logging

Logging is handled by the SDK based on **Project Settings → Advanced Web Request**. No manual `Debug.Log` calls are needed around requests.

Example console output at `Detailed` level:

```text
[API] → GET https://api.example.com/api/users/me
[API] ← 200 in 0.48s
[API] Response Body: {"id":1,"name":"John"}
```

**Log levels:** `None`, `Errors`, `Basic` (default), `Detailed`, `Verbose`

See [LOGGING.md](LOGGING.md) for the complete guide.

## Common patterns

### Loading screen with progress

```csharp
public async UniTask LoadGameData(Action<float> onProgress)
{
    var client = ApiManager.Instance.Client;
    var token = ApiManager.Instance.Token;

    onProgress?.Invoke(0.2f);
    var player = await client.Request("/api/player").Get().SendAsync<PlayerData>(token);

    onProgress?.Invoke(0.5f);
    var inventory = await client.Request("/api/inventory").Get().SendAsync<InventoryData>(token);

    onProgress?.Invoke(0.8f);
    var quests = await client.Request("/api/quests").Get().SendAsync<QuestData[]>(token);

    onProgress?.Invoke(1f);
}
```

### Custom timeout

```csharp
await client
    .Request("/api/large-data")
    .Get()
    .WithTimeout(60f)
    .SendAsync<LargeData>(token);
```

### Cancel on destroy

```csharp
private ApiService _api;

void Awake() => _api = new ApiService("https://your-api.com");
void OnDestroy() => _api.Dispose();
```

## Testing with JSONPlaceholder

After importing samples:

1. Attach `ExampleUsage.cs` to a GameObject
2. Run the scene
3. Check the Console — request/response logs come from Project Settings

## Next steps

- Read the full [README.md](README.md)
- Check [AdvancedExamples.cs](Samples~/BasicExamples/AdvancedExamples.cs) for complex scenarios
- See [TokenRefreshExample.cs](Samples~/BasicExamples/TokenRefreshExample.cs) for JWT refresh

## Troubleshooting

### "UniTask not found"

Install UniTask via Package Manager: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`

### "Newtonsoft.Json not found"

Install via Package Manager: Add package `com.unity.nuget.newtonsoft-json`

### "GetAsync / PostAsync not found"

These were removed in **1.1.0**. Use the fluent API:

```csharp
await client.Request("/path").Get().SendAsync<T>(ct);
await client.Request("/path").Post().WithBody(body).SendAsync<T>(ct);
```

### Wrong UPM path after upgrade

Update your Git URL to use `?path=Assets/AdvancedWebRequestSDK` (not `Assets/AdvancedWebRequest`).

### Request times out immediately

Increase timeout per request with `.WithTimeout(60f)` or raise the default in Project Settings.
