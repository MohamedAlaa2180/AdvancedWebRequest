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
https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequest#release/latest
```

4. Click **Add**

The `#release/latest` suffix pins the install to the [`release/latest`](https://github.com/MohamedAlaa2180/AdvancedWebRequest/tree/release/latest) branch so consumers always get the current stable line.

### Option B — `Packages/manifest.json`

Add an entry under `dependencies`:

```json
{
  "dependencies": {
    "com.mohamedalaa2180.advancedwebrequest": "https://github.com/MohamedAlaa2180/AdvancedWebRequest.git?path=Assets/AdvancedWebRequest#release/latest"
  }
}
```

Use the **same key** as the `name` field in [`Assets/AdvancedWebRequest/package.json`](Assets/AdvancedWebRequest/package.json) (`com.mohamedalaa2180.advancedwebrequest`).

After Unity resolves packages, you should see **Advanced Web Request**, **UniTask**, and **Newtonsoft Json** in the Package Manager list.

## Quick start

```csharp
using AdvancedWebRequest.Core;
using Cysharp.Threading.Tasks;

var config = ApiClientConfig.Create("https://api.example.com");
config.LogLevel = LogLevel.Basic;

var client = new ApiClient(config);

var user = await client.GetAsync<MyUserDto>("/users/me");
```

Full API, logging levels, JWT, retries, and examples live in the package docs:

- **[Package README](Assets/AdvancedWebRequest/README.md)** — features and usage  
- **[Quick start](Assets/AdvancedWebRequest/QUICKSTART.md)**  
- **[Logging guide](Assets/AdvancedWebRequest/LOGGING.md)**  
- **[Changelog](Assets/AdvancedWebRequest/CHANGELOG.md)**

## Repository layout

| Path | Purpose |
|------|---------|
| `Assets/AdvancedWebRequest/` | UPM package root (`package.json`, runtime code, examples, docs) |
| `Assets/Scenes/` | Sample scene (optional local testing) |
| `Packages/manifest.json` | This **template** project’s dependencies |

Cloning the repo is a full Unity project; **UPM consumers** only need the Git URL with `?path=Assets/AdvancedWebRequest` as above.

## Versioning & branches

- **`release/latest`** — default branch for UPM installs (stable line)  
- **`release/1.0.0`** — versioned release branch (example)  
- Git tags (e.g. **`v1.0.0`**) — optional; you can pin UPM with `#v1.0.0` instead of `#release/latest` if you prefer

## License

See [Assets/AdvancedWebRequest/LICENSE.md](Assets/AdvancedWebRequest/LICENSE.md).
