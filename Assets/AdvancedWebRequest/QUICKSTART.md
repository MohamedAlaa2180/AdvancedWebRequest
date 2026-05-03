# Quick Start Guide

## Installation

1. Ensure you have the following packages installed:
   - **UniTask** (via Package Manager or UPM)
   - **Newtonsoft.Json** (via Package Manager or UPM)

2. Import the `AdvancedWebRequest` folder into your project

## 5-Minute Setup

### Step 1: Create Your API Client (MonoBehaviour)

```csharp
using UnityEngine;
using AdvancedWebRequest.Core;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ApiManager : MonoBehaviour
{
    private static ApiManager _instance;
    public static ApiManager Instance => _instance;

    private ApiClient _client;
    private SimpleTokenProvider _tokenProvider;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeClient();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeClient()
    {
        var config = ApiClientConfig.Create("https://your-api.com");
        
        // Optional: Configure automatic logging
        config.LogLevel = LogLevel.Detailed;  // None, Errors, Basic, Detailed, Verbose
        config.LogResponseBody = true;        // Show response bodies in logs
        
        _tokenProvider = new SimpleTokenProvider();
        _client = new ApiClient(config, _tokenProvider);
    }

    public async UniTask<T> Get<T>(string path, CancellationToken ct = default)
    {
        return await _client.GetAsync<T>(path, ct);
    }

    public async UniTask<T> Post<T>(string path, object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<T>(path, body, ct);
    }

    public void SetToken(string token)
    {
        _tokenProvider.SetToken(token);
    }
}
```

### Step 2: Use in Your Game Code

```csharp
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;
using System.Threading;
using System;

public class GameController : MonoBehaviour
{
    private CancellationTokenSource _cts;

    async void Start()
    {
        _cts = new CancellationTokenSource();
        await LoadPlayerData();
    }

    async UniTask LoadPlayerData()
    {
        try
        {
            var player = await ApiManager.Instance.Get<PlayerData>("/api/player", _cts.Token);
            Debug.Log($"Welcome back, {player.name}!");
        }
        catch (ApiException ex)
        {
            Debug.LogError($"Failed to load player: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
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

### Step 3: Handle Login

```csharp
public class LoginManager : MonoBehaviour
{
    private CancellationTokenSource _cts;

    async void Start()
    {
        _cts = new CancellationTokenSource();
    }

    public async UniTask<bool> Login(string email, string password)
    {
        var request = new LoginRequest { email = email, password = password };

        try
        {
            var response = await ApiManager.Instance.Post<LoginResponse>("/api/auth/login", request, _cts.Token);
            
            ApiManager.Instance.SetToken(response.token);
            
            Debug.Log("Login successful!");
            return true;
        }
        catch (ApiException ex) when (ex.Category == ApiErrorCategory.Unauthorized)
        {
            Debug.LogError("Invalid credentials");
            return false;
        }
        catch (ApiException ex)
        {
            Debug.LogError($"Login failed: {ex.Message}");
            return false;
        }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
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

## Automatic Logging (New!)

The package automatically logs all requests/responses - **no manual logging needed!**

```csharp
var config = ApiClientConfig.Create("https://api.example.com");

// Control logging verbosity
config.LogLevel = LogLevel.Detailed;  // None, Errors, Basic, Detailed, Verbose
config.LogResponseBody = true;        // Show response bodies

var client = new ApiClient(config);

// Just make your request - logging happens automatically!
var user = await client.GetAsync<User>("/api/users/me");

// Console automatically shows:
// [API] → GET https://api.example.com/api/users/me
// [API] ← 200 in 0.48s
// [API] Response Body: {"id":1,"name":"John"}
```

**Log Levels:**
- `None` - No logging (production)
- `Errors` - Only errors
- `Basic` - Request + response status (default)
- `Detailed` - Basic + bodies (debugging)
- `Verbose` - Everything including retries

See [LOGGING.md](LOGGING.md) for complete guide.

## Common Patterns

### Loading Screen with Progress

```csharp
public async UniTask LoadGameData(Action<float> onProgress)
{
    _cts = new CancellationTokenSource();

    try
    {
        onProgress?.Invoke(0.2f);
        var player = await ApiManager.Instance.Get<PlayerData>("/api/player", _cts.Token);
        
        onProgress?.Invoke(0.5f);
        var inventory = await ApiManager.Instance.Get<InventoryData>("/api/inventory", _cts.Token);
        
        onProgress?.Invoke(0.8f);
        var quests = await ApiManager.Instance.Get<QuestData[]>("/api/quests", _cts.Token);
        
        onProgress?.Invoke(1f);
        Debug.Log("All data loaded!");
    }
    catch (ApiException ex)
    {
        Debug.LogError($"Loading failed: {ex.Message}");
    }
}
```

### Retry with User Feedback

```csharp
public async UniTask<bool> SavePlayerData(PlayerData data)
{
    const int maxUserRetries = 3;
    
    for (int i = 0; i < maxUserRetries; i++)
    {
        try
        {
            await ApiManager.Instance.Post<object>("/api/player/save", data, _cts.Token);
            return true;
        }
        catch (ApiException ex) when (ex.Category == ApiErrorCategory.NetworkError)
        {
            if (i < maxUserRetries - 1)
            {
                ShowMessage($"Connection lost, retrying... ({i + 1}/{maxUserRetries})");
                await UniTask.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
    
    ShowMessage("Failed to save. Please check your connection.");
    return false;
}
```

### Cancel on Back Button

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        _cts?.Cancel();
        Debug.Log("Request canceled");
    }
}
```

## Testing with JSONPlaceholder

Use the included `ExampleUsage.cs` which connects to https://jsonplaceholder.typicode.com for testing:

1. Attach `ExampleUsage.cs` to a GameObject
2. Run the scene
3. Check Console for results

## Next Steps

- Read the full [README.md](README.md) for all features
- Check [AdvancedExamples.cs](Examples/AdvancedExamples.cs) for complex scenarios
- See [TokenRefreshExample.cs](Examples/TokenRefreshExample.cs) for auto-refresh JWT tokens

## Troubleshooting

### "UniTask not found"
Install UniTask via Package Manager: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`

### "Newtonsoft.Json not found"
Install via Package Manager: Add package `com.unity.nuget.newtonsoft-json`

### "Request times out immediately"
Check your `TimeoutSeconds` - default is 30 seconds. Increase if needed:
```csharp
var options = RequestOptions.WithTimeout(60f);
await client.SendJsonAsync<T>("/path", "GET", null, ct, options);
```
