[Reading 72 lines from start (total: 72 lines, 0 remaining)]

---
title: World Generation API Audit
tags: [raft, research, world-generation, landmarks, chunks]
status: research
date: 2026-11-06
---

# World Generation API Audit — Raft 1.1.01

## Environment
- Game: Raft Steam build 22312909, Raft 1.1.01, Unity runtime 2021.3.45f2.
- Loader: RML 2.8.10.
- Raft process was closed during this static inspection; no live game state was changed.
- Inspected local managed assemblies under Raft_Data/Managed, especially Assembly-CSharp.dll, RaftModLoader_AssemblyLoader.dll and Unity CoreModule.

## Confirmed game types and API surface

### Chunk generation
- ChunkManager exists in Assembly-CSharp.dll.
- Fields include numberOfChunks, currentSpawners, allChunkPointRules, allChunkSpawners, chunks, chunkPoints and ChunkSize.
- Methods include InitializeChunks(), GenerateChunkDataStartOfGame(), GenerateChunkDataAllChunks(), GenerateChunkData(Int32, Int32), GenerateChunkData(Chunk), AddChunkPointForcibly(ChunkPointType), AddChunkPointCheat(ChunkPointType, Vector3), AddChunkPoint(ChunkPoint), AddRGDChunkPoint(RGD_ChunkPoint), GetChunkPointsFromPointType(ChunkPointType), GetAllChunkPointsDictionary(), GetAllChunkPointsList() and GetSpawner(ChunkPointType) overloads.
- ChunkPoint stores world position, spawner, spawn rule, spawnable object, duplicate index, spawned object and state flags; it exposes SpawnObject(Int32), ShowObject(), HideObject() and RemoveObject().
- SO_ChunkSpawnRuleAsset contains spawn count/chance, biome identifier, collision radius, view distance, minimum-distance constraints, spawn distance from raft and spawn angle. It exposes TestSpawnChance() and DoesPointPassSpawnRules(...).

### Closed point-type enum
Local reflection enumerated ChunkPointType:
None, Landmark_Small, Landmark_Big, Landmark_Pilot, Landmark_RadioTower, Landmark_FloatingRaft, Landmark_Boat, Landmark_Test, Landmark_Vasagatan, Landmark_Balboa, Landmark_CaravanIsland, Landmark_Tangaroa, Landmark_VarunaPoint, Landmark_Temperance, Landmark_Utopia.

This is a compiled enum, not a runtime registry. The inspected methods accept this enum; no dynamic point-type registration method was found in the inspected public/private declared surface. Therefore a new island category cannot yet be safely added by merely calling AddChunkPointForcibly with a new type.

### Landmark scene pipeline
- SceneLoader has loaded landmark collections and methods SpawnLandmarkFromUniqueIndex(Int32), GetLandmarksFromAssetBundleKey(String), LoadScenes(), AddLoadedLandmark(GameObject, Landmark), TrackAddressableLandmark(String, LandmarkData) and InitializeLoadedLandmark(Landmark).
- Landmark contains a terrain point generator, treasure manager, unique index, landmark items and entity spawners; lifecycle methods include Initialize(), OnSpawn(), OnRemove() and GenerateTreasures().
- AssetBundleManager contains bundle and scene-loading state (assetBundleTag, fileDirectory, bundleLandmark, asyncedLandmarks) and methods LoadAssetBundle(), AddLandmarksToHooker(), OnLoadGame(), StartSceneLoading(), LoadSpecificScene(String), LoadScenePaths(), GetGameObjectsFromScene(String) and UnloadBundle(Boolean).
- AssetBundleHooker stores a dictionary keyed by string to lists of AsyncedLandmark; it can return landmarks by bundle key or unique index and retrieve scene objects by name.
- This strongly indicates that additional landmark content is scene/AssetBundle-backed. It does not yet prove that an RML mod can register its own bundle with the game's manager.

### Save and network data
- RGD_World contains RGD_ChunkPoint[] rgdChunkPoints and Int32[] chunkManagerSpawnerIndexes, plus biome-zone and other world data.
- RGD_ChunkPoint stores world position, ChunkPointType, spawner index, spawnable unique index, spawned object index and visibility/corruption flags; fast network serialization methods are present.
- Message_WorldManager_CreateLandmarks carries RGD_Landmark[]; WorldManager.RestoreLandmarks(...) restores landmarks.
- This proves existing save/network structures for vanilla chunk points and landmarks, not that arbitrary custom island data is automatically serialized or synchronized.

### RML mod API relevant to asset payloads
- Local HMLLibrary.Mod exposes GetEmbeddedFileBytes(String) in addition to the already tested entry point and lifecycle callbacks.
- Unity AssetBundleModule assembly is present.
- A possible route is to package an AssetBundle as an embedded .rmod file and load its bytes, but the exact path/key convention, scene registration, unique-index allocation, cleanup and multiplayer authority are not confirmed. Do not implement this path until each is verified.

## Local asset inspection
- Raft_Data/StreamingAssets contains Data.rgd, Haptics audio, FMOD banks and Unity Services configuration; no obvious standalone AssetBundles directory exists at the inspected Raft root path.
- Project 03 — Content and 04 — Tests are currently empty.
- This is a scoped directory observation, not proof that the game has no embedded or addressable bundles elsewhere.

## Conclusions
1. The vanilla world uses chunk points plus registered spawners/rules; these are not simply arbitrary GameObjects placed at random.
2. Large/small island content is Landmark/scene-oriented and is loaded through AssetBundle/SceneLoader infrastructure.
3. Vanilla chunk-point and landmark state participates in save/network serialization.
4. No supported extension point for registering a new ChunkPointType or custom AssetBundle was confirmed in this audit.
5. Runtime compatibility of the diagnostic Core is proven, but custom world-generation compatibility is not.

## Recommended implementation sequence
1. Read-only runtime probe: add a diagnostic-only module that, on world-loaded, discovers ChunkManager, SceneLoader, AssetBundleManager and AssetBundleHooker, logs whether each exists and reports safe counts/types. It must not add/remove/spawn chunk points or modify saves.
2. Bundle integration research: verify GetEmbeddedFileBytes, AssetBundle.LoadFromMemory, bundle key conventions, scene naming, hook registration, unique index rules and unload lifecycle.
3. Offline content prototype: only after Unity asset workflow is confirmed, create one tiny test Landmark scene/AssetBundle using a compatible editor/toolchain; no original game assets without permission/license review.
4. Controlled runtime test: test one custom landmark in a disposable world, host-only first, then save/reload and multiplayer.
5. Only then consider extending procedural chunk rules or region/biome systems.

## Explicitly not done
- No game files, save files, live world state or existing mods were changed.
- No chunk point or landmark was spawned/removed.
- No AssetBundle was created or loaded.
- No Unity Editor or SDK was installed.

[executed on device: DESKTOP-3MKBODB (8088b63f-2a05-4f71-a7a4-ad740486ff11)]