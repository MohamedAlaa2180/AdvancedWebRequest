# Implementation Summary

## What Was Built

A production-ready HTTP client library built on top of Unity's `UnityWebRequest` with modern features for mobile and PC games.

## Architecture Overview

```
AdvancedWebRequest/
├── Core/
│   ├── ApiClient.cs          # Main client with request execution
│   └── RequestBuilder.cs     # Fluent API builder
├── Models/
│   ├── ApiClientConfig.cs    # Client configuration
│   ├── ApiException.cs       # Typed exception with categories
│   ├── ApiErrorResponse.cs   # Structured error DTO
│   ├── RequestSpec.cs        # Internal request specification
│   └── RequestOptions.cs     # Per-request options
├── Policies/
│   └── RetryPolicy.cs        # Retry logic with backoff
├── Interfaces/
│   ├── ITokenProvider.cs     # JWT token provider abstraction
│   └── ILogger.cs            # Logging abstraction
├── Utils/
│   ├── SimpleTokenProvider.cs    # Basic token provider
│   └── DefaultLogger.cs          # Console logger
└── Examples/
    ├── ExampleUsage.cs           # Basic usage
    ├── AdvancedExamples.cs       # All features demonstrated
    ├── FluentApiExample.cs       # Fluent API usage
    └── TokenRefreshExample.cs    # Token refresh pattern
```

## Key Components

### 1. ApiClient (Core)
The main entry point for all HTTP requests.

**Responsibilities:**
- Execute HTTP requests via UnityWebRequest
- Handle JSON serialization/deserialization
- Manage timeouts and cancellation
- Apply retry policies
- Inject authentication tokens
- Log requests/responses

**Key Methods:**
```csharp
UniTask<T> GetAsync<T>(string path, CancellationToken ct)
UniTask<T> PostAsync<T>(string path, object body, CancellationToken ct)
UniTask<T> PutAsync<T>(string path, object body, CancellationToken ct)
UniTask<T> DeleteAsync<T>(string path, CancellationToken ct)
UniTask<T> SendJsonAsync<T>(string path, string method, object body, CancellationToken ct, RequestOptions options)
```

### 2. ApiException (Error Model)
Comprehensive exception type with categorization.

**Categories:**
- `Canceled` - User/system canceled
- `Timeout` - Request exceeded timeout
- `NetworkError` - Connection issues
- `Unauthorized` (401)
- `Forbidden` (403)
- `NotFound` (404)
- `RateLimited` (429)
- `BadRequest` (4xx)
- `ServerError` (5xx)
- `JsonParseError` - Invalid JSON
- `Unknown` - Unexpected errors

**Properties:**
```csharp
ApiErrorCategory Category
int? StatusCode
ApiErrorResponse StructuredError  // Parsed error DTO (if available)
string RawResponseBody            // Fallback raw text
string Url
string Method
```

### 3. RetryPolicy (Reliability)
Configurable retry logic with exponential backoff.

**Default Behavior:**
- Max 3 retries
- Base delay: 1 second
- Max delay: 10 seconds
- Jitter enabled (reduces thundering herd)

**Retry Triggers:**
- Timeout
- Network errors
- HTTP 429 (Rate Limited)
- HTTP 5xx (Server Errors)

**Formula:**
```
delay = min(baseDelay * 2^(attempt-1), maxDelay)
delay += random(0, delay * 0.3)  // jitter
```

### 4. ITokenProvider (Auth)
Abstraction for JWT token management.

**Implementations:**
- `SimpleTokenProvider` - Static token storage
- `RefreshableTokenProvider` (example) - Auto-refresh pattern

**Interface:**
```csharp
UniTask<string> GetAccessTokenAsync(CancellationToken ct)
```

### 5. RequestBuilder (Fluent API)
Chainable request builder for readable code.

**Example:**
```csharp
await client
    .Request("/users/1")
    .Get()
    .WithTimeout(10f)
    .WithHeader("X-Custom", "value")
    .SendAsync<User>();
```

## Implementation Details

### Timeout Handling
Uses `UniTask.WhenAny` to race the request against a timeout:

```csharp
var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(timeout), ct);
var requestTask = request.SendWebRequest().ToUniTask(ct);
var completedTask = await UniTask.WhenAny(requestTask, timeoutTask);

if (completedTask == 1) // timeout won
{
    request.Abort();
    throw new ApiException(ApiErrorCategory.Timeout, ...);
}
```

### JSON Parsing (Thread-Safe)
Switches to thread pool for JSON parsing to avoid main thread blocking:

```csharp
await UniTask.SwitchToThreadPool();
var result = JsonConvert.DeserializeObject<T>(responseBody);
await UniTask.SwitchToMainThread();
```

### Error Response Parsing
Tries to parse structured error, falls back to raw text:

```csharp
ApiErrorResponse structuredError = null;
try {
    structuredError = JsonConvert.DeserializeObject<ApiErrorResponse>(responseBody);
} catch {
    // Fallback to raw text
}
```

### Resource Cleanup
Proper disposal of UnityWebRequest in finally block:

```csharp
try {
    // Execute request
}
finally {
    request?.Dispose();
}
```

## Configuration

### Basic Setup
```csharp
var config = ApiClientConfig.Create("https://api.example.com");
var tokenProvider = new SimpleTokenProvider("your-jwt");
var client = new ApiClient(config, tokenProvider);
```

### Advanced Setup
```csharp
var config = ApiClientConfig.Create("https://api.example.com");

// Custom headers
config.DefaultHeaders["X-App-Version"] = Application.version;
config.DefaultHeaders["X-Platform"] = Application.platform.ToString();

// Custom retry policy
config.DefaultRetryPolicy = new RetryPolicy
{
    MaxRetries = 5,
    BaseDelaySeconds = 0.5f,
    MaxDelaySeconds = 5f,
    UseJitter = true
};

// Custom request defaults
config.DefaultRequestOptions = new RequestOptions
{
    TimeoutSeconds = 60f
};

// Custom logger
var logger = new CustomLogger();
var client = new ApiClient(config, tokenProvider, logger);
```

## Usage Patterns

### 1. Simple GET Request
```csharp
var user = await client.GetAsync<User>("/api/users/me", ct);
```

### 2. POST with Body
```csharp
var request = new LoginRequest { Email = "user@example.com", Password = "pass" };
var response = await client.PostAsync<LoginResponse>("/api/auth/login", request, ct);
```

### 3. Custom Options
```csharp
var options = RequestOptions.WithTimeout(60f);
var data = await client.SendJsonAsync<Data>("/path", "GET", null, ct, options);
```

### 4. No Retry (Critical Operations)
```csharp
var result = await client.SendJsonAsync<Result>(
    "/api/payment",
    "POST",
    paymentData,
    ct,
    RequestOptions.NoRetry
);
```

### 5. Fluent API
```csharp
var user = await client
    .Request("/users/1")
    .Get()
    .WithTimeout(10f)
    .NoRetry()
    .SendAsync<User>(ct);
```

### 6. Error Handling
```csharp
try {
    await client.GetAsync<User>("/api/users/me", ct);
} catch (ApiException ex) {
    switch (ex.Category) {
        case ApiErrorCategory.Unauthorized:
            // Redirect to login
            break;
        case ApiErrorCategory.NetworkError:
            // Show "no internet" message
            break;
        case ApiErrorCategory.Timeout:
            // Show "slow connection" message
            break;
        default:
            Debug.LogError($"Error: {ex}");
            break;
    }
}
```

### 7. Cancellation
```csharp
private CancellationTokenSource _cts;

void Start() {
    _cts = new CancellationTokenSource();
    LoadData(_cts.Token).Forget();
}

void OnDestroy() {
    _cts?.Cancel();
    _cts?.Dispose();
}
```

### 8. Parallel Requests
```csharp
var (user, posts, comments) = await UniTask.WhenAll(
    client.GetAsync<User>("/api/users/1", ct),
    client.GetAsync<Post[]>("/api/posts?userId=1", ct),
    client.GetAsync<Comment[]>("/api/comments?userId=1", ct)
);
```

## Testing

### With JSONPlaceholder (Public Test API)
The `ExampleUsage.cs` uses https://jsonplaceholder.typicode.com:

1. Attach `ExampleUsage` to a GameObject
2. Run the scene
3. Check Console for results

### With Your API
1. Update base URL in `ApiClientConfig.Create("https://your-api.com")`
2. Set JWT token via `SimpleTokenProvider` or implement `RefreshableTokenProvider`
3. Define your DTOs (request/response classes)
4. Make requests

## Dependencies

### Required
- **Unity 2021.3+**
- **UniTask** - For async/await support
- **Newtonsoft.Json** - For JSON serialization

### Installation
```
UniTask: https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
Newtonsoft.Json: com.unity.nuget.newtonsoft-json (via Package Manager)
```

## Extension Points

### 1. Custom Token Provider
Implement `ITokenProvider` for custom token logic:

```csharp
public class MyTokenProvider : ITokenProvider
{
    public async UniTask<string> GetAccessTokenAsync(CancellationToken ct)
    {
        // Your logic (e.g., refresh if expired)
        return "token";
    }
}
```

### 2. Custom Logger
Implement `ILogger` for custom logging:

```csharp
public class MyLogger : ILogger
{
    public void LogInfo(string message) { /* Your logic */ }
    public void LogWarning(string message) { /* Your logic */ }
    public void LogError(string message) { /* Your logic */ }
}
```

### 3. Custom Retry Policy
Extend `RetryPolicy` for custom retry logic:

```csharp
public class MyRetryPolicy : RetryPolicy
{
    public override bool ShouldRetry(ApiException ex, int attempt)
    {
        // Your custom retry logic
        return base.ShouldRetry(ex, attempt);
    }
}
```

## Performance

- **Memory**: Minimal overhead (just wrapper objects)
- **CPU**: JSON parsing on thread pool (non-blocking)
- **Latency**: No additional latency vs raw UnityWebRequest
- **Battery**: Optimized retry policies prevent excessive requests

## Security Considerations

1. **JWT Storage**: Store tokens securely (PlayerPrefs is NOT secure)
2. **HTTPS**: Always use HTTPS in production
3. **Token Refresh**: Implement proper token refresh to avoid storing long-lived tokens
4. **Logging**: Don't log sensitive data (passwords, tokens) - truncated by default

## Next Steps

1. **Install Dependencies**: UniTask + Newtonsoft.Json
2. **Configure**: Set your API base URL
3. **Implement Auth**: Set up token provider
4. **Define DTOs**: Create request/response classes
5. **Test**: Use examples as reference
6. **Production**: Add error handling and logging

## Known Limitations

1. **UnityWebRequest Limitations**: Inherits all UnityWebRequest constraints
2. **WebGL CORS**: Subject to browser CORS policies
3. **Download Progress**: Limited by UnityWebRequest API
4. **File Uploads**: Multipart form not implemented (can be extended)
5. **Certificate Pinning**: Not implemented (security concern)

## Troubleshooting

### "UniTask not found"
Install via Package Manager: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`

### "Newtonsoft.Json not found"
Install via Package Manager: Add package `com.unity.nuget.newtonsoft-json`

### "Timeout immediately"
Check `TimeoutSeconds` configuration (default is 30s)

### "Retry not working"
Verify error category is retryable (check `ApiException.IsRetryable`)

### "Token not injected"
Ensure `ITokenProvider` is passed to `ApiClient` constructor

## Support

- Check `README.md` for feature documentation
- See `QUICKSTART.md` for 5-minute setup
- Review `Examples/` folder for usage patterns
- Read `FEATURES.md` for comprehensive feature list
