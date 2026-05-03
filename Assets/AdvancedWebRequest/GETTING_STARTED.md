# Getting Started with Advanced Web Request

## Installation (3 Steps)

### Step 1: Install Dependencies
Via Unity Package Manager, add:

1. **UniTask**: 
   - Add package from git URL: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`

2. **Newtonsoft.Json**: 
   - Add package by name: `com.unity.nuget.newtonsoft-json`

### Step 2: Import Advanced Web Request
The `AdvancedWebRequest` folder is already in your `Assets` directory.

### Step 3: Test Installation
1. Create a new GameObject in your scene
2. Attach the `ExampleUsage` script (found in `Assets/AdvancedWebRequest/Examples/`)
3. Run the scene
4. Check Console - you should see successful API calls

## Your First API Call (2 Minutes)

### 1. Create an API Manager

Create `Assets/Scripts/ApiManager.cs`:

```csharp
using UnityEngine;
using AdvancedWebRequest.Core;

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
        // Replace with your API URL
        var config = ApiClientConfig.Create("https://your-api.com");
        _tokenProvider = new SimpleTokenProvider();
        _client = new ApiClient(config, _tokenProvider);
    }

    public ApiClient Client => _client;
    
    public void SetToken(string token)
    {
        _tokenProvider.SetToken(token);
    }
}
```

### 2. Use in Your Game

Create `Assets/Scripts/GameController.cs`:

```csharp
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;
using System.Threading;

public class GameController : MonoBehaviour
{
    private CancellationTokenSource _cts;

    void Start()
    {
        _cts = new CancellationTokenSource();
        LoadPlayerData().Forget();
    }

    async UniTask LoadPlayerData()
    {
        try
        {
            var player = await ApiManager.Instance.Client
                .GetAsync<PlayerData>("/api/player", _cts.Token);
            
            Debug.Log($"Welcome, {player.name}!");
        }
        catch (ApiException ex)
        {
            Debug.LogError($"Failed: {ex.Message}");
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
        public string name;
        public int level;
    }
}
```

### 3. Run!
That's it! You now have a production-ready HTTP client.

## Common Tasks

### Login and Store Token

```csharp
public async UniTask<bool> Login(string email, string password)
{
    var request = new { email, password };
    
    try
    {
        var response = await ApiManager.Instance.Client
            .PostAsync<LoginResponse>("/api/auth/login", request);
        
        ApiManager.Instance.SetToken(response.token);
        return true;
    }
    catch (ApiException ex)
    {
        Debug.LogError($"Login failed: {ex.Message}");
        return false;
    }
}
```

### Handle Different Errors

```csharp
try
{
    await ApiManager.Instance.Client.GetAsync<User>("/api/users/me");
}
catch (ApiException ex)
{
    switch (ex.Category)
    {
        case ApiErrorCategory.Unauthorized:
            ShowLoginScreen();
            break;
        case ApiErrorCategory.NetworkError:
            ShowMessage("No internet connection");
            break;
        case ApiErrorCategory.Timeout:
            ShowMessage("Connection is slow");
            break;
        default:
            ShowMessage($"Error: {ex.Message}");
            break;
    }
}
```

### Load Multiple Things in Parallel

```csharp
var client = ApiManager.Instance.Client;

var (player, inventory, quests) = await UniTask.WhenAll(
    client.GetAsync<PlayerData>("/api/player", ct),
    client.GetAsync<Item[]>("/api/inventory", ct),
    client.GetAsync<Quest[]>("/api/quests", ct)
);
```

### Custom Timeout for Large Data

```csharp
var options = RequestOptions.WithTimeout(60f);

var data = await ApiManager.Instance.Client.SendJsonAsync<LargeData>(
    "/api/large-data",
    "GET",
    null,
    ct,
    options
);
```

### No Retry for Critical Operations

```csharp
var result = await ApiManager.Instance.Client.SendJsonAsync<Result>(
    "/api/payment",
    "POST",
    paymentData,
    ct,
    RequestOptions.NoRetry
);
```

## Next Steps

1. **Read [README.md](README.md)** - Full documentation
2. **Check [FEATURES.md](FEATURES.md)** - All features explained
3. **Review [Examples/](Examples/)** - Code examples for every feature
4. **See [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - Architecture details

## Quick Tips

### Always Use CancellationToken
```csharp
private CancellationTokenSource _cts;

void Start()
{
    _cts = new CancellationTokenSource();
}

void OnDestroy()
{
    _cts?.Cancel();
    _cts?.Dispose();
}
```

### Use `.Forget()` for Fire-and-Forget
```csharp
LoadData().Forget();  // Start but don't await
```

### Check API Response in Browser First
Before coding, test your API with:
- Postman
- Browser DevTools
- curl

### Define Your DTOs
Create classes matching your API responses:
```csharp
[Serializable]
public class User
{
    public int id;
    public string name;
    public string email;
}
```

### Use SerializeField for Inspector
```csharp
[SerializeField] private string _apiUrl = "https://api.example.com";
```

## Troubleshooting

### Build Errors
- Ensure UniTask is installed
- Ensure Newtonsoft.Json is installed
- Check assembly definition references

### Timeout Immediately
- Check if API URL is correct
- Verify internet connection
- Increase timeout: `RequestOptions.WithTimeout(60f)`

### Token Not Working
- Verify token format: `Authorization: Bearer <token>`
- Check token expiry
- Ensure `SetToken()` was called

### JSON Parse Error
- Verify DTO class matches API response
- Add `[Serializable]` attribute
- Check field names (case-sensitive)

## Need Help?

1. Check the Console for detailed error messages
2. Review example scripts in `Examples/` folder
3. Read error categories in `ApiException`
4. Enable logging to see request/response details

## Production Checklist

Before going live:
- [ ] Use HTTPS (not HTTP)
- [ ] Store tokens securely (not PlayerPrefs)
- [ ] Add proper error handling
- [ ] Test on mobile network
- [ ] Test with airplane mode
- [ ] Handle token refresh
- [ ] Add loading indicators
- [ ] Test cancellation on scene changes
- [ ] Log errors to analytics
- [ ] Add retry strategies for critical operations

Happy coding! 🚀
