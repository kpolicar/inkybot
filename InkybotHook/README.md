# InkybotHook — Dofus Process Injection

## Architecture Overview

This DLL is injected into the Dofus (Unity) process via EasyHook. Two entry points exist:

### Active: Simplified Entry Point (`InjectionEntryPoint.cs`)

A passive hook that supports the host-side PostMessage approach. Win32Input sends
WM_LBUTTONDOWN/UP or WM_POINTERDOWN/UP messages from outside; the hook provides
supporting services inside the process.

**Hooked functions:**

| Hook | Purpose |
|------|---------|
| `GetCursorPos` | Returns fixed screen position from `ServerInterface.point` when override is active |
| `IsIconic` | Always returns `false` so Dofus keeps rendering when backgrounded |
| `DispatchMessageW` | Captures real pointer ID from WM_POINTER messages; overrides WM_MOUSEMOVE/WM_POINTERUPDATE coordinates to fixed position; logs synthetic click messages |
| `GetKeyState` | Spoofs VK_LBUTTON as pressed when `IsClickActive` is true |
| `GetAsyncKeyState` | Spoofs VK_LBUTTON as pressed when `IsClickActive` is true |

**Detection:** On startup, calls `IsMouseInPointerEnabled()` from inside the Dofus process
and exposes the result via `ServerInterface.IsPointerInputEnabled`. The host reads this
to decide whether to send WM_POINTER or WM_LBUTTON messages.

**Flow:**
1. Win32Input.Click(x,y) sets cursor position via IPC, then PostMessages WM_POINTERDOWN/UP or WM_LBUTTONDOWN/UP
2. Messages enter Dofus's message queue
3. DispatchMessageW hook logs them and overrides move coordinates
4. GetCursorPos returns the fixed position when game code queries cursor
5. GetKeyState/GetAsyncKeyState report button pressed during simulated clicks

### Reserved: Advanced Entry Point (`AdvancedInjectionEntryPoint*.cs`)

Excluded from build (commented out in .csproj). A more complex approach that:
- Hooks 7 functions (PeekMessageW, TranslateMessage, DispatchMessageW, GetRawInputData, GetRawInputBuffer, GetAsyncKeyState, GetKeyState)
- Runs its own automation thread that generates synthetic click events
- Injects messages through three parallel channels:
  1. **Stealth Hardware (WM_INPUT + MAGIC_RAW_HANDLE 0x1337)** — fabricates RAWINPUT packets
  2. **Modern UI (WM_POINTER)** — sends pointer events with screen coordinates
  3. **Legacy UI (WM_LBUTTON)** — sends button events with client coordinates
- Subclasses all windows dynamically via SetWindowLongPtr

To re-enable: uncomment the Advanced files in InkybotHook.csproj, add `: EasyHook.IEntryPoint`
to the class declaration, and remove or rename the simplified InjectionEntryPoint.

## Shared Components

- **ServerInterface.cs** — IPC bridge (MarshalByRefObject) between host and injected code
- **NativeMethods.cs** — P/Invoke declarations and native structs (POINT, MSG, RAWINPUT, etc.)
