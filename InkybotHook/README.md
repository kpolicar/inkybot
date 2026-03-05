Case 1: You Physically Move the Mouse (The Shredder Flow)
When you bump your real mouse, Windows generates hardware interrupts. Our goal is to completely blind the game to this so it doesn't fight your bot.

Hardware -> OS: You move the mouse. Windows puts a WM_MOUSEMOVE and a WM_INPUT message into the game's thread queue.

Game -> PeekMessageW / GetMessageW: The game's engine wakes up and asks the OS for the next message.

Your Hook -> FilterMouseMessage: Your hook intercepts the message before the game sees it.

It recognizes WM_MOUSEMOVE or WM_POINTERUPDATE.

It explicitly changes msg.message = WM_NULL (an empty, harmless message).

Game Processes Message: The game receives WM_NULL, ignores it, and updates nothing on the UI.

Game -> GetRawInputData (or Buffer): If the OS sent a WM_INPUT message with a real physical handle, the game takes that handle and asks the OS for the raw bytes.

Your Hook -> GetRawInputData: Your hook calls the original OS function to grab the real bytes, then overwrites usButtonFlags, lLastX, and lLastY to 0.

Result: The game reads the buffer, sees 0 movement and 0 clicks, and the 3D camera stays perfectly still.

Case 2: Injected UI Interaction (The WM_POINTER Flow)
This flow interacts with the game's 2D elements (menus, inventory, dialogue buttons). It requires screen coordinates and high-level window messages.

Your Bot -> AutomationThreadLoop: The timer hits (or your IPC server sends a command). _msgState becomes Hover.

The Alarm Clock -> PostMessage: The thread posts WM_NULL to the game's background window. This forcefully wakes up the game's sleeping rendering thread.

Game -> PeekMessageW: The game wakes up and asks for the next message.

Your Hook -> State Machine: Your hook sees _msgState == Hover.

It throws away whatever message the OS just provided.

It overwrites the struct with msg.message = 0x0245 (WM_POINTERUPDATE).

It overwrites msg.pt with your locked screen coordinates.

It advances _msgState to ButtonDown.

Game Processes Message: The game's UI engine thinks a touch/pen device just hovered over the coordinates.

Repeat: On the next frames, steps 2-5 repeat automatically, injecting 0x0246 (WM_POINTERDOWN) and 0x0247 (WM_POINTERUP), completing a perfect background UI click.

Case 3: Injected 3D World Interaction (The Magic Handle Flow)
This flow interacts with the game's 3D environment (moving the character, rotating the camera, clicking a 3D model). It completely bypasses the OS and feeds memory structs directly to the game engine.

Your Bot -> AutomationThreadLoop: _rawState becomes ButtonDown.

The Magic Alarm -> PostMessage: The thread posts WM_INPUT directly to the background window, attaching the handle 0x1337 (MAGIC_RAW_HANDLE).

Game -> PeekMessageW: The game wakes up, reads the WM_INPUT message, and extracts the 0x1337 handle.

Game -> GetRawInputData(0x1337): The game asks the OS: "Give me the memory packet for handle 0x1337."

Your Hook -> Magic Intercept: Your hook sees hRawInput == 0x1337. It does not call the OS (because the OS would return an error for a fake handle).

It allocates unmanaged memory.

It builds a perfect RAWINPUTHEADER and RAWMOUSE struct with the RI_MOUSE_LEFT_BUTTON_DOWN flag.

It copies those structs into the game's pData pointer.

It returns the exact byte size of the fake packet, telling the game it succeeded.

Game Processes Raw Input: The game's 3D engine reads the memory, sees the Left Mouse Down flag, and registers a 3D click natively.

Repeat: The state machine advances to ButtonUp and repeats the process on the next frame to release the click.