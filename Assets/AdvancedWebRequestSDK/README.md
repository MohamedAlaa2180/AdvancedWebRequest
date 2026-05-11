# Advanced Web Request

A production-ready HTTP client built on top of Unity's UnityWebRequest with modern features for mobile and PC games.

## Install (UPM)

Add via **Package Manager → + → Add package from git URL** using:

`https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequest#release/latest`

Or add to `Packages/manifest.json` as `com.mohamedalaa2180.advancedwebrequest` with that URL. See the [repository README](https://github.com/MohamedAlaa2180/AdvancedWebRequest/blob/release/latest/README.md) for details.

## Features

- ✅ **UniTask async/await** - Clean async code with proper cancellation
- ✅ **JWT Authentication** - Automatic Bearer token injection
- ✅ **Retry with Backoff** - Exponential backoff + jitter for mobile networks
- ✅ **Timeout Control** - Hard timeouts to prevent hanging requests
- ✅ **Structured Errors** - Typed error responses with fallback
- ✅ **JSON Serialization** - Newtonsoft.Json integration
- ✅ **Automatic Logging** - Configurable log levels with zero manual logging needed
- ✅ **Cancellation Support** - CancellationToken for clean cancellation

## Quick Start

### 1. Basic Setup

```csharp
using AdvancedWebRequest.Core;
using Cysharp.Threading.Tasks;

var config = ApiClientConfig.Create("https://api.example.com");
var tokenProvider = new SimpleTokenProvider("your-jwt-token");
var client = new ApiClient(config, tokenProvider);
```

### 2. Making Requests

```csharp
// GET request
var user = await client.GetAsync<UserResponse>("/users/me");

// POST request
var loginRequest = new LoginRequest { Email = "user@example.com", Password = "pass" };
var response = await client.PostAsync<LoginResponse>("/auth/login", loginRequest);

// PUT request
var updateRequest = new UpdateProfileRequest { Name = "New Name" };
await client.PutAsync<UserResponse>("/users/me", updateRequest);

// DELETE request
await client.DeleteAsync<object>("/users/123");
```

### 3. Cancellation

```csharp
private CancellationTokenSource _cts;

async UniTask LoadData()
{
    _cts = new CancellationTokenSource();
    
    try
    {
        var data = await client.GetAsync<DataResponse>("/data", _cts.Token);
    }
    catch (ApiException ex) when (ex.Category == ApiErrorCategory.Canceled)
    {
        Debug.Log("Request canceled");
    }
}

void OnDestroy()
{
    _cts?.Cancel();
    _cts?.Dispose();
}
```

### 4. Error Handling

```csharp
try
{
    var response = await client.GetAsync<UserResponse>("/users/me");
}
catch (ApiException ex)
{
    switch (ex.Category)
    {
        case ApiErrorCategory.Unauthorized:
            Debug.Log("Need to login");
            break;
        case ApiErrorCategory.Timeout:
            Debug.Log("Request timed out");
            break;
        case ApiErrorCategory.NetworkError:
            Debug.Log("No internet connection");
            break;
        default:
            Debug.LogError($"Error: {ex.Message}");
            break;
    }
    
    if (ex.StructuredError != null)
    {
        Debug.Log($"Error code: {ex.StructuredError.Code}");
    }
}
```

### 5. Automatic Logging (No Manual Logging Needed!)

The package handles all logging automatically based on your configuration:

```csharp
var config = ApiClientConfig.Create("https://api.example.com");

// Configure logging (all optional)
config.LogLevel = LogLevel.Detailed;     // None, Errors, Basic, Detailed, Verbose
config.LogRequestBody = true;            // Show request bodies
config.LogResponseBody = true;           // Show response bodies
config.MaxLogBodyLength = 500;           // Truncate long bodies

var client = new ApiClient(config);

// That's it! All requests are now automatically logged
var user = await client.GetAsync<User>("/users/me");
// Console: [API] → GET https://api.example.com/users/me
// Console: [API] ← 200 in 0.48s
// Console: [API] Response Body: {"id":1,"name":"John"}
```

**Log Levels:**
- `None` - No logging at all
- `Errors` - Only log errors and exceptions
- `Basic` - Request URL + response status + timing (default)
- `Detailed` - Basic + request/response bodies (if enabled)
- `Verbose` - Everything including retry attempts and debug info

**Inspector-Friendly:** All log settings can be configured as `[SerializeField]` for easy toggling in Unity Inspector!

### 6. Custom Options

```csharp
// Custom timeout
var options = RequestOptions.WithTimeout(60f);
var data = await client.SendJsonAsync<DataResponse>("/large-data", "GET", null, default, options);

// No retry
var criticalData = await client.SendJsonAsync<Response>("/critical", "POST", body, default, RequestOptions.NoRetry);

// Aggressive retry
var config = ApiClientConfig.Create("https://api.example.com");
config.DefaultRetryPolicy = RetryPolicy.Aggressive;
```

### 7. Advanced: Token Refresh

```csharp
public class RefreshableTokenProvider : ITokenProvider
{
    private string _accessToken;
    private string _refreshToken;
    private DateTime _expiresAt;

    public async UniTask<string> GetAccessTokenAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow < _expiresAt)
            return _accessToken;

        await RefreshTokenAsync(ct);
        return _accessToken;
    }

    private async UniTask RefreshTokenAsync(CancellationToken ct)
    {
        // Call refresh endpoint
        // Update _accessToken and _expiresAt
    }
}
```

## Configuration

### Retry Policy

```csharp
config.DefaultRetryPolicy = new RetryPolicy
{
    MaxRetries = 3,
    BaseDelaySeconds = 1f,
    MaxDelaySeconds = 10f,
    UseJitter = true
};
```

### Default Headers

```csharp
config.DefaultHeaders["X-App-Version"] = Application.version;
config.DefaultHeaders["X-Platform"] = Application.platform.ToString();
```

### Custom Logger (Optional)

The default logger works great for most cases, but you can customize it:

```csharp
public class CustomLogger : ILogger
{
    // Basic logging
    public void LogInfo(string message) => Debug.Log(message);
    public void LogWarning(string message) => Debug.LogWarning(message);
    public void LogError(string message) => Debug.LogError(message);
    
    // Structured logging (called automatically by the client)
    public void LogRequest(string method, string url, object body = null)
    {
        // Your custom request logging
    }
    
    public void LogResponse(int statusCode, float duration, string body = null)
    {
        // Your custom response logging
    }
    
    public void LogException(ApiException exception)
    {
        // Your custom error logging
    }
}

var client = new ApiClient(config, tokenProvider, new CustomLogger());
```

## Error Categories

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

## Requirements

- Unity 2021.3+
- UniTask package
- Newtonsoft.Json package
