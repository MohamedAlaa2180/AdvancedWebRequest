# Advanced Web Request

A production-ready HTTP client built on top of Unity's UnityWebRequest with modern features for mobile and PC games.

## Install (UPM)

Add via **Package Manager → + → Add package from git URL** using:

`https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequestSDK#release/latest`

Or add to `Packages/manifest.json` as `com.mohamedalaa2180.advancedwebrequest` with that URL. See the [repository README](https://github.com/MohamedAlaa2180/AdvancedWebRequest/blob/release/latest/README.md) for details.

Pin a specific release with `#v1.1.0` instead of `#release/latest`.

### Samples

In Package Manager, select **Advanced Web Request** → **Samples** → import **Basic Examples**.

## Features

- ✅ **UniTask async/await** — Clean async code with proper cancellation
- ✅ **Fluent request API** — Single entry point: `Request().Get().SendAsync<T>()`
- ✅ **ApiService helper** — Owns `ApiClient` + `CancellationTokenSource` lifecycle
- ✅ **Editor Project Settings** — Centralized logging and default timeout
- ✅ **JWT Authentication** — Automatic Bearer token injection
- ✅ **Retry with Backoff** — Exponential backoff + jitter for mobile networks
- ✅ **Timeout Control** — Hard timeouts to prevent hanging requests
- ✅ **Structured Errors** — Typed error responses with fallback
- ✅ **JSON Serialization** — Newtonsoft.Json integration
- ✅ **Automatic Logging** — Configurable log levels with zero manual logging needed
- ✅ **Cancellation Support** — CancellationToken for clean cancellation

## Quick Start

### 1. Configure logging (Editor)

Open **Edit → Project Settings → Advanced Web Request** and set:

- Log level
- Request/response body logging
- Default timeout

These defaults apply automatically when you call `ApiClientConfig.Create()`.

### 2. Basic setup

```csharp
using AdvancedWebRequest.Core;
using Cysharp.Threading.Tasks;

// Simple setup
var api = new ApiService("https://api.example.com");

// With JWT
var tokenProvider = new SimpleTokenProvider("your-jwt-token");
var apiWithAuth = new ApiService(ApiClientConfig.Create("https://api.example.com"), tokenProvider);
```

### 3. Making requests

All HTTP calls use the fluent builder:

```csharp
// GET
var user = await api.Client
    .Request("/users/me")
    .Get()
    .SendAsync<UserResponse>(api.Token);

// POST
var loginRequest = new LoginRequest { Email = "user@example.com", Password = "pass" };
var response = await api.Client
    .Request("/auth/login")
    .Post()
    .WithBody(loginRequest)
    .SendAsync<LoginResponse>(api.Token);

// PUT
var updateRequest = new UpdateProfileRequest { Name = "New Name" };
await api.Client
    .Request("/users/me")
    .Put()
    .WithBody(updateRequest)
    .SendAsync<UserResponse>(api.Token);

// DELETE
await api.Client
    .Request("/users/123")
    .Delete()
    .SendAsync<object>(api.Token);
```

Pass request bodies as objects — the SDK serializes them. Do **not** call `JsonConvert.SerializeObject` yourself.

### 4. MonoBehaviour pattern

```csharp
public class MyScreen : MonoBehaviour
{
    [SerializeField] private string _baseUrl = "https://api.example.com";
    [SerializeField] private float _timeout = 30f;

    private ApiService _api;

    void Awake() => _api = new ApiService(_baseUrl);
    void OnDestroy() => _api.Dispose();

    async UniTask LoadData()
    {
        await _api.Client
            .Request("/data")
            .Get()
            .WithTimeout(_timeout)
            .SendAsync<DataResponse>(_api.Token);
    }
}
```

### 5. Error handling

```csharp
try
{
    var response = await api.Client
        .Request("/users/me")
        .Get()
        .SendAsync<UserResponse>(api.Token);
}
catch (ApiException ex)
{
    switch (ex.Category)
    {
        case ApiErrorCategory.Unauthorized:
            // Redirect to login
            break;
        case ApiErrorCategory.Timeout:
            // Show slow connection message
            break;
        case ApiErrorCategory.NetworkError:
            // Show offline message
            break;
        default:
            throw;
    }
}
```

### 6. Request options

```csharp
// Custom timeout
await api.Client
    .Request("/large-data")
    .Get()
    .WithTimeout(60f)
    .SendAsync<DataResponse>(api.Token);

// No retry (critical operations)
await api.Client
    .Request("/payment")
    .Post()
    .WithBody(paymentData)
    .NoRetry()
    .SendAsync<PaymentResponse>(api.Token);

// Custom header
await api.Client
    .Request("/users/me")
    .Get()
    .WithHeader("X-Custom", "value")
    .SendAsync<UserResponse>(api.Token);
```

### 7. Token refresh

See `Samples~/BasicExamples/TokenRefreshExample.cs` after importing samples.

## Package layout

```
Assets/AdvancedWebRequestSDK/
├── Runtime/          # Core SDK (ApiClient, RequestBuilder, ApiService, …)
├── Editor/           # Project Settings page
├── Samples~/         # Optional importable examples
├── package.json
└── README.md
```

## Configuration

### Retry policy

```csharp
var config = ApiClientConfig.Create("https://api.example.com");
config.DefaultRetryPolicy = new RetryPolicy
{
    MaxRetries = 3,
    BaseDelaySeconds = 1f,
    MaxDelaySeconds = 10f,
    UseJitter = true
};
```

### Default headers

```csharp
config.DefaultHeaders["X-App-Version"] = Application.version;
config.DefaultHeaders["X-Platform"] = Application.platform.ToString();
```

### Custom logger

```csharp
var client = new ApiClient(config, tokenProvider, new CustomLogger());
```

See [LOGGING.md](LOGGING.md) for the full logging guide.

## Error categories

- `Canceled` — Request was canceled
- `Timeout` — Request exceeded timeout
- `NetworkError` — Connection/DNS/TLS errors
- `Unauthorized` — HTTP 401
- `Forbidden` — HTTP 403
- `NotFound` — HTTP 404
- `RateLimited` — HTTP 429
- `BadRequest` — HTTP 4xx
- `ServerError` — HTTP 5xx
- `JsonParseError` — Failed to parse JSON
- `Unknown` — Unexpected errors

## Requirements

- Unity 2021.3+
- UniTask (installed automatically via UPM)
- Newtonsoft.Json (installed automatically via UPM)

## More docs

- [QUICKSTART.md](QUICKSTART.md) — 5-minute setup
- [GETTING_STARTED.md](GETTING_STARTED.md) — Step-by-step tutorial
- [LOGGING.md](LOGGING.md) — Automatic logging guide
- [FEATURES.md](FEATURES.md) — Full feature list
- [CHANGELOG.md](CHANGELOG.md) — Version history

## Migrating from 1.0.x

1. Update your UPM Git URL path to `Assets/AdvancedWebRequestSDK`.
2. Replace `GetAsync` / `PostAsync` / `PutAsync` / `DeleteAsync` / `SendJsonAsync` with the fluent API.
3. Move logging config from per-class `[SerializeField]` fields to **Project Settings → Advanced Web Request**.
4. Consider `ApiService` instead of managing `ApiClient` + `CancellationTokenSource` manually.
