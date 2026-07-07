# Implementation Summary

## What was built

A production-ready HTTP client library built on top of Unity's `UnityWebRequest` with modern features for mobile and PC games.

## Architecture overview

```
Assets/AdvancedWebRequestSDK/
├── Runtime/
│   ├── Core/
│   │   ├── ApiClient.cs          # Main client with request execution
│   │   ├── ApiService.cs         # ApiClient + CancellationTokenSource wrapper
│   │   └── RequestBuilder.cs     # Fluent API builder (public entry point)
│   ├── Models/
│   │   ├── ApiClientConfig.cs    # Client configuration + OnCreate delegate
│   │   ├── ApiException.cs       # Typed exception with categories
│   │   ├── ApiErrorResponse.cs   # Structured error DTO
│   │   ├── RequestSpec.cs        # Internal request specification
│   │   └── RequestOptions.cs     # Per-request options
│   ├── Policies/
│   │   └── RetryPolicy.cs        # Retry logic with backoff
│   ├── Interfaces/
│   │   ├── ITokenProvider.cs     # JWT token provider abstraction
│   │   └── ILogger.cs            # Logging abstraction
│   └── Utils/
│       ├── SimpleTokenProvider.cs
│       └── DefaultLogger.cs
├── Editor/
│   └── AdvancedWebRequestSettings.cs  # Project Settings page (EditorPrefs)
└── Samples~/BasicExamples/
    ├── ExampleUsage.cs
    ├── AdvancedExamples.cs
    ├── FluentApiExample.cs
    ├── LoggingExample.cs
    ├── TokenRefreshExample.cs
    └── TestGetRequest.cs
```

## Key components

### 1. ApiClient (core)

The main HTTP executor. **`SendJsonAsync` is internal** — consumers use `RequestBuilder` instead.

**Responsibilities:**

- Execute HTTP requests via UnityWebRequest
- Handle JSON serialization/deserialization
- Manage timeouts and cancellation
- Apply retry policies
- Inject authentication tokens
- Log requests/responses

### 2. RequestBuilder (public API)

The only public way to make requests:

```csharp
await client
    .Request("/users/1")
    .Get()
    .WithTimeout(10f)
    .WithHeader("X-Custom", "value")
    .SendAsync<User>(ct);
```

Methods: `Get()`, `Post()`, `Put()`, `Delete()`, `WithBody()`, `WithTimeout()`, `WithRetry()`, `NoRetry()`, `WithHeader()`.

### 3. ApiService (convenience)

Owns `ApiClient` + `CancellationTokenSource` for MonoBehaviours and services:

```csharp
var api = new ApiService("https://api.example.com");
await api.Client.Request("/path").Get().SendAsync<T>(api.Token);
api.Dispose();
```

### 4. AdvancedWebRequestSettings (Editor)

Registered via `[SettingsProvider]` at **Project/Advanced Web Request**. Applies EditorPrefs to `ApiClientConfig.Create()` through the `ApiClientConfig.OnCreate` delegate bridge (avoids Runtime → Editor assembly reference).

### 5. ApiException (error model)

**Categories:** `Canceled`, `Timeout`, `NetworkError`, `Unauthorized`, `Forbidden`, `NotFound`, `RateLimited`, `BadRequest`, `ServerError`, `JsonParseError`, `Unknown`

### 6. RetryPolicy (reliability)

Default: 3 retries, exponential backoff with jitter. Retries on timeout, network errors, 429, and 5xx.

## Usage patterns

### Simple GET

```csharp
var user = await api.Client.Request("/api/users/me").Get().SendAsync<User>(api.Token);
```

### POST with body

```csharp
var response = await api.Client
    .Request("/api/auth/login")
    .Post()
    .WithBody(loginRequest)
    .SendAsync<LoginResponse>(api.Token);
```

### Custom timeout

```csharp
await api.Client
    .Request("/api/large-data")
    .Get()
    .WithTimeout(60f)
    .SendAsync<LargeData>(api.Token);
```

### No retry

```csharp
await api.Client
    .Request("/api/payment")
    .Post()
    .WithBody(paymentData)
    .NoRetry()
    .SendAsync<Result>(api.Token);
```

### Parallel requests

```csharp
var client = api.Client;
var token = api.Token;

var (user, posts, comments) = await UniTask.WhenAll(
    client.Request("/api/users/1").Get().SendAsync<User>(token),
    client.Request("/api/posts?userId=1").Get().SendAsync<Post[]>(token),
    client.Request("/api/comments?userId=1").Get().SendAsync<Comment[]>(token)
);
```

### Error handling

```csharp
try
{
    await api.Client.Request("/api/users/me").Get().SendAsync<User>(api.Token);
}
catch (ApiException ex)
{
    switch (ex.Category)
    {
        case ApiErrorCategory.Unauthorized: break;
        case ApiErrorCategory.NetworkError: break;
        default: throw;
    }
}
```

## Configuration

### Editor defaults (recommended)

**Edit → Project Settings → Advanced Web Request**

### Code overrides

```csharp
var config = ApiClientConfig.Create("https://api.example.com");
config.DefaultHeaders["X-App-Version"] = Application.version;
config.DefaultRetryPolicy = RetryPolicy.Aggressive;
var api = new ApiService(config, tokenProvider);
```

## Dependencies

- Unity 2021.3+
- UniTask
- Newtonsoft.Json

Installed automatically when adding the package via UPM:

```text
https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequestSDK#release/latest
```

## Extension points

- **ITokenProvider** — Custom JWT / refresh logic
- **ILogger** — Custom log sinks
- **RetryPolicy** — Custom retry rules

## Breaking changes in 1.1.0

| Removed / changed | Replacement |
|---|---|
| `GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync` | Fluent `Request().Get/Post/Put/Delete().SendAsync()` |
| Public `SendJsonAsync` | Internal — use `RequestBuilder` |
| UPM path `Assets/AdvancedWebRequest` | `Assets/AdvancedWebRequestSDK` |
| Per-class logging `[SerializeField]` | Project Settings → Advanced Web Request |

## More docs

- [README.md](README.md) — Feature documentation
- [QUICKSTART.md](QUICKSTART.md) — 5-minute setup
- [LOGGING.md](LOGGING.md) — Automatic logging guide
- [FEATURES.md](FEATURES.md) — Comprehensive feature list
- Import **Basic Examples** from Package Manager for runnable code
