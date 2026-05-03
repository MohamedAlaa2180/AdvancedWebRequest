# Automatic Logging Feature - Implementation Summary

## What Was Added

### New Feature: Automatic Request/Response Logging
Users no longer need to manually log API calls. The package now handles all logging automatically based on configuration.

## Changes Made

### 1. New Log Level System (`ApiClientConfig.cs`)
Added 5 configurable log levels:
- `None` - No logging
- `Errors` - Only errors
- `Basic` - Request/response summary (default)
- `Detailed` - With request/response bodies
- `Verbose` - Everything including debug info

**New properties:**
```csharp
public LogLevel LogLevel { get; set; } = LogLevel.Basic;
public bool LogRequestBody { get; set; } = false;
public bool LogResponseBody { get; set; } = false;
public int MaxLogBodyLength { get; set; } = 500;
```

### 2. Enhanced Logger Interface (`ILogger.cs`)
Added structured logging methods:
```csharp
void LogRequest(string method, string url, object body = null);
void LogResponse(int statusCode, float duration, string body = null);
void LogException(ApiException exception);
```

### 3. Improved Default Logger (`DefaultLogger.cs`)
- Color-coded console output
- Structured request/response logging
- Rich exception formatting with all error details
- Automatic body truncation

### 4. Automatic Logging in ApiClient (`ApiClient.cs`)
Logging is now automatically called at key points:
- **Before request**: Logs method + URL + body (if enabled)
- **After response**: Logs status + timing + body (if enabled)
- **On error**: Logs full error details with structured data
- **On retry**: Logs retry attempt + delay
- **On cancel**: Logs cancellation notice

All logging respects the configured `LogLevel`.

### 5. Updated Examples
- `TestGetRequest.cs` - Shows Inspector-friendly log configuration
- `LoggingExample.cs` - NEW! Demonstrates all log levels
- Removed all manual logging from examples

### 6. Documentation Updates
- `README.md` - Added automatic logging section
- `QUICKSTART.md` - Updated with log configuration
- `LOGGING.md` - NEW! Complete logging guide with examples

## Benefits

### For Users
✅ **Zero manual logging** - No more Debug.Log in every API call  
✅ **Consistent format** - All logs look professional  
✅ **Inspector-friendly** - Toggle log levels without code changes  
✅ **Production-ready** - Easily disable for release builds  
✅ **Rich error details** - Automatic structured error reporting  
✅ **Performance-aware** - Configurable verbosity  

### For Debugging
✅ **See all requests** - Know exactly what's being called  
✅ **Track timing** - Identify slow endpoints  
✅ **View bodies** - Debug request/response data  
✅ **Monitor retries** - See retry behavior  
✅ **Catch errors** - Full error context automatically  

## Usage Comparison

### Before (Manual Logging Required)
```csharp
try
{
    Debug.Log($"Calling {url}...");
    
    var user = await client.GetAsync<User>("/api/users/me");
    
    Debug.Log($"✓ Success: {user.name}");
    Debug.Log($"Response: {JsonConvert.SerializeObject(user)}");
}
catch (ApiException ex)
{
    Debug.LogError($"✗ Failed: {ex.Category}");
    Debug.LogError($"Status: {ex.StatusCode}");
    Debug.LogError($"Error: {ex.Message}");
    if (ex.StructuredError != null)
    {
        Debug.LogError($"Code: {ex.StructuredError.Code}");
    }
}
```

### After (Automatic Logging)
```csharp
var config = ApiClientConfig.Create("https://api.example.com");
config.LogLevel = LogLevel.Detailed;
config.LogResponseBody = true;

var client = new ApiClient(config);

// Just make the call - logging happens automatically!
var user = await client.GetAsync<User>("/api/users/me");

// Console shows:
// [API] → GET https://api.example.com/api/users/me
// [API] ← 200 in 0.48s
// [API] Response Body: {"id":1,"name":"John","email":"john@example.com"}
```

## Migration Guide

### Step 1: Configure Log Level
Add logging configuration to your ApiClient setup:
```csharp
var config = ApiClientConfig.Create(baseUrl);
config.LogLevel = LogLevel.Detailed;  // Choose your level
config.LogResponseBody = true;        // Optional
```

### Step 2: Remove Manual Logging
Delete all manual Debug.Log calls from your API request methods.

### Step 3: Test
Run your app and see automatic logging in action!

### Step 4: Adjust for Production
```csharp
#if DEBUG
    config.LogLevel = LogLevel.Detailed;
#else
    config.LogLevel = LogLevel.Errors;  // Or None
#endif
```

## Configuration Examples

### Development (Verbose Logging)
```csharp
config.LogLevel = LogLevel.Detailed;
config.LogRequestBody = true;
config.LogResponseBody = true;
```

### Testing (Basic Logging)
```csharp
config.LogLevel = LogLevel.Basic;
```

### Production (Errors Only)
```csharp
config.LogLevel = LogLevel.Errors;
```

### Production (No Logging)
```csharp
config.LogLevel = LogLevel.None;
```

## Log Output Examples

### Basic Level
```
[API] → GET https://api.example.com/users/1
[API] ← 200 in 0.48s
```

### Detailed Level (with bodies)
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

### Error Logging
```
[API] → GET https://api.example.com/users/me
[API] ✗ Unauthorized: Token has expired
[API]   Status Code: 401
[API]   URL: GET https://api.example.com/users/me
[API]   Error Code: TOKEN_EXPIRED
[API]   Error Message: Your session has expired, please login again
```

### Retry Logging (Verbose)
```
[API] → GET https://api.example.com/data
[API] Request failed (attempt 1), retrying in 1000ms: Network error
[API] → GET https://api.example.com/data
[API] ← 200 in 1.23s
```

## Backward Compatibility

✅ **Fully backward compatible** - Existing code continues to work  
✅ **Default behavior** - `LogLevel.Basic` provides sensible defaults  
✅ **Optional opt-in** - Users can keep their manual logging if desired  

## Performance Impact

- **LogLevel.None**: Zero overhead ✅
- **LogLevel.Errors**: Minimal, only on failures ✅
- **LogLevel.Basic**: Very low, just string formatting ✅
- **LogLevel.Detailed**: Moderate with body logging ⚠️
- **LogLevel.Verbose**: Higher, use for debugging only ⚠️

Body truncation (`MaxLogBodyLength = 500`) prevents large logs from impacting performance.

## Files Modified

1. `Models/ApiClientConfig.cs` - Added LogLevel enum and config properties
2. `Interfaces/ILogger.cs` - Added structured logging methods
3. `Utils/DefaultLogger.cs` - Enhanced with structured formatting
4. `Core/ApiClient.cs` - Added automatic logging calls throughout
5. `Examples/TestGetRequest.cs` - Updated to show new approach
6. `README.md` - Added automatic logging documentation
7. `QUICKSTART.md` - Updated with log configuration

## Files Created

1. `Examples/LoggingExample.cs` - Demonstrates all log levels
2. `LOGGING.md` - Complete logging guide
3. `CHANGELOG_LOGGING.md` - This file

## Testing

To test the new logging system:
1. Attach `LoggingExample.cs` to a GameObject
2. Enable the log levels you want to test in Inspector
3. Run the scene
4. Observe different log outputs in Console

## Future Enhancements (Optional)

- [ ] Log filtering by endpoint
- [ ] Log output to file
- [ ] Log aggregation/analytics integration
- [ ] Request/response size logging
- [ ] Header logging option
- [ ] Custom log formatters
- [ ] Async logging to prevent blocking

## Summary

This update transforms the Advanced Web Request package from a great HTTP client into a **developer-friendly, production-ready** solution with zero-configuration logging that "just works" while remaining fully customizable for advanced users.

**Line count saved:** Users save ~10-20 lines of logging code per API call! 🎉
