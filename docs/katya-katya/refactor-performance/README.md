# Katya Katya Refactor And Performance Documentation

This folder documents the performance refactor for `KatyaKatya.Client`: frame-loop ownership, Skia rendering, asset caching, instrumentation, and memory-minigame optimization.

## Files

- `optimization-spec.md`: implementation specification for reducing frame drops and stabilizing rendering.
- `performance-rendering-pipeline.mmd` / `performance-rendering-pipeline.png`: target rendering and asset pipeline.
- `frame-budget-sequence.mmd` / `frame-budget-sequence.png`: per-frame update and render lifecycle.
- `asset-lifecycle.mmd` / `asset-lifecycle.png`: asset cache lifecycle from preload to disposal.
- `performance-instrumentation.mmd` / `performance-instrumentation.png`: what to measure and where it appears.
- `memory-minigame-optimization.mmd` / `memory-minigame-optimization.png`: staged plan to reduce memory board lag.

## Reading Order

1. Start with `optimization-spec.md`.
2. Review `performance-rendering-pipeline.png`.
3. Review `frame-budget-sequence.png`.
4. Review `asset-lifecycle.png`.
5. Review `performance-instrumentation.png`.
6. Review `memory-minigame-optimization.png`.
