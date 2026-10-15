# Asteroids

A 3D Asteroids game built with MonoGame 3.8.1 / .NET 9, migrated from the original XNA 4.0 / .NET Framework 4.0 codebase.

## Gameplay

- Pilot your ship through an asteroid field and shoot them down before they hit you
- Asteroids bounce off each other and spawn from outside the screen edges
- You lose a life when an asteroid hits your ship
- Game over when all lives are lost

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
| Destroy asteroid | +25 |
| Fire a shot | -1 |
| Ship destroyed | -100 |
| Warp | -50 |

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- Windows (MonoGame WindowsDX backend)

## Building & Running

```bash
cd "3D Model/3D Model"
dotnet run --project Asteroids.csproj
```

The MonoGame content pipeline runs automatically as part of the build via `MonoGame.Content.Builder.Task`.

## Project Structure

```
3D Model/
  3D Model/           # Game source code (.cs files)
  3D ModelContent/    # Game assets (models, textures, audio, shaders)
fbxconvert/           # Utility to convert FBX 6.0 models to DirectX .x format
```

## Tech Notes

- **XNA → MonoGame**: Migrated from XNA 4.0 to MonoGame 3.8.1.303 (WindowsDX)
- **Model format**: Original FBX 6.0 files are too old for Assimp; a custom converter (`fbxconvert/`) converts them to DirectX `.x` ASCII format
- **Coordinate system**: FBX uses right-handed coordinates; the converter negates Z and reverses triangle winding to match XNA/DirectX left-handed conventions
- **Particle system**: GPU-driven particles using a circular vertex buffer; uses `HalfVector2` for corner offsets (required for DX11 — `Short2` is an integer format and cannot be read as `float2` in HLSL)
- **Shader**: `ParticleEffect.fx` compiled to `vs_4_0_level_9_1` / `ps_4_0_level_9_1`
