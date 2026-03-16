# Building a Dofus Maging Bot with OCR, Win32 API, and a Rule-Based AI

Maging is one of the most tedious and repetitive activities in Dofus. You sit at the workshop, apply runes one at a time, check if they landed, check your sink, decide the next rune, and repeat — for hours. Automating it is appealing, but it comes with a catch: users often mage on their main accounts. Getting the magus profession leveled up is costly, and high-value items require high profession levels. Getting banned means losing that entire investment.

This creates a unique constraint for bot developers. Socket bots and packet sniffers are powerful tools for many Dofus activities — farming, fighting, navigation — but for maging, the risk profile is different. You need an approach that prioritizes safety above all else.

OCR is the safest approach to game automation and doesn't break across game updates — the screen looks the same regardless of patches. Usually, OCR alone isn't enough for a game bot. It's too slow for combat, too imprecise for navigation, and too limited for complex multi-step interactions. This is why most developers don't go down that route. But maging is a rare case where OCR is not just sufficient — it's ideal. You apply one rune every few seconds. The UI is static and predictable. Every piece of information the bot needs — item stats, mage history, sink value, rune inventory — is right there on screen.

This article walks through how we built [Inkybot](https://inkybot.me), a maging bot that uses only OCR and Win32 API. We'll cover the tick-based architecture, how we read the game screen with Tesseract, how we verify that combines actually landed, the rule-based AI that decides which rune to apply, and a future optimization that could push OCR-based bots to socket-bot speeds.

![The Inkybot client with the Dofus game window embedded via Win32 SetParent](https://inkybot.me/images/features/client.png)
*The bot client with the Dofus game window embedded via Win32 SetParent.*

---

## The Tick Loop

The bot operates in discrete **ticks**. Each tick is one complete rune combination cycle: read the screen, decide what to do, do it, confirm it worked, and loop.

```
┌──────────┐    ┌────────────┐    ┌─────────────────┐
│ OCR Scan │───>│ AI Resolve │───>│ Execute Combine  │
│  (Item)  │    │  (Action)  │    │ (Mouse Click)    │
└──────────┘    └────────────┘    └────────┬─────────┘
                                           │
                                 ┌─────────▼─────────┐
                                 │ Poll History OCR   │
                                 │ (Until Changed)    │
                                 └─────────┬──────────┘
                                           │
                                 ┌─────────▼─────────┐
                                 │ Read Sink Value    │──> NEXT TICK
                                 └───────────────────┘
```

Here's what happens on every tick:

1. **OCR Scan** — Capture the Dofus window and read the current item stats from specific screen regions using Tesseract.
2. **AI Resolve** — The rule-based AI examines the current stats against the user's configuration and decides which rune to apply next (or whether the item is finished).
3. **Execute Combine** — The bot simulates mouse input to select the rune and click the combine button.
4. **Poll History** — The bot continuously OCR-reads the maging history region until it detects a change, confirming the combine went through.
5. **Read Sink** — Once the combine is confirmed, read the updated sink value from screen.
6. **Loop** — Return to step 1.

The tick-based design means the bot is always reactive to what's actually on screen. It never assumes a combine succeeded — it verifies. If the game lags, the bot waits. If a rune fails, the bot sees it. This makes the system robust against network hiccups and unexpected game behavior.

---

## Screen Reading with Tesseract and Win32 API

We use the Win32 API to interact with the game client. The Dofus window is embedded as a child of the bot's own form using `SetParent`, which gives us precise control over the window's position and consistent screen coordinates regardless of where the user moves the bot.

Rather than OCR-ing the entire screen (which would be slow and error-prone), we target specific rectangular regions:

- **Stats area** — The vertical list of current stat values, mins, and maxes
- **Mage history** — The scrolling list of recent combine results
- **Sink value** — The current sink percentage
- **Rune quantities** — Individual cells showing how many of each rune the player has

Each region is cropped from a single screenshot of the Dofus window, then fed to Tesseract independently. This region-based approach is dramatically faster than full-screen OCR and gives us much higher accuracy since each region contains a small, predictable amount of text.

```
function scanGameState():
    screenshot = captureWindow(dofusHandle)

    statLines    = tesseractOCR(crop(screenshot, STATS_BOUNDS))
    historyLines = tesseractOCR(crop(screenshot, HISTORY_BOUNDS))
    sinkValue    = tesseractOCR(crop(screenshot, SINK_BOUNDS))

    item = parseStats(statLines)
    return { item, historyLines, sinkValue }
```

There were some challenges with getting the image preprocessing right to feed Tesseract reliable input — small text on game UIs isn't exactly what Tesseract was designed for. But once calibrated, accuracy is high and consistent.

---

## Speculative OCR Overlapping

OCR is inherently slow. If you read stats, then history, then sink sequentially, each tick takes too long. This is where we borrowed an idea from CPU architecture: **speculative execution**.

While polling the history region to check if a combine has landed, we don't just sit idle. We simultaneously kick off OCR reads for the sink value and item stats in parallel. These reads happen concurrently with the history poll — we're speculatively reading data we'll need *if* the combine succeeded.

```
function waitForCombineAndReadState(previousHistory):
    while true:
        // Kick off all OCR reads in parallel
        historyTask = ocrRegionAsync(HISTORY_BOUNDS)
        sinkTask    = ocrRegionAsync(SINK_BOUNDS)
        statsTask   = ocrRegionAsync(STATS_BOUNDS)

        currentHistory = await historyTask

        if hasChanged(currentHistory, previousHistory):
            // Sink and stats are already being read!
            return {
                history: currentHistory,
                sink: await sinkTask,
                stats: await statsTask
            }
```

By the time we confirm the history has changed, the sink and stats are already read (or nearly done). This overlapping eliminates the sequential bottleneck and significantly reduces per-tick latency. Instead of `OCR(history) + OCR(sink) + OCR(stats)` taking 3x the time, we get all three for roughly the cost of one.

---

## History Monitoring — Knowing When a Rune Landed

After clicking the combine button, the bot needs to know when the action actually went through. It can't just assume — network lag, game animations, and server processing all introduce delay. This is where the history monitoring comes in, and it doubles as a safeguard against the bot getting out of sync with the game.

Before each combine, the bot stores the current mage history text. After clicking combine, it enters a tight polling loop:

1. Capture the history region from a fresh screenshot
2. Run Tesseract OCR on it
3. Compare the result line-by-line against the stored baseline
4. If any line differs, the combine has landed — move on
5. If nothing changed, recapture and retry

This continuous monitoring means the bot **never blindly assumes a combine succeeded**. It always verifies. If the game takes a moment to process, the bot simply waits. If something goes wrong and the history never updates, the bot detects the timeout and handles it.

**Out-of-runes detection** was an interesting problem. Our first approach was to scan the rune quantity numbers directly from the maging table UI. But reading small numbers via OCR turned out to be unreliable — a "3" easily becomes an "8", a "1" disappears entirely. Instead, we switched to tracking consecutive failed history changes. If the bot clicks combine multiple times and the history never updates, it concludes the runes are depleted and stops. This turned out to be more reliable than trying to read tiny digits.

---

## The Rule-Based AI

The bot's decision-making doesn't use machine learning. It uses a straightforward rule-based approach: examine the current item state, look at the user's configuration, and deterministically decide which rune to apply.

The core logic is what we call **TargetResolve**. It works like this:

1. Look at all stats that are currently below their configured target value
2. Sort them by distance from their max — **furthest first**
3. For each candidate, check if we have an appropriate rune and if the rune type is allowed by the user's config
4. Return the first valid match

Why prioritize the stat furthest from its max? It's a Dofus mechanic: runes are more likely to succeed when the stat is further from its cap. By targeting these stats first, we maximize the probability of each combine landing, which means fewer wasted runes and faster completion.

```
function resolveNextAction(item, config, sink):
    rune = TargetResolve(item, config, sink)
    if rune != null:
        return COMBINE(rune)
    return FINISH

function TargetResolve(item, config, sink):
    candidates = stats where value < target

    // Sort by distance from max — furthest first
    // (further from cap = higher success rate in Dofus)
    sorted = sort(candidates, key = stat.max - stat.value, descending)

    for stat in sorted:
        // Rune type selected based on user config (with reasonable defaults)
        rune = resolveRuneType(stat, config)
        if rune != null:
            return rune

    return null
```

The AI also respects the game's **oversink constraint** — part of Dofus's domain logic that limits how far an item can go beyond its base stats. The bot checks this before every combine to avoid putting the item in an unrecoverable state.

---

## Configuration — Fine-Grained Control Per Stat

A maging bot is only as good as its configuration. Each stat on the item gets its own config object, giving the user precise control over how the AI handles it:

- **Target** — The desired value for this stat. Can exceed the item's natural maximum for intentional overmages.
- **TargetMinimum** — The lowest acceptable value. The bot won't consider the item finished until every stat meets its minimum.
- **Priority** — When multiple stats need work, this determines the order. Higher priority stats are resolved first.
- **Rune type toggles** — Enable or disable SM (small), PA (medium), and RA (large) runes individually. Some users prefer to avoid large runes on certain stats to reduce variance.
- **Rune switch thresholds** — At what stat value should the bot switch from small to medium runes, or medium to large? Larger runes carry higher risk when the stat gap is small, so these thresholds let users dial in their risk tolerance.
- **HighSinkStat** — A flag for stats like Vitality that have high sink values. The bot handles these with different priority ordering.

The rune type resolution follows the user's config, falling through from large to small:

```
function resolveRuneType(stat, config):
    if config.useRaRunes AND stat.value >= config.raThreshold:
        return RA
    if config.usePaRunes AND stat.value >= config.paThreshold:
        if stat.value > config.maxPaCanHit: return null
        return PA
    if config.useSmRunes:
        if stat.value > config.maxSmCanHit: return null
        return SM
    return null
```

Reasonable defaults are provided out of the box, so users don't need to configure every threshold. But for experienced magers who know exactly how they want their items built, the granularity is there.

---

## Why Not Sockets?

Socket bots are powerful. For farming, fighting, and navigation in Dofus, they offer speed and precision that OCR can't match. So why go the OCR route for maging?

It comes down to risk management. Maging is different from other botted activities because of who's doing it. Users often mage on their main accounts — the ones with costly, high-level professions. The character that can mage a high-value item is usually not a throwaway alt. Losing that account to a ban means losing everything.

OCR is the safest option for game automation. It's also the least likely to get detected and doesn't break across game updates — the screen looks the same regardless of patches. There are no modified game files, no injected DLLs, no custom clients.

Most developers don't choose OCR because it's usually too limited for a full game bot. And they're right — you can't run a dungeon or navigate a map with OCR alone. But maging is a special case. The activity is slow by nature (one rune every few seconds). The UI is static and predictable. Every piece of data the bot needs is displayed on screen. For maging specifically, OCR isn't a compromise — it's the right tool for the job.

When the stakes are highest, choosing the safest approach isn't playing it safe. It's playing it smart.

---

## Future Work — Speculative Plan Execution

We've already optimized Inkybot's performance to approximately **2 runes per second**. But we're not done. The next frontier is pushing OCR-based bots to match — or even exceed — socket bot speeds.

The idea is **plan-based speculative execution**. Instead of the current approach where the AI resolves one action, executes it, verifies it, and then moves to the next, the AI would pre-compute a sequence of N combine actions, assuming each one will succeed. It then executes them in rapid succession without waiting for individual verification.

```
function executeSpeculativePlan(item, config):
    plan = ai.computePlan(item, config, depth=5)

    for action in plan:
        executeImmediate(action)  // click combine, don't wait

    // After all actions, read the actual state
    actualItem = ocrScan()

    succeeded = countMatchingActions(plan, actualItem)
    if succeeded < len(plan):
        // Some failed — re-read state, continue from here
        continueFromActualState(actualItem)
```

After the entire plan executes, the bot reads the screen once to check how many actions actually succeeded, then adjusts and continues from the real state. Think of it like TCP's sliding window — send multiple packets without waiting for individual ACKs, then reconcile when the responses come back.

The beauty of this approach is that it turns the OCR bottleneck from a per-action cost into a per-plan cost. If a plan has 5 actions and takes one OCR read to verify, the effective speed is 5x faster than the current tick-by-tick approach.

---

## Takeaways

Building Inkybot taught us that the "best" technical approach depends entirely on the problem domain. The key architectural decisions — a tick-based loop, region-targeted OCR, speculative overlapping of OCR reads, and a rule-based AI with a chain-of-responsibility pattern — all stem from understanding what maging actually *is*: a slow, visual, repetitive process where safety matters more than raw speed.

OCR requires calibration of screen regions and image preprocessing work. It has trade-offs. But for maging, these are manageable problems — and the safety benefits are real.

Maging is one of those rare automation targets where the simplest, safest approach is also the most practical one. Sometimes the boring solution is the right one.
