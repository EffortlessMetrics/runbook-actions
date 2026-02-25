# runbook-actions

Logi Actions SDK plugin for the **MX Creative Console**, bridging hardware to the [Runbook](https://github.com/runbook-rs) daemon.

## What this repo is

A **thin client**. It does hardware I/O, rendering, connection health, and nothing else.

- Receives button / dial / roller events from the MX Creative Console
- Forwards them to `runbookd` over WebSocket (localhost)
- Renders the daemon's UI model onto the 3×3 LCD keypad
- Reports connection health via Plugin Status

> **This plugin does not infer agent state.** It renders exactly what the daemon asserts.
> Actions are scoped to VS Code application profiles; they are not exposed globally.

## Build

```powershell
dotnet build src/Runbook.LogiPlugin.sln
```

## Test

```powershell
dotnet test src/Runbook.LogiPlugin.sln
```

## Pack & Install

Requires the [Logi Actions SDK](https://logitech.github.io/actions-sdk-docs/) tooling.

```powershell
.\tools\pack.ps1        # dotnet publish → logiplugintool pack → verify
```

Or on Linux/macOS:

```bash
./tools/pack.sh
```

The output is a `.lplug4` file. Install it via Logi Options+ or submit to the Logitech Marketplace.

## Export Default Profiles

Default profiles make "install → open VS Code → it works" happen.

1. Open **Logi Options+** → create a VS Code profile
2. Bind the 9 LCD keys to **Runbook Slot** (parameters `0`–`8`)
3. Bind the 4 dialpad buttons to **Runbook Dialpad Button** (`ctrl_c`, `export`, `esc`, `enter`)
4. Bind the roller to **Runbook Adjustment** (`roller`)
5. Bind the dial to **native scroll** (Logi built-in)
6. **Export** each profile and rename to:
   - `profiles/DefaultProfile70.lp5` (Keypad)
   - `profiles/DefaultProfile71.lp5` (Dialpad)
7. Commit the `.lp5` files — do **not** manually edit the zip

> **"Adapt to App"** must be enabled in Options+ for the VS Code profile to activate automatically.

## Configuration

| Source | Setting | Default |
|--------|---------|---------|
| Plugin Settings | `daemon_url` | `ws://127.0.0.1:29381/ws` |
| Plugin Settings | `client_id` | auto-generated on first run |
| Environment variable | `RUNBOOKD_WS` | overrides `daemon_url` |

## Action Surface

| Action | Type | Parameters |
|--------|------|------------|
| Runbook Slot | Keypad LCD | `0`–`8` |
| Runbook Dialpad Button | Button | `ctrl_c`, `export`, `esc`, `enter` |
| Runbook Page | Button | `prev`, `next` |
| Runbook Adjustment | Dial/Roller | `dial`, `roller` |

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Keys show **OFFLINE** | Daemon not running | Start `runbookd` |
| Plugin status: **Warning** | Reconnecting to daemon | Check daemon logs |
| Plugin status: **Error** | Protocol mismatch | Update plugin or daemon to matching versions |
| No profile in VS Code | "Adapt to App" disabled | Enable in Options+ → Devices → MX Creative Console |

## Architecture

```text
MX Creative Console ←→ Logi Actions SDK ←→ RunbookPlugin ←→ runbookd ←→ VS Code
                                              (this repo)
```

The daemon owns all state. The plugin is I/O + rendering.

## License

MIT — see [LICENSE](LICENSE).
