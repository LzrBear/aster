# Asteroids

A 3D Asteroids game built with MonoGame 3.8.1 / .NET 9, migrated from the original XNA 4.0 / .NET Framework 4.0 codebase.

![CI](https://github.com/LzrBear/aster/actions/workflows/ci.yml/badge.svg)

## Gameplay

- Pilot your ship through an asteroid field and shoot them down before they hit you
- Asteroids crack and shatter — shoot a large one and it breaks into 3 medium fragments; a medium breaks into 2 small ones; fragments spawn inside the explosion cloud
- Difficulty ramps up over 120 seconds: the field starts sparse and slow, then fills to full speed
- Asteroids obey elastic collision physics, so they bounce off each other with proper momentum transfer
- Larger asteroids do more crack damage to smaller ones on collision
- Ship thrust is logarithmic — responsive acceleration that tapers as you approach top speed; no drag, so speed is conserved in the Newtonian tradition
- You lose a life when an asteroid hits your ship; game over when all lives are lost
- High scores are saved between sessions (AES-256 encrypted, HMAC-SHA256 integrity checked)

### Controls

| Action | Keyboard | Gamepad |
|---|---|---|
| Thrust | `W` / `↑` | Right Trigger |
| Rotate Left | `A` / `←` | Left Stick |
| Rotate Right | `D` / `→` | Left Stick |
| Fire | `Space` | `A` |
| Warp to Center | `Home` | `Start` |
| Quit | `Escape` | `Back` |

### Scoring

| Event | Points |
|---|---|
| Destroy large asteroid | +10 |
| Destroy medium asteroid | +25 |
| Destroy small asteroid | +50 |
| Fire a shot | −1 |
| Ship destroyed | −100 |
| Warp | −50 |

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- Windows (MonoGame WindowsDX backend)

## Building & Running

```bash
cd Asteroids
dotnet run --project Asteroids.csproj
```

The MonoGame content pipeline runs automatically as part of the build via `MonoGame.Content.Builder.Task`.

## Tests

```bash
dotnet test Asteroids.Tests/Asteroids.Tests.csproj
```

17 xUnit tests covering `HighScoreManager` (save/load round-trip, tamper detection, sort order, capacity) and `GameConstants` (size rank ordering, fragment counts, speed and difficulty invariants).

## Creating a Windows Installer

1. Publish a self-contained win-x64 build:
   ```bash
   dotnet publish Asteroids/Asteroids.csproj /p:PublishProfile=win-x64
   ```
2. Install [Inno Setup 6](https://jrsoftware.org/isdl.php) and compile `installer/Asteroids.iss`. The installer packages the publish output into a standard Windows setup wizard.

## Project Structure

```
.github/workflows/   # GitHub Actions CI (build + test on every push/PR)
Asteroids/           # Game source (.cs), publish profiles
AsteroidsContent/    # Game assets (models, textures, audio, shaders)
Asteroids.Tests/     # xUnit test project
fbxconvert/          # Utility: FBX 6.0 → DirectX .x format
installer/           # Inno Setup script for the Windows installer
```

## Tech Notes

- **XNA → MonoGame**: Migrated from XNA 4.0 to MonoGame 3.8.1.303 (WindowsDX)
- **Model format**: Original FBX 6.0 files are too old for Assimp; a custom converter (`fbxconvert/`) converts them to DirectX `.x` ASCII format
- **Coordinate system**: FBX uses right-handed coordinates; the converter negates Z and reverses triangle winding to match XNA/DirectX left-handed conventions
- **Particle system**: GPU-driven particles using a circular vertex buffer; uses `HalfVector2` for corner offsets (required for DX11 — `Short2` is an integer format and cannot be read as `float2` in HLSL)
- **Shader**: `ParticleEffect.fx` compiled to `vs_4_0_level_9_1` / `ps_4_0_level_9_1`
- **High scores**: `highscores.dat` is AES-256-CBC encrypted with a fresh IV per save; an HMAC-SHA256 over IV + ciphertext is stored in the file header — any modification is detected and the file is discarded
- **Constants**: All magic numbers and string literals live in `GameConstants.cs` — see `CLAUDE.md` for the enforcement rule
