# Changelog

All notable changes to this project will be documented in this file.

## [1.1.0] - 2026-05-14

### Added
- **Editor Settings**: New Project Settings page for centralized logging and default request configuration
  - Configurable log level, request/response body logging, max log body length, and default timeout
  - Settings stored in EditorPrefs for per-developer preferences
  - Automatic application to all ApiClient instances via delegate bridge pattern
- **ApiService class**: New helper class to reduce boilerplate in MonoBehaviour examples
  - Encapsulates ApiClient and CancellationTokenSource lifecycle management
  - Provides convenient Token property and Cancel/Dispose methods
  - Eliminates repetitive setup code in example classes

### Changed
- **API Consolidation**: Simplified public API surface to enforce fluent builder pattern
  - Made `SendJsonAsync` internal (previously public)
  - Removed public `GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync` methods
  - All requests now use fluent API: `client.Request(path).Get().SendAsync<T>()`
- **Package Structure**: Reorganized into standard Unity Package Manager layout
  - `Runtime/` folder for core SDK code with Runtime assembly definition
  - `Editor/` folder for Editor-only code with Editor assembly definition
  - `Samples~/` folder for example code with Samples assembly definition
- **Example Classes**: Refactored all examples to follow new patterns
  - Use ApiService instead of direct ApiClient + CancellationTokenSource
  - Removed manual Debug.Log calls (logging now driven by SDK settings)
  - Simplified exception handling (throw instead of log-and-consume)
  - Removed serialized logging configuration fields

### Fixed
- **Error Handling**: Improved exception categorization in request pipeline
  - UnityWebRequestException now properly mapped to specific ApiException categories
  - Fixed issue where network errors were incorrectly classified as "Unknown"
  - OperationCanceledException and ApiException now properly propagated

### Technical
- Added delegate bridge pattern (ApiClientConfig.OnCreate) to apply Editor settings without cross-assembly dependencies
- Improved assembly definition structure with proper platform restrictions
- Enhanced error routing through HandleResponseAsync for better categorization

## [1.0.0] - 2026-05-03

### Added - Core Features
- Core `ApiClient` with UniTask async/await support
- JWT authentication with `ITokenProvider` interface and `SimpleTokenProvider` implementation
- Automatic retry with exponential backoff and jitter via `RetryPolicy`
- Timeout control with cancellation support using `CancellationToken`
- Structured error handling with `ApiException` and 11 error categories
- JSON serialization/deserialization with Newtonsoft.Json (thread-safe parsing)
- **Automatic logging system** with 5 configurable log levels (None, Errors, Basic, Detailed, Verbose)
- Fluent request builder API via `RequestBuilder` for chainable syntax
- Configurable `ILogger` interface with color-coded `DefaultLogger`
- `ApiClientConfig` for centralized configuration
- Request/response body logging with truncation support
- Comprehensive examples and documentation

### Added - API Methods
- `GetAsync<T>()` - GET requests
- `PostAsync<T>()` - POST requests with body
- `PutAsync<T>()` - PUT requests with body
- `DeleteAsync<T>()` - DELETE requests
- `SendJsonAsync<T>()` - Generic method with full control
- Fluent API: `client.Request(path).Get().WithTimeout().SendAsync<T>()`

### Features
- ✅ Mobile and PC optimized with memory-efficient design
- ✅ Network error handling with proper categorization
- ✅ HTTP status code categorization (401, 403, 404, 429, 5xx, etc.)
- ✅ Full CancellationToken support throughout
- ✅ Parallel request support via UniTask.WhenAll
- ✅ Token refresh pattern example
- ✅ Custom retry policies (Default, Aggressive, NoRetry)
- ✅ Per-request timeout configuration
- ✅ Custom headers support (global + per-request)
- ✅ Production-ready error model with structured + raw fallback
- ✅ **Automatic request/response/error logging** (no manual logging needed)
- ✅ Inspector-friendly configuration
- ✅ Zero allocation async operations (via UniTask)

### Examples
- `ExampleUsage.cs` - Basic usage with JSONPlaceholder
- `AdvancedExamples.cs` - All features demonstration
- `FluentApiExample.cs` - Fluent API patterns
- `TokenRefreshExample.cs` - JWT token refresh implementation
- `TestGetRequest.cs` - Real API test with configurable logging
- `LoggingExample.cs` - Demonstrates all 5 log levels

### Documentation
- `README.md` - Full feature documentation with code examples
- `QUICKSTART.md` - 5-minute setup guide
- `GETTING_STARTED.md` - Step-by-step tutorial
- `FEATURES.md` - Complete feature list with comparisons
- `LOGGING.md` - **Complete automatic logging guide**
- `IMPLEMENTATION_SUMMARY.md` - Architecture and design details
- `CHANGELOG_LOGGING.md` - Logging feature implementation notes
- `LICENSE.md` - MIT License
- Inline code examples throughout
- API usage patterns and best practices

### Dependencies
- Unity 2021.3+
- UniTask 2.3.3+ (com.cysharp.unitask)
- Newtonsoft.Json 3.2.1+ (com.unity.nuget.newtonsoft-json)

### Technical Details
- Assembly definition files for proper Unity integration
- Namespace: `AdvancedWebRequest.Core`
- Thread-safe JSON parsing via UniTask thread pool
- Proper UnityWebRequest disposal and cleanup
- Color-coded console logs for better readability
- Configurable log body truncation (default 500 chars)
