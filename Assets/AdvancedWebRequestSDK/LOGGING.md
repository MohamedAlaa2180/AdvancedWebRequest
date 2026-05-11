# Automatic Logging Guide

## Overview

The Advanced Web Request package includes **automatic logging** that eliminates the need for manual logging in your code. Simply configure the log level once, and all requests/responses are logged automatically.

## Quick Comparison

### ❌ Before (Manual Logging)

```csharp
async UniTask LoadUser()
{
    try
    {
        Debug.Log($"Loading user from {url}...");
        
        var user = await client.GetAsync<User>("/api/users/me");
        
        Debug.Log($"✓ User loaded: {user.name}");
        Debug.Log($"User data: {JsonConvert.SerializeObject(user)}");
    }
    catch (ApiException ex)
    {
        Debug.LogError($"✗ Failed to load user");
        Debug.LogError($"Status: {ex.StatusCode}");
        Debug.LogError($"Error: {ex.Message}");
        Debug.LogError($"Response: {ex.RawResponseBody}");
    }
}
```

### ✅ After (Automatic Logging)

```csharp
var config = ApiClientConfig.Create("https://api.example.com");
config.LogLevel = LogLevel.Detailed;  // Configure once
config.LogResponseBody = true;

var client = new ApiClient(config);

async UniTask LoadUser()
{
    // No manual logging needed!
    var user = await client.GetAsync<User>("/api/users/me");
    
    // Console automatically shows:
    // [API] → GET https://api.example.com/api/users/me
    // [API] ← 200 in 0.48s
    // [API] Response Body: {"id":1,"name":"John","email":"john@example.com"}
}
```

## Log Levels

### `LogLevel.None`
**No logging at all.** Use for production builds to minimize log spam.

```csharp
config.LogLevel = LogLevel.None;
```

**Console output:** (nothing)

---

### `LogLevel.Errors`
**Only logs errors.** Shows when requests fail, with full error details.

```csharp
config.LogLevel = LogLevel.Errors;
```

**Console output:**
```
[API] ✗ Unauthorized: Invalid token
[API]   Status Code: 401
[API]   URL: GET https://api.example.com/users/me
[API]   Error Message: Token has expired
```

---

### `LogLevel.Basic` (Default)
**Request + response summary.** Shows URL, method, status code, and timing.

```csharp
config.LogLevel = LogLevel.Basic;
```

**Console output:**
```
[API] → GET https://api.example.com/users/me
[API] ← 200 in 0.48s
```

---

### `LogLevel.Detailed`
**Basic + request/response bodies** (when enabled). Great for debugging API issues.

```csharp
config.LogLevel = LogLevel.Detailed;
config.LogRequestBody = true;   // Show request body
config.LogResponseBody = true;  // Show response body
```

**Console output:**
```
[API] → POST https://api.example.com/auth/login
[API] Request Body:
{
  "email": "user@example.com",
  "password": "***"
}
[API] ← 200 in 0.52s
[API] Response Body:
{
  "token": "eyJhbGc...",
  "userId": "123"
}
```

---

### `LogLevel.Verbose`
**Everything.** Includes retry attempts, cancellations, and all debug info.

```csharp
config.LogLevel = LogLevel.Verbose;
```

**Console output:**
```
[API] → GET https://api.example.com/users/me
[API] Request failed (attempt 1), retrying in 1000ms: Network error
[API] → GET https://api.example.com/users/me
[API] ← 200 in 1.23s
```

## Configuration Options

### Basic Configuration

```csharp
var config = ApiClientConfig.Create("https://api.example.com");

config.LogLevel = LogLevel.Detailed;    // Control verbosity
config.LogRequestBody = true;           // Log request bodies
config.LogResponseBody = true;          // Log response bodies
config.MaxLogBodyLength = 500;          // Truncate long bodies

var client = new ApiClient(config);
```

### Inspector-Friendly (MonoBehaviour)

```csharp
public class ApiManager : MonoBehaviour
{
    [Header("Logging")]
    [SerializeField] private LogLevel _logLevel = LogLevel.Basic;
    [SerializeField] private bool _logRequestBody = false;
    [SerializeField] private bool _logResponseBody = false;
    
    void Start()
    {
        var config = ApiClientConfig.Create("https://api.example.com");
        config.LogLevel = _logLevel;
        config.LogRequestBody = _logRequestBody;
        config.LogResponseBody = _logResponseBody;
        
        var client = new ApiClient(config);
    }
}
```

Now you can toggle logging settings directly in the Unity Inspector!

## What Gets Logged Automatically

### ✅ Requests
- HTTP method (GET, POST, PUT, DELETE)
- Full URL
- Request body (if `LogRequestBody = true` and `LogLevel >= Detailed`)

### ✅ Responses
- HTTP status code
- Response time (duration)
- Response body (if `LogResponseBody = true` and `LogLevel >= Detailed`)

### ✅ Errors
- Error category (Timeout, NetworkError, Unauthorized, etc.)
- HTTP status code
- Error message
- Request URL and method
- Structured error details (if available)
- Raw response body (fallback)

### ✅ Retries
- Attempt number
- Delay before retry
- Reason for retry

### ✅ Cancellations
- Which request was canceled
- URL and method

## Examples

### Example 1: Development Mode (Detailed Logging)

```csharp
#if UNITY_EDITOR
    config.LogLevel = LogLevel.Detailed;
    config.LogResponseBody = true;
#else
    config.LogLevel = LogLevel.Basic;
#endif
```

### Example 2: Production Mode (Errors Only)

```csharp
#if DEBUG
    config.LogLevel = LogLevel.Detailed;
#else
    config.LogLevel = LogLevel.Errors;
#endif
```

### Example 3: Debugging Specific Issues

```csharp
// When debugging authentication issues
config.LogLevel = LogLevel.Detailed;
config.LogRequestBody = true;  // See what we're sending
config.LogResponseBody = true; // See what server responds

// When debugging performance
config.LogLevel = LogLevel.Basic;  // Shows timing for each request

// When testing retry logic
config.LogLevel = LogLevel.Verbose;  // Shows all retry attempts
```

## Performance Notes

- **`LogLevel.None`** - Zero overhead, no performance impact
- **`LogLevel.Errors`** - Minimal overhead, only logs on failures
- **`LogLevel.Basic`** - Very low overhead, just string formatting
- **`LogLevel.Detailed`** - Moderate overhead when logging bodies (JSON serialization)
- **`LogLevel.Verbose`** - Higher overhead, use only for debugging

Body truncation (`MaxLogBodyLength`) helps prevent huge logs from impacting performance.

## Custom Logging

If you need custom log formatting or want to send logs elsewhere (analytics, file, server):

```csharp
public class MyCustomLogger : ILogger
{
    public void LogRequest(string method, string url, object body = null)
    {
        // Send to your analytics service
        Analytics.LogEvent("api_request", new { method, url });
        
        // Also log to console
        Debug.Log($"API: {method} {url}");
    }
    
    public void LogResponse(int statusCode, float duration, string body = null)
    {
        // Track API performance
        Analytics.LogTiming("api_response_time", duration);
        
        Debug.Log($"API: {statusCode} in {duration:F2}s");
    }
    
    public void LogException(ApiException exception)
    {
        // Send errors to crash reporting
        CrashReporter.LogError(exception);
        
        Debug.LogError($"API Error: {exception}");
    }
    
    // Required but can be simple
    public void LogInfo(string message) => Debug.Log(message);
    public void LogWarning(string message) => Debug.LogWarning(message);
    public void LogError(string message) => Debug.LogError(message);
}

var client = new ApiClient(config, tokenProvider, new MyCustomLogger());
```

## Tips

1. **Start with `LogLevel.Detailed`** during development
2. **Switch to `LogLevel.Basic`** for testing builds
3. **Use `LogLevel.Errors`** in production (or `None` for minimal overhead)
4. **Enable body logging temporarily** when debugging specific API issues
5. **Set `MaxLogBodyLength`** to prevent huge logs (default 500 characters)
6. **Use conditional compilation** (`#if DEBUG`) to change log levels per platform

## Benefits

✅ **No manual logging code** - Save time and reduce boilerplate  
✅ **Consistent format** - All logs look the same  
✅ **Configurable verbosity** - One setting controls everything  
✅ **Inspector-friendly** - Toggle in Unity without code changes  
✅ **Performance-aware** - Disable in production easily  
✅ **Rich error details** - Automatic structured error logging  
✅ **Zero learning curve** - Works out of the box  

## Migrating from Manual Logging

If you have existing code with manual logging:

1. Remove all `Debug.Log` calls from your API calls
2. Configure `ApiClientConfig.LogLevel` once
3. Let the package handle logging automatically

**Before:**
```csharp
Debug.Log($"Calling {url}...");
var response = await client.GetAsync<T>(url);
Debug.Log($"Success: {response}");
```

**After:**
```csharp
var response = await client.GetAsync<T>(url);
// Logs automatically based on config.LogLevel
```

That's it! 🎉
