# Katya Katya Game Architecture Specification

## Product Shape

`Katya Katya` is a dating simulator and visual novel. The memory game remains in the project, but it becomes a supporting activity inside the dating loop rather than the primary architecture driver.

The intended player loop is:

1. Visit the Dating Hub.
2. Interact with Katya through dialogue, gifts, dates, and unlocked scenes.
3. Play the memory minigame to earn coins, streak bonuses, and rare drops.
4. Spend coins on dates and gifts.
5. Increase Katya affinity.
6. Unlock new story scenes, expressions, mood states, diary entries, and milestones.

## Architectural Principle

The application should be organized around the dating simulator domain, not around lobby or memory-game concepts.

Recommended top-level ownership:

```text
Game/
  Dating/
  Narrative/
  Economy/
  MiniGames/
Rendering/
  Core/
  Skia/
  VisualNovel/
  Assets/
Feedback/
  Audio/
  HapticsOrJuice/
Presentation/
  Dating/
  Dialogue/
  MiniGames/
  Shell/
```

## Dating Simulator Core

The dating core owns relationship progression and emotional state.

Responsibilities:

- Track coins.
- Track Katya love points and permanent level.
- Track mood, penalties, and reconciliation state.
- Apply rewards from minigames.
- Apply effects from dialogue choices.
- Resolve gifts and dates.
- Unlock scenes at affinity thresholds.
- Persist dating progress.

Key services:

```text
DatingEngine
AffinitySystem
MoodSystem
GiftSystem
DateSystem
MilestoneUnlockSystem
RewardService
DatingProgressRepository
```

## Narrative Core

The narrative engine owns dialogue, choices, scene flow, and story flags.

Responsibilities:

- Load scene definitions.
- Start dialogue scenes.
- Advance dialogue nodes.
- Filter choices by conditions.
- Apply choice effects.
- Emit render state changes such as background, music, expression, and transition.
- Signal scene completion and unlocks.

Dialogue content should live in data files, not in ViewModels.

Recommended content format:

```text
Resources/VisualNovel/Content/
  scenes/
    katya_intro.json
    katya_level_10.json
    katya_reconciliation.json
  characters/
    katya.json
```

## Visual Novel Rendering

Avalonia should handle UI. Skia should handle the premium visual scene.

Avalonia owns:

- buttons,
- dialogue box,
- choice list,
- settings,
- modals,
- HUD counters,
- accessibility-friendly text.

Skia owns:

- backgrounds,
- Katya sprites,
- expression transitions,
- atmospheric particles,
- parallax,
- screen transitions,
- mood effects.

The main renderer should consume a `SceneRenderState`, not bind directly to ViewModels.

## Feedback Layer

Recent commits added global hover and click sound effects. That direction fits the dating simulator well: Katya Katya should feel responsive and tactile, but feedback systems should stay separate from game rules and rendering.

The feedback layer owns:

- button hover and click SFX,
- reward sounds,
- affinity gain sounds,
- scene unlock sounds,
- optional future screen shake or subtle UI pulses.

Rules:

- Feedback should react to UI/game events, not contain domain decisions.
- SFX settings should be independent from music settings.
- Hover/click feedback should be cheap enough for global shell use.
- Dating-specific emotional feedback should be triggered by domain events such as `AffinityGained`, `MilestoneUnlocked`, or `KatyaMoodChanged`.

## Memory Minigame Boundary

The memory game should become a minigame module.

It should know:

- board layout,
- card state,
- matching rules,
- timer,
- attempts,
- completion or abandonment.

It should not know:

- Katya affinity,
- dating milestones,
- gift unlocks,
- story scenes,
- mood consequences beyond emitting abandonment/result data.

The output is `MemoryGameResult`; the dating engine converts that into coins, drops, penalties, or dialogue hooks.

Current code note: recent commits moved the multiplayer board resolution into the lobby view area. Refactor references should treat the multiplayer board as legacy/optional lobby UI, while the dating loop should depend only on the single-player memory minigame result contract.

## Presentation Layer

ViewModels should coordinate screens and commands. They should not hold deep game rules or render internals.

Examples:

- `DatingHubViewModel`: shows Katya status and available actions.
- `DialogueSceneViewModel`: exposes current line, speaker, choices, and commands.
- `MemoryGameViewModel`: adapts `MemoryGameSession` to the board UI.
- `GameCompletionViewModel`: displays rewards and navigates back to the dating hub.

## Persistence

Progress should be stored through a repository abstraction so the client can support both offline and server-backed progress.

Persist:

- coin balance,
- Katya affinity and level,
- mood,
- penalty history,
- gift log,
- dates completed,
- unlocked scenes,
- story flags,
- minigame stats,
- streaks.

## Initial Migration Path

1. Add `Game/Dating`, `Game/Narrative`, `Game/Economy`, and `Game/MiniGames/Memory`.
2. Move memory rules out of `SinglePlayerGameViewModel`.
3. Add `DatingHubViewModel` and connect `GoToDating()`.
4. Add local `DatingProgressRepository`.
5. Apply memory completion rewards to coins.
6. Add gifts and dates as data-driven definitions.
7. Add dialogue scene engine and first Katya scene.
8. Connect visual novel renderer for scene backgrounds and Katya expressions.
9. Add mood and penalty handling.
10. Add feedback events for SFX, reward sounds, and affinity gain sounds.
11. Add server sync after local behavior is stable.
