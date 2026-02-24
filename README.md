# runbook-actions-plugin

C# plugin for **Logi Options+** / **Logi Actions SDK**.

This plugin is the hardware bridge:

- Receives button / dial / roller events from the MX Creative Console
- Forwards them to `runbookd` (localhost)
- Renders the daemon's UI model back onto the 3×3 LCD keypad

## Build / package

The Logi Actions SDK uses a generator and packager workflow.

High level:

1. Install the Logi Actions SDK + tooling.
2. Build this project (produces a DLL).
3. Package into an `.lplug4` for installation / Marketplace.

See Logitech docs for the exact workflow.

## Wiring (recommended)

Create a profile in Logi Options+ with two devices:

### Keypad (9 LCD keys)
Assign **Runbook Slot** to each key with parameters:

- 0..8

### Dialpad buttons
Assign **Runbook Dialpad Button** with parameters:

- ctrl_c
- export
- esc
- enter

### Dial + roller
Assign **Runbook Adjustment** with parameters:

- dial
- roller

## Config

This plugin connects to `ws://127.0.0.1:29381/ws` by default.
Override with environment variable:

- `RUNBOOKD_WS=ws://127.0.0.1:29381/ws`

## Notes

This repo is intentionally thin. The daemon owns all state.
