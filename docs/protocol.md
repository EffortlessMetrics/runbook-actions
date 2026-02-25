# Protocol Spec (v0.1)

This document defines the boundary schema connecting the Logitech plugin, the Python/Rust daemon (`runbookd`), and the VS Code extension (`runbook-vscode`).

**Core Invariant:** All implementations MUST strictly parse and emit `snake_case` JSON fields.

## 1. Envelopes and Base Types

All messages exchanged between the daemon and its clients must include a `type` field. Any unknown fields MUST be safely ignored by parsers.

### `HooksMode`
Represents the freshness of Claude Code hooks.
* `"active"`: Hooks exist and `now() - last_hook_ts < STALENESS_TIMEOUT` (e.g., 5 seconds).
* `"absent"`: No hooks or stale hooks.

### `AgentState`
The true lifecycle state of the Claude session. Must never be inferred absent hooks.
* `"idle"` (from `idle_prompt`)
* `"running"` (from `UserPromptSubmit` or `ToolUse`)
* `"waiting"` (from `permission_prompt`)
* `"complete"` (from `TaskCompleted` / `SessionEnd`)
* `"blocked"` (from exit-2 `PreToolUse` bash block)
* `"sent"` (degraded mode only: prompt dispatched, receipt unconfirmed)
* `"offline"` (daemon disconnected)
* `"unknown"` (hooks active but multiple sessions lack correlation tags)

### `DialMode`
Dictates dial semantic dispatch.
* `"os_scroll"`: Handled natively by OS (no plugin dispatch).
* `"vscode_terminal_scroll"`: Managed by extension tracking terminal limits.

---

## 2. Inbound Messages to Daemon (From Devices/VS Code)

### `hello`
Sent initially by the client (VS Code or Logi plugin) to begin the session.
```json
{
  "type": "hello",
  "client": "logi|vscode",
  "protocol": 1,
  "version": "0.1.0",
  "client_id": "8-char-id"
}
```

### `terminals_snapshot`
Sent by VS Code extension on terminal lifecycle changes to keep cycle-index true.
```json
{
  "type": "terminals_snapshot",
  "terminals": [
    { "index": 0, "name": "bash", "session_tag": "uuid-1234" }
  ],
  "active_index": 0
}
```

### `hook_event`
Sent by the Claude `runbook-hooks` script.
```json
{
  "type": "hook_event",
  "session_id": "claude-run-xyz",
  "session_tag": "uuid-1234",
  "event_type": "PreToolUse|UserPromptSubmit|Notification|SessionStart|SessionEnd",
  "payload": { ... }
}
```

### Actuator Events (From Logi Plugin)
```json
// Keypad Slot
{ "type": "keypad_press", "slot": 0 }

// Page
{ "type": "page", "direction": "next" }

// Dialpad Button
{ "type": "dialpad_button_press", "button": "ctrl_c" }

// Adjustments
{ "type": "adjustment", "kind": "roller", "delta": -1 }
```

---

## 3. Outbound Messages from Daemon

### `hello_ack`
Daemon response to `hello` acknowledging protocol compatibility.
```json
{
  "type": "hello_ack",
  "protocol": 1,
  "daemon_version": "0.1.0"
}
```

### `render`
Sent to the Logi Plugin when true state changes.
```json
{
  "type": "render",
  "agent_state": "idle",
  "hooks_mode": "active",
  "pending_prompt": {
    "id": "prep_pr",
    "label": "Prep PR",
    "style": "queue" // or "prefill"
  },
  "keypad": {
    "slots": [
      { "slot": 0, "label": "Prep PR", "armed": true }
    ],
    "page_count": 3
  }
}
```

### VS Code Actuators
Sent to VS Code extension to execute editor-level behaviors.
```json
// Text
{ "type": "send_text", "text": "/runbook:prep-pr", "should_execute": true }

// Sequence (for Ctrl+C, Esc, Enter)
{ "type": "send_sequence", "sequence": "enter" }

// Terminals
{ "type": "focus_terminal", "index": 1 }
{ "type": "cycle_terminal", "direction": "next" }

// Jumpgates
{ "type": "open_uri", "uri": "https://..." }
{ "type": "reveal_receipt", "file": "..." }
```

---

## 4. Fixtures Requirement

Implementations (Rust, Typescript, C#) **MUST** execute round-trip tests against JSON fixtures representing the envelopes documented above to guarantee strict schema adherence across language boundaries.
