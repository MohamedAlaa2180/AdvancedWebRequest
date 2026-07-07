# Getting Started with Advanced Web Request

## Installation

### Step 1: Add the package (UPM)

Via **Package Manager → + → Add package from git URL**:

```text
https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequestSDK#release/latest
```

Or add to `Packages/manifest.json`:

```json
"com.mohamedalaa2180.advancedwebrequest": "https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequestSDK#release/latest"
```

UniTask and Newtonsoft.Json are installed automatically.

### Step 2: Configure defaults

Open **Edit → Project Settings → Advanced Web Request** and set log level, body logging, and default timeout.

### Step 3: Import samples (optional)

In Package Manager → **Advanced Web Request** → **Samples** → import **Basic Examples**.

### Step 4: Test installation

1. Attach `ExampleUsage` from the imported samples to a GameObject
2. Run the scene
3. Check the Console for automatic API logs

## Your first API call (2 minutes)

### 1. Create an API manager

Create `Assets/Scripts/ApiManager.cs`:

```csharp
using UnityEngine;
using System.Threading;
using AdvancedWebRequest.Core;

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

### 2. Use in your game

Create `Assets/Scripts/GameController.cs`:

```csharp
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdvancedWebRequest.Core;

public class GameController : MonoBehaviour
{
    void Start()
    {
        LoadPlayerData().Forget();
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
        public string name;
        public int level;
    }
}
```

### 3. Run

That is it — you now have a production-ready HTTP client with automatic logging.

## Common tasks

### Login and store token

```csharp
public async UniTask<bool> Login(string email, string password)
{
    var request = new { email, password };

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
    catch (ApiException)
    {
        return false;
    }
}
```

### Handle different errors

```csharp
try
{
    await ApiManager.Instance.Client
        .Request("/api/users/me")
        .Get()
        .SendAsync<User>(ApiManager.Instance.Token);
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
            throw;
    }
}
```

### Load multiple things in parallel

```csharp
var client = ApiManager.Instance.Client;
var token = ApiManager.Instance.Token;

var (player, inventory, quests) = await UniTask.WhenAll(
    client.Request("/api/player").Get().SendAsync<PlayerData>(token),
    client.Request("/api/inventory").Get().SendAsync<Item[]>(token),
    client.Request("/api/quests").Get().SendAsync<Quest[]>(token)
);
```

### Custom timeout for large data

```csharp
var data = await ApiManager.Instance.Client
    .Request("/api/large-data")
    .Get()
    .WithTimeout(60f)
    .SendAsync<LargeData>(ApiManager.Instance.Token);
```

### No retry for critical operations

```csharp
var result = await ApiManager.Instance.Client
    .Request("/api/payment")
    .Post()
    .WithBody(paymentData)
    .NoRetry()
    .SendAsync<Result>(ApiManager.Instance.Token);
```

## Next steps

1. **Read [README.md](README.md)** — Full documentation
2. **Check [FEATURES.md](FEATURES.md)** — All features explained
3. **Review samples** — Import **Basic Examples** from Package Manager
4. **See [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** — Architecture details

## Quick tips

### Use ApiService for lifecycle

```csharp
private ApiService _api;

void Awake() => _api = new ApiService("https://api.example.com");
void OnDestroy() => _api.Dispose();
```

### Use `.Forget()` for fire-and-forget

```csharp
LoadData().Forget();
```

### Define your DTOs

```csharp
[Serializable]
public class User
{
    public int id;
    public string name;
    public string email;
}
```

## Troubleshooting

### Build errors

- Ensure the UPM path is `Assets/AdvancedWebRequestSDK`
- Ensure UniTask and Newtonsoft.Json resolved in Package Manager

### GetAsync / PostAsync not found

Use the fluent API (removed in 1.1.0):

```csharp
await client.Request("/path").Get().SendAsync<T>(ct);
```

### Token not working

- Verify token format: `Authorization: Bearer <token>`
- Ensure `SetToken()` was called

### JSON parse error

- Verify DTO class matches API response
- Add `[Serializable]` attribute
- Field names are case-sensitive

## Production checklist

Before going live:

- [ ] Use HTTPS (not HTTP)
- [ ] Store tokens securely (not PlayerPrefs)
- [ ] Add proper error handling
- [ ] Set log level to `Errors` or `None` for release builds
- [ ] Test on mobile network
- [ ] Test with airplane mode
- [ ] Handle token refresh
- [ ] Test cancellation on scene changes
