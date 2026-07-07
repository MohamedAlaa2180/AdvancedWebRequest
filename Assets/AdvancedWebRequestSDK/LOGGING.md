# Automatic Logging Guide

## Overview

The Advanced Web Request package includes **automatic logging** that eliminates the need for manual logging in your code. Configure logging once in **Project Settings**, and all requests/responses are logged automatically.

## Quick comparison

### Before (manual logging)

```csharp
async UniTask LoadUser()
{
    try
    {
        Debug.Log($"Loading user from {url}...");

        var user = await client.GetAsync<User>("/api/users/me");

        Debug.Log($"✓ User loaded: {user.name}");
    }
    catch (ApiException ex)
    {
        Debug.LogError($"✗ Failed: {ex.Message}");
    }
}
```

### After (automatic logging)

Configure once in **Edit → Project Settings → Advanced Web Request**, then:

```csharp
var api = new ApiService("https://api.example.com");

async UniTask LoadUser()
{
    var user = await api.Client
        .Request("/api/users/me")
        .Get()
        .SendAsync<User>(api.Token);

    // Console automatically shows:
    // [API] → GET https://api.example.com/api/users/me
    // [API] ← 200 in 0.48s
    // [API] Response Body: {"id":1,"name":"John","email":"john@example.com"}
}
```

## Editor configuration (recommended)

Open **Edit → Project Settings → Advanced Web Request** (or **Advanced Web Request → Settings** from the menu):

| Setting | Description |
|---------|-------------|
| **Log Level** | `None`, `Errors`, `Basic`, `Detailed`, `Verbose` |
| **Log Request Body** | Include serialized request body in logs |
| **Log Response Body** | Include response body in logs |
| **Max Log Body Length** | Truncate long bodies (default 500) |
| **Default Timeout (s)** | Applied via `ApiClientConfig.Create()` |

Settings are stored in **EditorPrefs** (per developer machine), not committed to source control.

Every call to `ApiClientConfig.Create()` automatically applies these defaults through the Editor bridge.

## Log levels

### `LogLevel.None`

No logging. Use for production builds to minimize log spam.

**Console output:** (nothing)

---

### `LogLevel.Errors`

Only logs errors with full error details.

**Console output:**

```text
[API] ✗ Unauthorized: Invalid token
[API]   Status Code: 401
[API]   URL: GET https://api.example.com/users/me
```

---

### `LogLevel.Basic` (default)

Request + response summary: URL, method, status code, and timing.

**Console output:**

```text
[API] → GET https://api.example.com/users/me
[API] ← 200 in 0.48s
```

---

### `LogLevel.Detailed`

Basic + request/response bodies (when body logging is enabled). Great for debugging API issues.

**Console output:**

```text
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

Everything — retry attempts, cancellations, and debug info.

**Console output:**

```text
[API] → GET https://api.example.com/users/me
[API] Request failed (attempt 1), retrying in 1000ms: Network error
[API] → GET https://api.example.com/users/me
[API] ← 200 in 1.23s
```

## Code-level overrides (optional)

You can still override defaults per client:

```csharp
var config = ApiClientConfig.Create("https://api.example.com");
config.LogLevel = LogLevel.Detailed;
config.LogRequestBody = true;
config.LogResponseBody = true;
config.MaxLogBodyLength = 500;

var api = new ApiService(config);
```

For most projects, Project Settings alone is enough.

## What gets logged automatically

### Requests

- HTTP method (GET, POST, PUT, DELETE)
- Full URL
- Request body (if enabled and `LogLevel >= Detailed`)

### Responses

- HTTP status code
- Response time
- Response body (if enabled and `LogLevel >= Detailed`)

### Errors

- Error category (Timeout, NetworkError, Unauthorized, etc.)
- HTTP status code
- Error message
- Request URL and method
- Structured error details (if available)

### Retries

- Attempt number
- Delay before retry
- Reason for retry

## Examples

### Development vs production

Use Project Settings during development. For release builds, set log level to `Errors` or `None` before building, or override in code:

```csharp
#if !DEBUG
var config = ApiClientConfig.Create(baseUrl);
config.LogLevel = LogLevel.Errors;
#endif
```

### Debugging authentication

In Project Settings:

- Log Level: `Detailed`
- Log Request Body: on
- Log Response Body: on

## Performance notes

- **`LogLevel.None`** — Zero overhead
- **`LogLevel.Errors`** — Minimal, only on failures
- **`LogLevel.Basic`** — Very low, string formatting only
- **`LogLevel.Detailed`** — Moderate when logging bodies
- **`LogLevel.Verbose`** — Higher overhead, debugging only

Body truncation (`MaxLogBodyLength`) prevents huge logs from impacting performance.

## Custom logging

Implement `ILogger` to send logs to analytics, files, or crash reporting:

```csharp
public class MyCustomLogger : ILogger
{
    public void LogRequest(string method, string url, object body = null)
    {
        Analytics.LogEvent("api_request", new { method, url });
    }

    public void LogResponse(int statusCode, float duration, string body = null)
    {
        Analytics.LogTiming("api_response_time", duration);
    }

    public void LogException(ApiException exception)
    {
        CrashReporter.LogError(exception);
    }

    public void LogInfo(string message) => Debug.Log(message);
    public void LogWarning(string message) => Debug.LogWarning(message);
    public void LogError(string message) => Debug.LogError(message);
}

var api = new ApiService(config, tokenProvider, new MyCustomLogger());
```

## Migrating from 1.0.x

1. Remove per-class `[SerializeField]` log fields from MonoBehaviours
2. Configure logging in **Project Settings → Advanced Web Request**
3. Remove manual `Debug.Log` / `Debug.LogError` around API calls
4. Replace `GetAsync` / `PostAsync` with the fluent API

**Before:**

```csharp
[SerializeField] private LogLevel _logLevel = LogLevel.Detailed;
config.LogLevel = _logLevel;
var user = await client.GetAsync<User>("/api/users/me");
Debug.Log($"Success: {user.name}");
```

**After:**

```csharp
var user = await api.Client.Request("/api/users/me").Get().SendAsync<User>(api.Token);
// Logs automatically from Project Settings
```
