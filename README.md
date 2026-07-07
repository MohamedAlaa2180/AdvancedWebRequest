# Advanced Web Request

Production-ready HTTP client for **Unity** built on `UnityWebRequest`, with **UniTask** async/await, optional **JWT** auth, retries, timeouts, Newtonsoft JSON, and **configurable automatic logging**.

**Repository:** [github.com/MohamedAlaa2180/AdvancedWebRequest](https://github.com/MohamedAlaa2180/AdvancedWebRequest)

## Requirements

- Unity **2021.3** or newer
- This package depends on **UniTask** and **Newtonsoft.Json** (declared in `package.json`; UPM installs them when you add the package from Git)

## Install via Unity Package Manager (UPM)

### Option A — Add from Git URL (recommended)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL…**
3. Paste:

```text
https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequestSDK#release/latest
```

4. Click **Add**

The `#release/latest` suffix pins the install to the [`release/latest`](https://github.com/MohamedAlaa2180/AdvancedWebRequest/tree/release/latest) branch so consumers always get the current stable line.

To pin a specific version, use a tag instead:

```text
https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequestSDK#v1.1.0
```

### Option B — `Packages/manifest.json`

Add an entry under `dependencies`:

```json
{
  "dependencies": {
    "com.mohamedalaa2180.advancedwebrequest": "https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequestSDK#release/latest"
  }
}
```

Use the **same key** as the `name` field in [`Assets/AdvancedWebRequestSDK/package.json`](Assets/AdvancedWebRequestSDK/package.json) (`com.mohamedalaa2180.advancedwebrequest`).

After Unity resolves packages, you should see **Advanced Web Request**, **UniTask**, and **Newtonsoft Json** in the Package Manager list.

### Import samples (optional)

In Package Manager, select **Advanced Web Request** → **Samples** → import **Basic Examples**.

## Quick start

```csharp
using AdvancedWebRequest.Core;
using Cysharp.Threading.Tasks;

var api = new ApiService("https://api.example.com");

var user = await api.Client
    .Request("/users/me")
    .Get()
    .SendAsync<MyUserDto>(api.Token);
```

Configure logging and default timeout once in the Editor: **Edit → Project Settings → Advanced Web Request**.

Full API, logging levels, JWT, retries, and examples live in the package docs:

- **[Package README](Assets/AdvancedWebRequestSDK/README.md)** — features and usage
- **[Quick start](Assets/AdvancedWebRequestSDK/QUICKSTART.md)**
- **[Logging guide](Assets/AdvancedWebRequestSDK/LOGGING.md)**
- **[Changelog](Assets/AdvancedWebRequestSDK/CHANGELOG.md)**

## Repository layout

| Path | Purpose |
|------|---------|
| `Assets/AdvancedWebRequestSDK/` | UPM package root (`package.json`, docs) |
| `Assets/AdvancedWebRequestSDK/Runtime/` | Runtime SDK code |
| `Assets/AdvancedWebRequestSDK/Editor/` | Editor settings (Project Settings page) |
| `Assets/AdvancedWebRequestSDK/Samples~/BasicExamples/` | Optional importable samples |
| `Assets/Scenes/` | Sample scene (optional local testing) |
| `Packages/manifest.json` | This **template** project's dependencies |

Cloning the repo is a full Unity project; **UPM consumers** only need the Git URL with `?path=Assets/AdvancedWebRequestSDK` as above.

## Versioning & branches

- **`release/latest`** — default branch for UPM installs (stable line)
- **`release/1.1.0`** — versioned release branch
- Git tags (e.g. **`v1.1.0`**) — pin UPM with `#v1.1.0` instead of `#release/latest`

## Migrating from 1.0.x

If you installed **1.0.x**, update your Git URL path from `Assets/AdvancedWebRequest` to `Assets/AdvancedWebRequestSDK`, then replace shortcut methods (`GetAsync`, `PostAsync`, etc.) with the fluent API:

```csharp
await client.Request("/path").Get().SendAsync<T>(ct);
await client.Request("/path").Post().WithBody(body).SendAsync<T>(ct);
```

See the [1.1.0 changelog](Assets/AdvancedWebRequestSDK/CHANGELOG.md) for full details.

## License

See [Assets/AdvancedWebRequestSDK/LICENSE.md](Assets/AdvancedWebRequestSDK/LICENSE.md).
