# System Architecture & Constitution

This document bridges `runbook-actions` (Logitech interface), `runbookd` (the daemon), `runbook-vscode` (terminal integration), and Claude Code (the agent).

If implementations drift from these guarantees, the system stops being an instrument and starts feeling like “just a macro pad.”

## 1. Truth Boundaries & Ownership

The system runs on independent truth boundaries. **Never infer what you can read.**

* **Daemon (`runbookd`)**: Owns state reducers, the `runbook.yaml` configurations, and `pending_prompt` intent. It is the only component that correlates outputs.
* **Claude Hooks (`runbook-hooks`)**: The only source of truth for agent lifecycle (`RUNNING`, `WAITING`, `IDLE`, `COMPLETE`).
* **VS Code (`runbook-vscode`)**: Provides terminal arrays (`TerminalsSnapshot`), current focus index, and executes terminal keystrokes (`send_text`, `send_sequence`).
* **Hardware (`runbook-actions`)**: Pure dumb rendering terminal and event dispatcher. It remembers absolutely nothing.

## 2. Default UX: The Queue-Arming Model

Because Claude officially recommends against pasting long prompts into the VS Code terminal (due to input truncation and terminal lag), the default arming style is a **Queue** tied to a Claude Plugin **Skill/Command**.

* **Keypad (Arm)**: Device sets `pending_prompt` internally on the daemon. UI renders `PENDING: <Task>`. No side-effect in VS Code natively.
* **Dialpad (Commit)**: `Enter` causes the daemon to send `should_execute=true` pointing to the short skill command (e.g. `/runbook:prep-pr`).
* **Dialpad (Cancel)**: `Esc` drops the pending prompt from the daemon without triggering editor escapes.
* **Dialpad (Interrupt)**: `Ctrl+C` is always concave and directly targets the active terminal.

*(Optionally configurable is `arm_style: prefill`, explicitly degraded UX allowing manual terminal pasting that voids “Pending” truth for “Pasted” feedback).*

## 3. Multi-Session Correlation

To support multiple VS Code terminals without lying:

1. The `runbook-vscode` extension starts Claude sessions passing the `RUNBOOK_SESSION_TAG=<uuid>` environment variable.
2. `TerminalsSnapshot` publishes terminal indices matched to this `session_tag` back to the daemon.
3. Hook scripts capture this environment variable and pass it to the daemon.
4. The daemon joins Hooks (`session_id`) to Terminal index (`session_tag`).

If multiple sessions exist and no `session_tag` bridges the events, the daemon **MUST** render state as `"unknown"` to prevent lying to the operator.

## 4. Policy Gate Dispatches

When blocking a destructive tool call via Bash intercept:
1. `runbook-hooks` must emit `stderr` reason and strictly `exit 2`. Async hooking cannot be used here because it fails to block the command immediately.
2. Claude pauses gracefully while the device renders a hard `BLOCKED` status.
3. The operator relies on “safe” keypad resets to bail them out, proving the hardware is superior to finding the right terminal and clearing buffers manually.

## 5. Fallback States (Hooks Mode)

The daemon uses `last_hook_ts` to determine `"active"` vs `"absent"` hook states.
* If no hooks arrive for a configured timeout across any session interaction, the daemon degrades to `"absent"`.
* When `"absent"`, the UI **cannot** display progress states (`RUNNING`, `IDLE`). The only permissible action-feedback state in disconnected conditions is `"sent"`.

## 6. Action Payload Routing

Dial semantics belong strictly to configuration enums:
* `DialMode.os_scroll`: Device utilizes native OS scrolling.
* `DialMode.vscode_terminal_scroll`: Device delegates dial payloads directly to `runbook-vscode`.

The VS Code plugin should actuate intents natively representing inputs. E.g. dispatching `esc`, `enter`, or `ctrl_c` must utilize VS Code's `workbench.action.terminal.sendSequence` over naive character feeding.
