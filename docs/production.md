# Production Handoff — Runbook Actions Plugin

## Role

**C# Actions plugin = hardware I/O + rendering + settings + resilience.**

It must not become a second daemon. It must not infer Claude state.
It can only render what the daemon asserts.

## Definition of Done

- `.lplug4` packs and verifies (`logiplugintool pack` + `verify`)
- Daemon down → OFFLINE tiles + `PluginStatus.Warning`
- Daemon up → keys redraw on render updates (no stale LCDs)
- Default profiles appear for VS Code ("Adapt to App") and bind Keypad (70) + Dialpad (71) correctly
- Protocol mismatch → `PluginStatus.Error` with version detail, reconnect stops
- No business logic beyond: debounce/coalesce for hardware deltas, connection health, rendering

## Must Ship (v0)

| Area | What | SDK Reference |
|------|------|---------------|
| Packaging | `metadata/` + icon + `tools/pack.ps1` | [Plugin Structure](https://logitech.github.io/actions-sdk-docs/csharp/Plugin-structure/) |
| App Wiring | `RunbookApplication` with `GetProcessNames()` | [Link to App](https://logitech.github.io/actions-sdk-docs/csharp/Link-the-plugin-to-an-application/) |
| Profiles | Export `DefaultProfile70.lp5` / `71.lp5` from UI | [Default Profiles](https://logitech.github.io/actions-sdk-docs/csharp/Default-Application-Profiles/) |
| Networking | WS reconnect, hello/ack, protocol error, coalescing | — |
| Settings | `daemon_url`, `client_id` via `TryGetPluginSetting` | [Plugin Settings](https://logitech.github.io/actions-sdk-docs/csharp/Managing-Plugin-Settings/) |
| Status | Normal/Warning/Error via `OnPluginStatusChanged` | [Plugin Status](https://logitech.github.io/actions-sdk-docs/csharp/Plugin-Status/) |
| Rendering | `GetCommandImage` + `ActionImageChanged()` | [Button Images](https://logitech.github.io/actions-sdk-docs/csharp/Change-a-button-image/) |
| Actions | 9 keypad slots, 4 dialpad buttons, dial, roller, page | — |

## Nice to Have (v1+)

| Feature | Notes |
|---------|-------|
| Dynamic Folders | Plugin-controlled workspace. Adds UX decisions (entry/exit). |
| Selective redraw | Only invalidate changed slots instead of all. Performance polish. |
| `UsesApplicationApiOnly` | Re-add if actions should be globally accessible from Action Panel. |

## Commit/PR Sequence

1. **Packaging** — metadata fixes, icon, pack scripts
2. **App Wiring** — `RunbookApplication.cs`, README "Adapt to App" note
3. **Transport** — DaemonClient: multi-frame, handshake, reconnect, settings, status
4. **Renderer** — KeyRenderer + cache + OFFLINE tile + correct redraw triggers
5. **Actions** — PageCommand, adjustment coalescing, slot numbering
6. **Fixtures** — protocol fixtures + snake_case round-trip tests
7. **Hardware** — export and commit DefaultProfile70/71

## Invariants

- The plugin **never** invents state; it renders what the daemon sends
- `snake_case` on the wire, enforced by protocol fixture tests
- Actions are scoped to VS Code profiles (not globally exposed)
- Protocol mismatch is terminal (no reconnect spam)
- Roller/dial events are coalesced at 16ms to protect the daemon
