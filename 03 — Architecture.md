[Reading 90 lines from start (total: 90 lines, 0 remaining)]

---
title: Architecture
tags: [raft, architecture]
status: planning
---

# Architecture

## Принципы
- Модульность.
- Минимальное вмешательство в оригинальные файлы.
- Проверка зависимостей.
- Логирование и обработка ошибок.
- Безопасные сохранения.
- Возможность отключения необязательных модулей.
- Документированные публичные интерфейсы.

## Модули
- Core
- World
- Underwater
- Building
- Items
- Crafting
- Creatures
- NPC
- Technology
- Automation
- Survival
- Transport
- Economy
- Quests
- Weather
- UI
- Multiplayer
- Localization
- Tests

## Диаграммы
Добавлять Mermaid-диаграммы после подтверждения реальных связей между модулями.

## Зависимости
| Модуль | Зависит от | Статус |
|---|---|---|
| Core | Загрузчик и API игры | Не проверено |


## World module — preliminary architecture (2026-11-06)

**Evidence-based vanilla integration points:**
- Chunk placement: ChunkManager, ChunkPoint, SO_ChunkSpawnRuleAsset, ObjectSpawner_Chunk.
- Landmark scenes: SceneLoader, Landmark, AssetBundleManager, AssetBundleHooker, AsyncedLandmark.
- Persistence/network: RGD_World, RGD_ChunkPoint, RGD_Landmark, Message_WorldManager_CreateLandmarks.

**Design constraint:** ChunkPointType is a compiled enum with vanilla values; no runtime registration extension was found in the inspected API. Avoid treating AddChunkPointForcibly / AddChunkPointCheat as a custom island API. Do not mutate chunk points until server authority, spawner mapping, save/reload and collision rules are understood.

**Proposed dependency flow:** World.Core (read-only discovery/config) → World.Content (embedded AssetBundle + Landmark scene) → World.Spawn (server-authoritative placement) → World.Persistence (stable IDs and save migration). This is a proposal, not implemented code.

First milestone: read-only runtime probe. Second: prove custom AssetBundle registration. Third: one isolated Landmark prototype.


## World.Core diagnostic implementation — 2026-11-06

The first World.Core slice is implemented in the existing single RML entry point:
- On WorldEvent_WorldLoaded, perform one read-only probe.
- Resolve the four known manager types from loaded assemblies.
- Count objects and active-scene component instances using Unity's object discovery API.
- Read only selected manager fields; report collection counts or scalar values, and do not expand arbitrary object graphs.
- Catch per-manager/per-field reflection errors and log them instead of aborting the entire probe.

No chunk mutation, object spawning, AssetBundle loading, save writes, or network messages are performed. The probe is diagnostic scaffolding, not a custom world-generation implementation.


## World probe runtime evidence — 2026-11-06
v0.1.1 runtime probe completed in-game. Current observed vanilla integration surface:
- ChunkManager: 17 spawner entries, 9 chunk entries, 3 ChunkPointType dictionary keys, ChunkSize 1000.
- SceneLoader: 38 loadedLandmarks metadata entries.
- AssetBundleManager / AssetBundleHooker are resolvable types, but the probe did not find active-scene component instances. Do not assume these types are scene components or require scene instances.

Next diagnostic slice should remain read-only and bounded: enumerate dictionary keys and list element type/name summaries only; do not call SpawnObject, AddChunkPoint*, LoadAssetBundle, scene initialization, or any save/network method. Keep the single existing RML entry point until the API boundary is understood.


## World probe runtime evidence — 2026-11-06
v0.1.1 completed its read-only probe in-game. Observed: ChunkManager has 17 spawner entries, 9 chunk entries, 3 ChunkPointType dictionary keys, ChunkSize 1000; SceneLoader has 38 loadedLandmarks metadata entries. AssetBundleManager and AssetBundleHooker resolve as types, but no active-scene component instances were found. Next diagnostic slice: bounded read-only enumeration of dictionary keys and spawner element summaries. Do not call spawn, chunk mutation, bundle load, scene initialization, save, or network methods.


## World.Core v0.1.2 diagnostic boundary — 2026-11-06
The read-only diagnostic pass now maps bounded contents of ChunkManager.chunkPoints and allChunkSpawners and logs metadata-only type surfaces for ChunkPoint, ObjectSpawner_Chunk, SO_ChunkSpawnRuleAsset, AssetBundleManager, AssetBundleHooker, and ChunkPointType. Limits are fixed constants in the Core. Reflection reads fields only; it does not invoke getters, constructors, game methods, or mutate collections. The single RML entry point is retained.

Candidate compiled and packaged, but remains unvalidated in a live game. Do not interpret the new code as proof of custom island registration.

[executed on device: DESKTOP-3MKBODB (8088b63f-2a05-4f71-a7a4-ad740486ff11)]