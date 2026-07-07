# Automatic Logging Feature - Implementation Summary

## What was added

Automatic request/response logging so users no longer need manual `Debug.Log` calls around API requests.

## Current configuration (1.1.0+)

Logging defaults are configured in **Edit → Project Settings → Advanced Web Request**:

- Log Level (`None`, `Errors`, `Basic`, `Detailed`, `Verbose`)
- Log Request Body
- Log Response Body
- Max Log Body Length
- Default Timeout

Settings are stored in **EditorPrefs** and applied automatically when `ApiClientConfig.Create()` is called via the `ApiClientConfig.OnCreate` delegate bridge in the Editor assembly.

Per-client overrides in code are still supported.

## Changes made

### 1. Log level system (`Runtime/Models/ApiClientConfig.cs`)

```csharp
public LogLevel LogLevel { get; set; } = LogLevel.Basic;
public bool LogRequestBody { get; set; } = false;
public bool LogResponseBody { get; set; } = false;
public int MaxLogBodyLength { get; set; } = 500;
```

### 2. Enhanced logger interface (`Runtime/Interfaces/ILogger.cs`)

```csharp
void LogRequest(string method, string url, object body = null);
void LogResponse(int statusCode, float duration, string body = null);
void LogException(ApiException exception);
```

### 3. Default logger (`Runtime/Utils/DefaultLogger.cs`)

Color-coded console output, structured request/response logging, body truncation.

### 4. Automatic logging in ApiClient (`Runtime/Core/ApiClient.cs`)

Logging at request start, response, error, retry, and cancellation — respecting configured `LogLevel`.

### 5. Editor settings (`Editor/AdvancedWebRequestSettings.cs`)

Project Settings page replaces per-MonoBehaviour `[SerializeField]` logging fields.

### 6. Updated samples (`Samples~/BasicExamples/`)

- `LoggingExample.cs` — demonstrates log levels
- All samples rely on Project Settings instead of Inspector log fields

## Usage comparison

### Before (manual logging)

```csharp
try
{
    Debug.Log($"Calling {url}...");
    var user = await client.GetAsync<User>("/api/users/me");
    Debug.Log($"✓ Success: {user.name}");
}
catch (ApiException ex)
{
    Debug.LogError($"✗ Failed: {ex.Message}");
}
```

### After (automatic logging)

Configure **Project Settings → Advanced Web Request**, then:

```csharp
var api = new ApiService("https://api.example.com");

var user = await api.Client
    .Request("/api/users/me")
    .Get()
    .SendAsync<User>(api.Token);

// Console shows request, response, timing, and bodies (if enabled)
```

## Migration guide

1. Update UPM path to `Assets/AdvancedWebRequestSDK` if upgrading from 1.0.x
2. Open **Project Settings → Advanced Web Request** and set log preferences
3. Remove per-class `[SerializeField]` log fields from MonoBehaviours
4. Remove manual `Debug.Log` calls around API requests
5. Use the fluent API: `client.Request(path).Get().SendAsync<T>(ct)`

## Files (current layout)

| Path | Role |
|------|------|
| `Runtime/Models/ApiClientConfig.cs` | Log config + `OnCreate` delegate |
| `Runtime/Interfaces/ILogger.cs` | Logger interface |
| `Runtime/Utils/DefaultLogger.cs` | Default console logger |
| `Runtime/Core/ApiClient.cs` | Automatic logging calls |
| `Editor/AdvancedWebRequestSettings.cs` | Project Settings UI |
| `Samples~/BasicExamples/LoggingExample.cs` | Sample |
| `LOGGING.md` | User guide |

## Performance impact

- `LogLevel.None` — zero overhead
- `LogLevel.Errors` — minimal
- `LogLevel.Basic` — very low
- `LogLevel.Detailed` / `Verbose` — use for debugging only

Body truncation (`MaxLogBodyLength`) limits log size impact.
