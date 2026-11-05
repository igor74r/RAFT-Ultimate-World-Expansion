# Raft: Ultimate World Expansion

Глобальный мод для **Raft**. Проект находится на ранней стадии: подтверждённая совместимость и безопасное ядро важнее количества контента.

## Текущее состояние

- Raft: **1.1.01** (Steam build **22312909**)
- Raft Mod Loader: **2.8.10**
- Последний кандидат: **v0.1.2 — read-only World Probe**
- Core strict preflight compile: passed
- RML package integrity: verified
- v0.1.2 package staged in the local game's mods folder; **in-game validation is pending**
- Custom island/world generation: **not implemented**
- Current work: static/offline research of Landmark, SceneLoader, AssetBundle and world-generation lifecycle. The game does not need to be launched for this research.

## Project documentation

- [Master Prompt](00%20%E2%80%94%20MASTER%20PROMPT.md)
- [Roadmap](01%20%E2%80%94%20Roadmap.md)
- [Technical Audit](02%20%E2%80%94%20Technical%20Audit.md)
- [Architecture](03%20%E2%80%94%20Architecture.md)
- [Modules](04%20%E2%80%94%20Modules.md)
- [Testing](05%20%E2%80%94%20Testing.md)
- [Changelog](06%20%E2%80%94%20Changelog.md)
- [Known Issues](07%20%E2%80%94%20Known%20Issues.md)
- [RML API map](01%20%E2%80%94%20Research/RML%20API%20map.md)
- [World Generation API Audit](01%20%E2%80%94%20Research/World%20Generation%20API%20Audit.md)
- [AssetBundle and Landmark Integration Plan](01%20%E2%80%94%20Research/AssetBundle%20and%20Landmark%20Integration%20Plan.md)
- [First Landmark Prototype Specification](01%20%E2%80%94%20Research/First%20Landmark%20Prototype%20Specification.md)

## Development rules

1. Do not invent game APIs or claim unverified compatibility.
2. Keep static findings separate from runtime-confirmed findings.
3. Make small, reviewable commits and preserve rollback points.
4. Do not change original saves or live game state during research.
5. Do not mark v0.1.2 runtime-tested until an actual in-game result is recorded.
6. Keep proprietary game binaries, local build outputs, backups, logs and packaged releases out of the source repository unless explicitly reviewed.

## Build

The RML project source and Core source are under their respective directories. The RML `.csproj` uses overridable MSBuild properties: `RaftDir` (default Steam install), `RmlDataDir` (default `%APPDATA%\\RaftModLoader`), `RmlPublicizedDir`, and `RmlBinariesDir`. The Core build script accepts the `RAFT_DIR` environment variable (default Steam install). Set these values for non-standard installations before building. These path-portability edits have not yet been compiled on a second machine.
- [Landmark API Evidence Matrix
- [v0.1.2 Diagnostic Source Review](01%20%E2%80%94%20Research/v0.1.2%20Diagnostic%20Source%20Review.md)
](01%20%E2%80%94%20Research/Landmark%20API%20Evidence%20Matrix.md)
