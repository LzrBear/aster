# Asteroids – Agent Instructions

## Constants and strings

All numeric constants and string literals must be defined in `GameConstants.cs`.
Do **not** hardcode magic numbers or string literals inline in game logic files.

This includes:
- Content paths (fonts, textures, models, audio)
- UI text (titles, prompts, HUD labels, hints)
- UI layout values (positions, scales, spacing)
- Gameplay tuning values (speeds, timers, distances, thresholds)
- Window and camera settings
- File names and separators

When adding a new feature, add its constants to the appropriate section in
`GameConstants.cs` first, then reference them from your implementation.
