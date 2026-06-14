# Katya Katya Performance Optimization Specification

## Purpose

This specification defines the refactor needed to reduce frame drops in `KatyaKatya.Client` while preparing the client for a premium dating simulator and visual novel experience. The core strategy is to keep Avalonia responsible for UI composition and move animation-heavy rendering into a Skia-based scene renderer.

## Current Performance Risks

The current client has several likely sources of lag:

- Multiple `DispatcherTimer` instances update visual effects independently.
- `AnimatedBackground` creates many Avalonia controls for mist, clouds, bokeh, gradients, transforms, and opacity changes.
- `ParticleCanvas` renders Skia into a `WriteableBitmap`, which introduces per-frame bitmap locking and pixel-copy overhead.
- Card and character assets are loaded through view-facing models instead of a centralized asset store.
- The memory game board uses many nested controls per card, multiplying layout and render work.
- Some hot templates use reflection bindings where compiled or direct bindings would be cheaper.
- Effects continue to exist at the shell level even when the active view may not need them.
- Recent liquid-glass styling, panel lift/glow, translucent buttons, and global animated backgrounds increase visual polish but also increase composition and animation cost.
- Global hover/click sound effects now run through `SoundService`; short SFX should be measured because rapid hover events can create small spikes if media resources are recreated too often.

## Performance Goals

| Target | Goal |
| --- | --- |
| 1280x720 | Stable 60 FPS |
| 1920x1080 | Stable 60 FPS for normal dating scenes |
| 4K | 30 to 60 FPS depending on graphics preset |
| 60 FPS frame budget | 16.6 ms total |
| Skia draw budget | Under 6 ms |
| Simulation update budget | Under 2 ms |
| Avalonia layout budget | Under 4 ms |
| Per-frame allocations | Near zero in active render loops |

## Target Architecture

The client should be split into four major areas:

- **Avalonia UI Layer**: shell, navigation, HUD, menus, settings, modals, dialogue choices.
- **Skia Rendering Layer**: backgrounds, character sprites, particles, shaders, transitions, post effects.
- **Game And Narrative Layer**: dating progression, affinity, mood, dialogue state machine, rewards.
- **Minigame Layer**: memory game rules, board state, scoring, result emission.

Avalonia ViewModels should not load image streams, own particle lists, or perform render-time calculations. They should expose user intent and screen state. Rendering systems should consume immutable scene snapshots.

## Implementation Plan

### Phase 1: Instrumentation

Add a developer-only performance overlay:

- FPS.
- Smoothed frame time.
- Active particles.
- Active render layers.
- Cached image count.
- Estimated cached image memory.
- Current graphics preset.

Add timing probes around:

- background updates,
- particle updates,
- Skia draw operations,
- memory board view render/update,
- asset load and decode,
- navigation transitions,
- liquid-glass panel hover/lift animations,
- global hover/click sound playback.

Acceptance criteria:

- Developers can see real-time FPS and frame time in debug builds.
- Slow frames over 24 ms are logged with the active scene name.
- The overlay can show whether expensive optional effects are active: animated background, particles, glass hover animation, and UI SFX.

### Phase 2: Unified Game Loop

Create:

```text
Engine/Core/IGameLoop.cs
Engine/Core/GameLoop.cs
Engine/Core/IFrameUpdatable.cs
Engine/Core/FrameTime.cs
```

Responsibilities:

- Own a single frame ticker.
- Calculate delta time and smoothed frame time.
- Register and unregister active simulation systems.
- Pause when no animated systems are active.
- Pause when the visual tree is detached.
- Reduce FPS for static screens or low-power mode.
- Coordinate global animated backgrounds so static/auth/settings screens do not pay for scene effects they do not need.

Acceptance criteria:

- `AnimatedBackground` and `ParticleCanvas` no longer own independent long-lived timers.
- Effects stop updating after navigation away from their view.
- Global animated backgrounds can be paused, reduced, or swapped for static layers based on the active view and graphics preset.

### Phase 3: Skia Custom Draw Operation

Replace the `WriteableBitmap` path in `ParticleCanvas` with an Avalonia custom draw operation.

Create:

```text
Controls/SkiaSceneControl.cs
Engine/Skia/SkiaSceneDrawOperation.cs
Engine/Skia/SkiaRenderContext.cs
```

Rules:

- `Render(DrawingContext)` captures a scene snapshot and submits a draw operation.
- The draw operation resolves only cached native assets.
- No asset decoding, stream opening, or bitmap resizing is allowed inside `Render`.
- Per-frame allocations should be avoided in draw operations.

Acceptance criteria:

- Particle rendering no longer locks a `WriteableBitmap` every frame.
- Render work is isolated from ViewModels.

### Phase 4: Skia Background Renderer

Convert `AnimatedBackground` from many Avalonia controls into Skia layers:

```text
Engine/Effects/RomanticBackgroundRenderer.cs
Engine/Effects/MistLayer.cs
Engine/Effects/CloudLayer.cs
Engine/Effects/BokehLayer.cs
Engine/Effects/VignetteLayer.cs
```

Rendering approach:

- mist: radial gradients or cached translucent blobs,
- clouds: cached sprite/path clusters,
- bokeh: circles or low-resolution cached sprites,
- vignette: radial shader,
- parallax: uniform transform based on pointer position.

Acceptance criteria:

- Main window visual tree is substantially smaller.
- Background animation remains visually similar but with lower layout cost.

### Phase 5: Visual Asset Store

Create:

```text
Engine/Assets/IVisualAssetStore.cs
Engine/Assets/VisualAssetStore.cs
Engine/Assets/VisualAssetId.cs
Engine/Assets/AssetManifest.cs
```

Responsibilities:

- Preload scene assets.
- Decode images at appropriate sizes.
- Cache `SKImage` and Avalonia `Bitmap` separately when needed.
- Release unused large assets after scene transitions.
- Provide stable IDs for sprites, backgrounds, effects, and UI art.

Rules:

- ViewModels must request asset IDs, not paths.
- Game systems must not open Avalonia resources directly.
- Asset loading should happen before scene reveal, not during frame rendering.

Acceptance criteria:

- Card, character, background, and effect images are served from one cache.
- Missing assets return an explicit placeholder and log the ID.

### Phase 5.5: UI Feedback Audio

The recent `SoundService` commits add global hover and click feedback. This belongs to the feedback layer, but it should remain cheap enough to use across the whole shell.

Optimize:

- Avoid constructing media objects on every hover if profiling shows spikes.
- Cache or pool short SFX resources where the selected audio backend allows it.
- Throttle hover ticks so pointer movement across nested controls does not trigger repeated playback.
- Respect a user-facing SFX enable/volume setting in `ClientSettings`.
- Keep audio initialization asynchronous and non-blocking.

Acceptance criteria:

- Rapidly moving across buttons does not produce measurable frame drops.
- Sound failures degrade silently and do not affect input responsiveness.
- SFX can be disabled independently from music.

### Phase 6: Memory Minigame Boundary

Move memorama rules out of `SinglePlayerGameViewModel`.

Create:

```text
Game/MiniGames/Memory/MemoryGameSession.cs
Game/MiniGames/Memory/MemoryBoard.cs
Game/MiniGames/Memory/MemoryCard.cs
Game/MiniGames/Memory/MemoryGameResult.cs
Game/MiniGames/Memory/MemoryRewardCalculator.cs
```

The minigame should emit `MemoryGameResult`; it should not directly change Katya affinity. The dating engine consumes the result and applies coins, penalties, bonuses, and unlocks.

Acceptance criteria:

- Memory game can be unit tested without Avalonia.
- `SinglePlayerGameViewModel` becomes a presentation adapter.

### Phase 7: Memory Board Optimization

Short term:

- Preload all card faces before game start.
- Simplify the XAML template.
- Avoid expensive overlays per card.
- Use transform-only animations.
- Replace reflection binding in hot templates where possible.
- Preserve the recent 5:7 card aspect ratio and compact board chrome improvements.

Long term:

- Implement a Skia-rendered memory board with coordinate-based hit testing.
- Keep Avalonia buttons only for HUD and modal controls.

Acceptance criteria:

- Starting a hard board does not decode images during interaction.
- Flipping cards does not trigger full-page layout churn.

### Phase 8: Visual Novel Scene Renderer

Create:

```text
Engine/VisualNovel/VisualNovelSceneRenderer.cs
Engine/VisualNovel/CharacterRenderer.cs
Engine/VisualNovel/SceneTransitionRenderer.cs
Engine/VisualNovel/EmotionEffectRenderer.cs
```

Layer order:

1. background,
2. parallax atmosphere,
3. character sprites,
4. foreground particles,
5. transition overlays,
6. Avalonia HUD/dialogue UI above the Skia control.

Acceptance criteria:

- Dialogue scenes can change Katya expressions without recreating Avalonia image controls.
- Scene transitions are drawn by Skia and controlled by the unified loop.

## Graphics Presets

| Preset | Target | Features |
| --- | --- | --- |
| Ultra | 60 FPS | Full particles, parallax, bloom-like effects, transitions |
| Normal | 60 FPS | Reduced particles, cheaper bokeh, standard transitions |
| Battery | 30 FPS | Minimal particles, static or slow background, no expensive effects |

Preset settings should be stored in `ClientSettings` and applied through the rendering services.

## Threading Rules

- Avalonia UI state changes stay on the UI thread.
- Skia drawing happens through Avalonia render integration.
- Asset decoding and manifest parsing can run asynchronously before scene activation.
- Game rule calculations should be UI-independent and testable.
- Do not mutate live render collections while a draw operation is reading them; use snapshots or double buffering.

## Memory Rules

- Dispose `SKPaint`, `SKPath`, `SKShader`, `SKImage`, `SKTypeface`, and temporary surfaces when no longer needed.
- Reuse paints and paths in renderers.
- Avoid creating gradients and shaders every frame.
- Decode large backgrounds to the maximum needed display size, not original size unless required.
- Use LRU eviction for large scene assets.

## Acceptance Checklist

- One active game loop controls animation systems.
- No per-frame resource stream loading.
- No per-frame `WriteableBitmap` locking for particles.
- Main animated background is Skia-rendered.
- Liquid-glass and panel hover effects are measured and can be reduced by graphics preset.
- UI hover/click SFX are measured and can be disabled independently from music.
- Dating dialogue scenes use a Skia scene renderer.
- Memory game rules are separated from ViewModels.
- Debug overlay confirms stable frame time.
- Graphics presets can reduce workload without changing gameplay.
