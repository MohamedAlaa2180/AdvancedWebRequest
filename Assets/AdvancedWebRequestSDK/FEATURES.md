# Advanced Web Request - Feature Overview

## Core Features

### 🚀 Modern Async/Await with UniTask
- Clean async/await syntax using UniTask
- Proper CancellationToken support
- No callback hell
- Easy integration with Unity's main thread

```csharp
var user = await client.Request("/api/users/me").Get().SendAsync<User>(cancellationToken);
```

### 🔐 JWT Authentication
- Automatic Bearer token injection
- Extensible `ITokenProvider` interface
- Support for token refresh patterns
- Per-request token override

```csharp
var tokenProvider = new SimpleTokenProvider("your-jwt-token");
var client = new ApiClient(config, tokenProvider);
```

### 🔄 Smart Retry Logic
- Exponential backoff with jitter
- Configurable max retries
- Automatic retry on:
  - Network errors
  - HTTP 429 (Rate Limited)
  - HTTP 5xx (Server Errors)
  - Timeouts
- Skip retry for non-idempotent operations

```csharp
config.DefaultRetryPolicy = new RetryPolicy
{
    MaxRetries = 3,
    BaseDelaySeconds = 1f,
    MaxDelaySeconds = 10f,
    UseJitter = true
};
```

### ⏱️ Timeout Control
- Per-request timeout configuration
- Proper request abortion on timeout
- No hanging connections
- Mobile-friendly defaults (30s)

```csharp
await client.Request("/path").Get().WithTimeout(60f).SendAsync<T>(ct);
```

### 🎯 Structured Error Handling
- Typed error categories
- Automatic error response parsing
- Fallback to raw text
- Rich exception details

```csharp
try {
    await client.Request("/api/users/me").Get().SendAsync<User>();
} catch (ApiException ex) {
    Debug.Log($"Category: {ex.Category}");
    Debug.Log($"Status: {ex.StatusCode}");
    Debug.Log($"Error: {ex.StructuredError?.Message}");
}
```

### 📦 JSON Serialization
- Newtonsoft.Json integration
- Automatic request/response serialization
- Generic type support
- Thread-safe parsing (switches to thread pool)

```csharp
var request = new LoginRequest { Email = "user@example.com", Password = "pass" };
var response = await client.Request("/auth/login").Post().WithBody(request).SendAsync<LoginResponse>();
```

### 📝 Configurable Logging
- **Project Settings page** for centralized defaults (EditorPrefs)
- `ILogger` interface for custom logging
- Request/response logging with timing
- Retry attempt logging
- Color-coded console output

Configure in **Edit → Project Settings → Advanced Web Request**, or override per client in code.

### 🔗 Fluent Request Builder (public API)
- **Single entry point** for all HTTP requests
- Chainable API design
- Per-request configuration override

```csharp
var user = await client
    .Request("/users/1")
    .Get()
    .WithTimeout(10f)
    .NoRetry()
    .WithHeader("X-Custom", "value")
    .SendAsync<User>();
```

### 🧩 ApiService helper
- Encapsulates `ApiClient` + `CancellationTokenSource`
- Reduces boilerplate in MonoBehaviours and services
- Implements `IDisposable` for clean shutdown

```csharp
var api = new ApiService("https://api.example.com");
await api.Client.Request("/data").Get().SendAsync<Data>(api.Token);
api.Dispose();
```

### 🔁 Cancellation Support
- Full CancellationToken integration
- Proper cleanup on cancellation
- Safe for scene transitions

```csharp
private ApiService _api;

void Start() {
    _api = new ApiService("https://api.example.com");
    LoadData().Forget();
}

void OnDestroy() {
    _api?.Dispose();
}
```

## Advanced Features

### 🔄 Token Refresh Pattern
Built-in support for automatic JWT token refresh:

```csharp
public class RefreshableTokenProvider : ITokenProvider
{
    public async UniTask<string> GetAccessTokenAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow < _expiresAt.AddMinutes(-5))
            return _accessToken;
            
        await RefreshTokenAsync(ct);
        return _accessToken;
    }
}
```

### ⚡ Parallel Requests
Easy parallel request execution with UniTask:

```csharp
var (user, posts, comments) = await UniTask.WhenAll(
    client.Request("/api/users/1").Get().SendAsync<User>(),
    client.Request("/api/posts?userId=1").Get().SendAsync<Post[]>(),
    client.Request("/api/comments?userId=1").Get().SendAsync<Comment[]>()
);
```

### 🎛️ Flexible Configuration
- Global defaults with per-request overrides
- Custom headers
- Environment-based base URLs
- Retry policy customization

```csharp
var config = ApiClientConfig.Create("https://api.example.com");
config.DefaultHeaders["X-App-Version"] = Application.version;
config.DefaultHeaders["X-Platform"] = Application.platform.ToString();
config.DefaultRetryPolicy = RetryPolicy.Aggressive;
```

### 📊 Error Categories
Comprehensive error categorization for better handling:

- `Canceled` - Request was canceled
- `Timeout` - Request exceeded timeout
- `NetworkError` - Connection/DNS/TLS errors
- `Unauthorized` - HTTP 401
- `Forbidden` - HTTP 403
- `NotFound` - HTTP 404
- `RateLimited` - HTTP 429
- `BadRequest` - HTTP 4xx
- `ServerError` - HTTP 5xx
- `JsonParseError` - Failed to parse JSON
- `Unknown` - Unexpected errors

### 🛠️ Extensibility Points

#### Custom Token Provider
```csharp
public interface ITokenProvider
{
    UniTask<string> GetAccessTokenAsync(CancellationToken ct = default);
}
```

#### Custom Logger
```csharp
public interface ILogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
}
```

#### Custom Retry Policy
```csharp
public class CustomRetryPolicy : RetryPolicy
{
    public override bool ShouldRetry(ApiException exception, int attemptNumber)
    {
        // Your custom logic
    }
}
```

## Mobile Optimizations

### Network Resilience
- Automatic retry on spotty connections
- Exponential backoff to prevent battery drain
- Jitter to prevent thundering herd
- Proper timeout handling

### Memory Efficiency
- Proper `UnityWebRequest` disposal
- Thread pool for JSON parsing
- No memory leaks across scene changes
- Efficient cancellation cleanup

### Battery Friendly
- Configurable retry limits
- Smart backoff strategies
- Timeout controls
- No infinite loops

## Production Ready

### ✅ Battle-Tested Patterns
- Based on industry-standard HTTP client designs
- Clean separation of concerns
- Testable architecture
- SOLID principles

### ✅ Complete Error Handling
- Never swallows exceptions
- Rich error context
- Structured and raw error support
- Proper categorization

### ✅ Documentation
- Comprehensive README
- Quick start guide
- Multiple examples
- Inline code documentation

### ✅ Maintainable
- Clean code structure
- Clear naming conventions
- Modular design
- Easy to extend

## What's NOT Included (By Design)

These features are intentionally excluded to keep the library focused:

- ❌ Request caching (implement in your layer if needed)
- ❌ Rate limiting (implement in interceptors if needed)
- ❌ Request de-duplication (can be added via custom logic)
- ❌ Multipart form uploads (can be extended)
- ❌ Download progress (UnityWebRequest limitation)
- ❌ Certificate pinning (security layer concern)
- ❌ Proxy support (transport layer concern)

## Comparison with Unity's UnityWebRequest

| Feature | UnityWebRequest | AdvancedWebRequest |
|---------|----------------|-------------------|
| Async/Await | ❌ (callbacks only) | ✅ UniTask |
| Cancellation | ⚠️ (manual abort) | ✅ CancellationToken |
| Retry Logic | ❌ | ✅ Automatic |
| Timeout | ⚠️ (basic) | ✅ Proper |
| JSON | ❌ (manual) | ✅ Automatic |
| JWT Auth | ❌ | ✅ Built-in |
| Error Handling | ⚠️ (basic) | ✅ Structured |
| Logging | ❌ | ✅ Configurable |
| Type Safety | ❌ | ✅ Generics |

## Performance

- **Overhead**: Minimal (just abstraction layer)
- **Memory**: Same as UnityWebRequest + small objects
- **CPU**: JSON parsing on thread pool (non-blocking)
- **Latency**: No additional latency vs raw UnityWebRequest

## Use Cases

Perfect for:
- ✅ Mobile games with REST APIs
- ✅ Backend-connected PC games
- ✅ Live service games
- ✅ Multiplayer games with REST APIs
- ✅ Games with user accounts
- ✅ Analytics integration
- ✅ In-game purchases API

Not ideal for:
- ❌ Real-time multiplayer (use WebSocket/UDP)
- ❌ Large file downloads (use Unity's download handler)
- ❌ WebGL with CORS issues (browser limitations)
