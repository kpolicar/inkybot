# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Inkybot is a Windows bot for automating item "maging" (stat enchanting) in the MMORPG Dofus. It uses screen capture + OCR (Tesseract) to read the game UI, then executes mouse/keyboard actions to apply runes.

## Build & Test

This is a .NET 4.8 solution built with MSBuild (via Visual Studio or JetBrains Rider). Dont attempt to build the solutions.

Tests use NUnit 3.5 (in the `Tests/` project).

**Critical:** This is a legacy-style `.csproj` (not SDK-style). **Every new `.cs` file must be manually added to the `<Compile>` section of the appropriate `.csproj`.** Files not listed there are silently ignored by the build.

## Project Structure

| Project | Purpose |
|---------|---------|
| `Inkybot/` | Main WinForms application (x64 WinExe, .NET 4.8) |
| `Inkybot.Dofus/` | Domain library: game data structures (Item, Stat, Rune, MageConfig, etc.) |
| `Inkybot.Design/` | DI infrastructure: `ServiceContainer`, `HasDependencies` interface |
| `InkybotHook/` | EasyHook DLL injected into the Dofus process to hook `GetCursorPos` and `IsIconic` |
| `Tests/` | NUnit tests (MagingAI logic, OCR parsing) |

## Architecture

### Dependency Injection

Services are registered in `Inkybot/Program.cs` in the `_services` dictionary (type → instance). After registration, all services implementing `HasDependencies` receive their dependencies via `BindDependencies(ServiceContainer)`. To get a service: `serviceContainer.GetService<T>()`.

Services use abstract contract types as keys (e.g., `DofusMagingJobContract`, `DofusDataProvider`), with concrete implementations registered against those types.

### Maging Loop

`ScreenReaderDofusMagingJob` runs on a background thread. Each iteration is a `Tick` driven by a state machine:

```
STANDARD → EXECUTING_COMBINE → CALCULATING_SINK_CHANGE → CALCULATING_PRICE_CHANGE → STANDARD
```

- **STANDARD**: AI resolves next action (rune to apply or Finish), executes it
- **EXECUTING_COMBINE**: Polls OCR until history changes (rune landed)
- **CALCULATING_SINK_CHANGE**: Reads new sink value from screen
- **CALCULATING_PRICE_CHANGE**: Resets to STANDARD

### Screen Reading

`ScreenReaderDataProvider` orchestrates OCR via `DofusScreenScan`. It reads item stats, mage history, sink value, and rune quantities from game screenshots. Multiple screen capture backends exist: `Win32ScreenCapture`, `WinGraphicsCaptureScreenCapture`, `WinScreenRecorderScreenCapture`.

### AI Decision Making

`DofusMagingAI` (in `Inkybot/Services/`) implements the built-in maging AI. It uses a chain of `ItemMageResolve` strategies (Target → Perfection → ReachMinimum → FinishSink → Exo) to decide which rune to apply. Custom AI can be implemented by extending `Inkybot.Dofus.Contracts.DofusMagingAI`.

### Localization

The app supports English and French. Each form and resource file has a `.fr.resx` counterpart. Locale is auto-detected from Dofus game settings or set in user preferences. Stat/Rune/Maging dictionaries live in `Inkybot/Resources/` as `.resx` + `.fr.resx` pairs.

### Release Builds

Release configuration applies obfuscation via `Confuser.MSBuild` (the `Inkybot.crproj` file configures it). `MouseKeyHook` is only included in Debug builds.
