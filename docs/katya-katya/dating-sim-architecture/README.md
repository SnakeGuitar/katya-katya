# Katya Katya Dating Simulator Architecture

This folder documents the target architecture for `Katya Katya` as a dating simulator and interactive visual novel. The inherited memory game is treated as a minigame that feeds the dating loop through coins, rewards, penalties, and special events.

## Files

- `game-architecture.md`: implementation-oriented overview of the target game architecture.
- `game-components.mmd` / `game-components.png`: component diagram for the full dating simulator.
- `domain-classes.mmd` / `domain-classes.png`: class diagram for dating progression, narrative, economy, scenes, and memory minigame boundaries.
- `core-gameplay-sequence.mmd` / `core-gameplay-sequence.png`: sequence diagram for the full loop: play minigame, earn coins, spend on Katya, unlock story.
- `dialogue-choice-sequence.mmd` / `dialogue-choice-sequence.png`: sequence diagram for dialogue choices, affinity changes, mood, unlocks, and rendering.
- `state-machines.mmd` / `state-machines.png`: state machines for application flow, dating hub, dialogue, affinity mood, and memory minigame.
- `window-flow-current.mmd` / `window-flow-current.png`: simplified current screen flow.
- `window-flow-target.mmd` / `window-flow-target.png`: simplified target dating simulator screen flow.
- `window-flow-migration-map.mmd` / `window-flow-migration-map.png`: focused migration map from current areas to target areas.
- `katya-personality-model.mmd` / `katya-personality-model.png`: compact personality trait model.
- `katya-reaction-triggers.mmd` / `katya-reaction-triggers.png`: compact trigger-to-reaction map.
- `katya-gameplay-impact.mmd` / `katya-gameplay-impact.png`: compact mood-to-output-to-gameplay-impact map.

## Reading Order

1. `game-architecture.md`
2. `game-components.png`
3. `core-gameplay-sequence.png`
4. `dialogue-choice-sequence.png`
5. `domain-classes.png`
6. `state-machines.png`
7. `window-flow-current.png`
8. `window-flow-target.png`
9. `window-flow-migration-map.png`
10. `katya-personality-model.png`
11. `katya-reaction-triggers.png`
12. `katya-gameplay-impact.png`

The diagrams are intentionally split into smaller views so each one can be used directly during implementation discussions.
