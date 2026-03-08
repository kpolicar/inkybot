# InkybotHook

EasyHook DLL injected into the Dofus (Unity) process to forge synthetic mouse clicks, keyboard input, and cursor position overrides — all invisible to the game's input pipeline.

## Why hook injection?

Dofus uses Unity's raw input pipeline (`WM_INPUT` / `RAWINPUT` packets, `WM_POINTER` messages) rather than just standard `WM_LBUTTONDOWN`. Simply calling `PostMessage` or `SendInput` doesn't produce the low-level raw input events Unity expects. This library hooks the Win32 message pump and raw input APIs *inside* the game process, injecting synthetic events that are indistinguishable from real hardware input.

## Architecture

```
Host Process (Inkybot)                    Target Process (Dofus)
┌──────────────────────┐                  ┌───────────────────────────────────┐
│  Win32Input          │   IPC (.NET      │  InjectionEntryPoint              │
│  - RequestClick()  ──┼── Remoting) ───> │  - InputProcessorLoop (thread)    │
│  - RequestKey()      │                  │  - Hooked PeekMessageW            │
│  - SetCursorPos()    │                  │  - Hooked GetRawInputData         │
│                      │                  │  - Hooked GetCursorPos            │
│  ServerInterface ◄───┼──────────────────┤  - ... (10 hooked functions)      │
│  (MarshalByRefObject)│                  │                                   │
└──────────────────────┘                  └───────────────────────────────────┘
```

### IPC Protocol

`ServerInterface` (a `MarshalByRefObject`) is the bridge between host and hook:
- **Cursor override**: Host sets `point` to client coords; `(-1,-1)` = no override
- **Click request**: Host calls `RequestClick(screenX, screenY)` — hook reads and clears `clickRequested`
- **Key request**: Host calls `RequestKey(char)` — hook reads and clears `keyRequested`
- **Lifecycle**: `ShutdownFlag`, `State` (enum), `Ping()` keep-alive

### Click Forge State Machine

A background thread (`InputProcessorLoop`) drives a three-state machine:

```
Idle ──(clickRequested)──> ButtonDown ──(50ms)──> ButtonUp ──(100ms)──> Idle
```

Each transition enqueues a burst of 6 synthetic messages into `_syntheticMessages`:

| Step | Message | Purpose |
|------|---------|---------|
| 1 | `WM_INPUT` (move) | Raw input absolute position |
| 2 | `WM_POINTERUPDATE` | Pointer API position update |
| 3 | `WM_MOUSEMOVE` | Classic mouse move |
| 4 | `WM_INPUT` (click) | Raw input button down/up |
| 5 | `WM_POINTERDOWN/UP` | Pointer API button event |
| 6 | `WM_LBUTTONDOWN/UP` | Classic button event |

Keyboard input is simpler: `WM_KEYDOWN` → `WM_CHAR` → `WM_KEYUP`, tagged with a magic `lParam` so `TranslateMessage` doesn't generate a duplicate `WM_CHAR`.

### How Messages Reach the Game

The hooked `PeekMessageW` is the central dispatcher:
1. Call the real `PeekMessageW` to get the next OS message
2. Track which window receives input (`_mainHwnd`, pointer ID)
3. Drop real hardware input when `IsInputOverriden` (cursor override active)
4. When the OS queue is empty or only has `WM_NULL`, dequeue from `_syntheticMessages` and return it as if the OS produced it

The game's message loop sees: `PeekMessage` → `TranslateMessage` → `DispatchMessage`, and processes synthetic messages identically to real ones.

### Raw Input Packet Injection

When the game calls `GetRawInputData` with our magic handle (`0x1337` for clicks, `0x1338` for moves), we build a fake `RAWINPUT` packet in native memory:
- **Move packets**: Absolute coordinates normalized to 0–65535 screen space
- **Click packets**: `RI_MOUSE_LEFT_BUTTON_DOWN` / `UP` flags

`GetRawInputBuffer` is also hooked to inject button flags into any real hardware packets that arrive mid-click (race condition fix), or inject a fake packet when the buffer is empty.

### Polling State Hooks

- `GetCursorPos` → returns the fixed screen point when override is active
- `GetAsyncKeyState` / `GetKeyState` → reports left mouse button as pressed during `ButtonDown`
- `ReleaseCapture` → blocked during `ButtonDown` so the game doesn't lose mouse capture
- `GetCapture` → reports `_mainHwnd` during `ButtonDown`

## Unity's Input Event Loop (Deduced)

Through hooking and observing which Win32 APIs Unity calls (and in what order), we've reverse-engineered the following input processing model used by the Dofus Unity client:

```
┌─────────────────────────────────────────────────────────┐
│  Unity Main Thread — Per-Frame Input Poll               │
│                                                         │
│  1. PeekMessageW (loop until empty)                     │
│     ├─ WM_INPUT        → queue for step 2               │
│     ├─ WM_POINTER*     → processed by UI system         │
│     ├─ WM_LBUTTON*     → fallback legacy path           │
│     ├─ WM_MOUSEMOVE    → cursor tracking                │
│     └─ WM_KEY*/WM_CHAR → keyboard input                 │
│                                                         │
│  2. GetRawInputData / GetRawInputBuffer                 │
│     └─ Reads RAWINPUT structs from queued WM_INPUT      │
│        handles to get raw mouse deltas + button flags   │
│                                                         │
│  3. GetCursorPos                                        │
│     └─ Polled each frame for absolute cursor position   │
│                                                         │
│  4. GetAsyncKeyState / GetKeyState                      │
│     └─ Polled for current button/key state              │
│        (used to confirm mouse-down is still held, etc.) │
│                                                         │
│  5. SetCapture / ReleaseCapture / GetCapture            │
│     └─ Managed around mouse-down/up to ensure the       │
│        window keeps receiving mouse events even if the   │
│        cursor leaves the client area                    │
└─────────────────────────────────────────────────────────┘
```

**Key observations:**

- Unity does **not** rely on `GetMessage` (blocking). It uses `PeekMessageW` in a non-blocking loop, draining the entire queue each frame before processing.
- Raw input (`WM_INPUT` → `GetRawInputData`) is the **primary** input path for mouse. The `WM_LBUTTONDOWN/UP` and `WM_MOUSEMOVE` messages appear to serve as a secondary/fallback path, but both must be present for reliable input.
- `WM_POINTER*` messages are used for the 2D UI layer (menus, inventory, buttons). These carry screen-space coordinates and a pointer ID that must match across `UPDATE` → `DOWN` → `UP` sequences.
- Unity polls `GetAsyncKeyState(VK_LBUTTON)` and `GetKeyState(VK_LBUTTON)` each frame to confirm button state — if these disagree with the message-based state, clicks are ignored. This is why we must hook these polling APIs too.
- `SetCapture` is called on mouse-down and `ReleaseCapture` on mouse-up. If capture is released prematurely (e.g., by OS or game logic during our synthetic click), the click is lost. Hence we block `ReleaseCapture` during `ButtonDown`.
- The `RAWINPUT` packet size must be exactly `headerSize + 24` bytes (the standard `RAWMOUSE` payload). Unity allocates a stack buffer of this size and rejects packets that don't match — this was discovered through trial and error after larger "bloated" hardware packets were silently dropped.
- Device handles in `RAWINPUT` packets must correspond to a real registered device. We capture a genuine mouse device handle during `ProbeRawInputDevices` at startup and reuse it in all fake packets.

## File Structure

| File | Responsibility |
|------|---------------|
| `InjectionEntryPoint.cs` | EasyHook entry point, hook installation, lifecycle |
| `InjectionEntryPoint.Delegates.cs` | Hook delegate types and original function pointers |
| `InjectionEntryPoint.InputForge.cs` | Click/key state machine, synthetic message enqueuing, capture hooks |
| `InjectionEntryPoint.MessageHooks.cs` | PeekMessageW, TranslateMessage, DispatchMessageW hooks |
| `InjectionEntryPoint.PollingHooks.cs` | GetCursorPos, GetAsyncKeyState, GetKeyState hooks |
| `InjectionEntryPoint.RawInputHooks.cs` | GetRawInputData, GetRawInputBuffer, fake packet construction, device probing |
| `InjectionEntryPoint.Coordinates.cs` | `IsInputOverriden` property, client-screen coordinate conversion |
| `InjectionEntryPoint.DebugVisuals.cs` | GDI crosshair overlay for debugging cursor position |
| `NativeMethods.cs` | All Win32 structs, constants, and P/Invoke declarations |
| `ServerInterface.cs` | IPC interface (`MarshalByRefObject`) between host and hook |

## Magic Handles

| Value | Name | Purpose |
|-------|------|---------|
| `0x1337` | `MAGIC_RAW_HANDLE` | Fake `WM_INPUT` lParam for click packets |
| `0x1338` | `MAGIC_RAW_MOVE_HANDLE` | Fake `WM_INPUT` lParam for move packets |
| `0x1339` | `MAGIC_KEY_HANDLE` | Fake `WM_KEYDOWN/UP` lParam to bypass `TranslateMessage` |

## Thread Safety

Four locks protect shared state:
- `_queueLock` — synthetic message queue (written by InputProcessorLoop, read by PeekMessageW)
- `_deviceLock` — captured device handle/packet size (written by ProbeRawInputDevices + GetRawInputData passthrough, read by BuildFakeRawInputPacket)
- `_nativeBufLock` — native memory buffer for fake RAWINPUT packets
- `_allHooksInstalled` — `ManualResetEventSlim` gate ensuring no hook runs before all hooks are installed

## Build

.NET Framework 4.8 class library (x64). Depends on EasyHook 2.7.7097. Output goes to `Inkybot/bin/{Debug,Release}/`.
