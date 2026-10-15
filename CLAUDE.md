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

## Before checking in

Always run both of the following from the repo root and confirm both pass
before committing. Do not commit code that fails either check.

1. **Build** – run `dotnet build` from `Asteroids/` and confirm it
   reports **0 errors**.

2. **Unit tests** – run `dotnet test Asteroids.Tests/Asteroids.Tests.csproj`
   from the repo root and confirm it reports **0 failed** tests.
