---
title: Landmark API Evidence Matrix
tags: [raft, research, api, landmark, assetbundle]
status: partial-evidence
date: 2026-11-06
game: Raft 1.1.01 / Steam 22312909
loader: RML 2.8.10
---

# Landmark API Evidence Matrix

## Purpose and evidence boundary

This matrix converts the existing local audit notes into explicit proof obligations. It is **not** a fresh assembly inspection: the raw game assemblies, metadata dump and IL listing are not present in this GitHub repository. Names and observations below are transcribed from the existing project audit and must be revalidated against the exact target build before code is written.

Evidence labels:
- **Observed (audit note):** recorded in the prior local static audit; exact signature or IL is not preserved here.
- **Type/member surface:** type or member name was observed, but semantics are unknown.
- **Runtime observation:** supported by the v0.1.1 read-only in-game probe; does not establish custom registration.
- **Unknown:** must not be guessed.
- **Not tested:** requires controlled runtime evidence.

## 1. API matrix

| Type / member | Recorded evidence | What it safely tells us | Missing proof | Status |
|---|---|---|---|---|
| ChunkManager.chunkPoints | Field name and dictionary shape recorded; runtime dictionary had 3 keys | Vanilla chunk points are grouped by point type | Exact generic types, mutability, key/value ownership, insertion invariants | Observed + v0.1.1 runtime count |
| ChunkManager.allChunkSpawners | Field name recorded; runtime count 17 | Vanilla spawner collection exists | Element-to-point mapping, index stability, rule ownership | Observed + v0.1.1 runtime count |
| ChunkManager.AddChunkPoint(ChunkPoint) | Method name recorded | A method accepts a ChunkPoint | Visibility, exact signature, preconditions, side effects, authority, save/network updates | Member name only |
| ChunkManager.AddChunkPointForcibly(ChunkPointType) | Method name recorded | Vanilla point-type-oriented helper exists | Exact effects and whether it can safely support custom content | Member name only; do not use |
| ChunkManager.AddChunkPointCheat(ChunkPointType, Vector3) | Method name recorded | Cheat-oriented helper exists | Exact effects and production safety | Member name only; do not use |
| ChunkPoint.SpawnObject(Int32) | Method name recorded | ChunkPoint has a spawn operation | Parameters, authority, duplicate guards, object lifecycle and serialization | Member name only; do not call |
| ChunkPoint.RemoveObject() | Method name recorded | ChunkPoint has a removal operation | Cleanup semantics and persistence effects | Member name only; do not call |
| ChunkPointType | Compiled enum; vanilla values enumerated in prior audit | No runtime extension registry was found in inspected surface | Whether any separate registration mechanism exists elsewhere | Static audit observation; treat as closed |
| SceneLoader.loadedLandmarks | Field/collection name recorded; count 38 in v0.1.1 runtime probe | Runtime metadata collection contained 38 entries | Whether entries are definitions, loaded scenes, spawned instances or a mixture | Runtime count; not spawned count |
| SceneLoader.GetLandmarksFromAssetBundleKey(String) | Method name and parameter type recorded in audit notes | Lookup is keyed by a string-like bundle key | Exact return type, key normalization, lookup source, caller/callee | Member name only |
| SceneLoader.LoadScenes() | Method name recorded | Scene-loading entry point exists | Call order, coroutine/async behavior, prerequisites, ownership | Member name only |
| SceneLoader.AddLoadedLandmark(GameObject, Landmark) | Method name and parameter types recorded | A GameObject and Landmark are passed together | Whether this registers, tracks, or records an already-loaded object | Semantics unknown |
| SceneLoader.TrackAddressableLandmark(String, LandmarkData) | Method name and parameter types recorded | Addressable Landmark metadata can be tracked by string key | Key contract and whether this is a mod extension point | Semantics unknown |
| SceneLoader.InitializeLoadedLandmark(Landmark) | Method name and parameter type recorded | Initialization helper exists | Required prior state, idempotence, side effects, caller | Semantics unknown |
| SceneLoader.SpawnLandmarkFromUniqueIndex(Int32) | Method name recorded | A spawn path refers to an integer unique index | Allocation, collision handling, network authority, save coupling | Member name only |
| Landmark.uniqueIndex | Field/property described in prior audit as unique index | Landmark identity includes an integer | Allocation source, range, persistence and collision policy | Partial observation |
| Landmark.Initialize() / OnSpawn() / OnRemove() | Lifecycle method names recorded | Landmark has lifecycle phases | Exact order, required components, repeated invocation behavior | Member names only |
| AssetBundleManager.assetBundleTag / fileDirectory / bundleLandmark / asyncedLandmarks | Field names recorded | Manager tracks bundle and Landmark-related state | Exact field types, static/instance status, initialization and ownership | Partial observation |
| AssetBundleManager.LoadAssetBundle() | Method name recorded | Bundle-load operation exists | Parameters, bundle source, return/async contract, call site and error handling | Member name only |
| AssetBundleManager.AddLandmarksToHooker() | Method name recorded | Manager interacts with a hooker | What is registered, which instance, when, and whether mods can participate | Semantics unknown |
| AssetBundleManager.OnLoadGame() / StartSceneLoading() | Method names recorded | Game-load and scene-loading phases exist | Actual order and event dependencies | Member names only |
| AssetBundleManager.LoadSpecificScene(String) / LoadScenePaths() / GetGameObjectsFromScene(String) | Method names recorded | Scene/path/object lookup helpers exist | Path format, bundle scope, exact return types and lifetime | Member names only |
| AssetBundleManager.UnloadBundle(Boolean) | Method name and Boolean parameter recorded | Bundle unload path exists | Meaning of Boolean, reference safety, scene-object lifetime | Member name only; do not call |
| AssetBundleHooker | String-keyed dictionary of AsyncedLandmark lists recorded | Hooker maps string keys to Landmark-related entries | Owner/instance, key grammar, mutation path, registration lifecycle | Partial observation |
| HMLLibrary.Mod.GetEmbeddedFileBytes(String) | Method existence recorded in local RML audit | RML exposes a way to retrieve embedded mod-file bytes | Exact signature/visibility, path convention, null/error behavior, package inclusion rules | Member name only |
| Unity AssetBundle.LoadFromMemory | Unity AssetBundle API presence recorded | Unity has a byte-loading API | Compatible overload, compression/CRC expectations, bundle compatibility and Raft registration bridge | API presence only; not a Raft registration path |
| RGD_World / RGD_ChunkPoint / RGD_Landmark | Save structure names/fields recorded in audit | Vanilla world/point/Landmark state is serialized | Custom extension mechanism, version migration and unknown-field behavior | Static audit observation |
| Message_WorldManager_CreateLandmarks | Network message name and RGD_Landmark[] payload recorded | Vanilla Landmark creation has a network message | Whether arbitrary custom IDs/assets are accepted and replicated | Static audit observation |

## 2. Runtime evidence separated from static evidence

The v0.1.1 F10 probe confirmed:
- ChunkManager: 17 spawner entries, 9 chunk entries, 3 ChunkPointType dictionary keys, ChunkSize 1000.
- SceneLoader.loadedLandmarks: 38 metadata entries.
- AssetBundleManager and AssetBundleHooker types were discoverable; the probe did not find active-scene component instances.

The v0.1.2 bounded probe was historically built and staged according to project notes, but its in-game execution is **not confirmed**. Its source performs reflection and bounded field/method metadata inspection; source code is not a substitute for runtime output.

## 3. Required IL/metadata capture template

For each member needed by the proposed path, capture:

| Field | Required value |
|---|---|
| Target build | Raft version, Steam build ID, Unity version |
| Assembly | Exact assembly name and file hash |
| Declaring type | Fully qualified type name |
| Member | Exact name and overload |
| Signature | Visibility, static/instance, return type, parameter types |
| Fields | Exact field types and static/instance flags |
| Call sites | Caller and call order, including startup/world-load entry point |
| IL behavior | Relevant branch/field writes/calls, not just decompiled method name |
| Ownership | Who creates, stores, and destroys manager/hooker/bundle |
| Exceptions | Null/invalid key/missing scene/unload behavior |
| Confidence | Direct IL / metadata only / inferred |
| Open questions | What remains unresolved |

Preserve raw metadata or IL excerpts with the target assembly hash. Do not upload proprietary game DLLs or game assets to the repository; a text-only, minimal evidence extract is sufficient where legally appropriate.

## 4. Priority investigation order

1. **Manager ownership:** determine whether AssetBundleManager and AssetBundleHooker are static services, scene components, or objects owned by another manager. The absence of scene instances in v0.1.1 does not answer this.
2. **Load call graph:** trace game startup/world load → AssetBundleManager.LoadAssetBundle → AddLandmarksToHooker → SceneLoader.LoadScenes / lookup → initialization.
3. **Registration semantics:** trace every caller and field write for AddLoadedLandmark and TrackAddressableLandmark.
4. **Identity:** trace uniqueIndex assignment, lookup, collision checks, and serialization.
5. **Asset convention:** identify actual bundle key, scene path and embedded-file naming rules from callers and data.
6. **Authority and persistence:** trace server/client guards and save/network payload construction before any placement experiment.

## 5. Decision

**No registration adapter or placement code is authorized by this matrix.** It records partial evidence and names the missing proof. The next implementation decision is blocked until the call graph, manager ownership, identity rules and bundle naming contract are supported by actual metadata/IL evidence.

Repository-only limitation: because the raw assemblies/IL are absent from GitHub, this session can organize known audit evidence but cannot independently complete the static analysis. No local machine, build, Raft process, save, or runtime test was accessed or changed.


## Diagnostic source coverage note

The checked-in v0.1.2 source review is documented in [[v0.1.2 Diagnostic Source Review]]. Its bounded reflection output is not a complete API contract: it omits key Landmark data types, constructor/property detail, assembly-qualified signatures and call-site/IL semantics. Do not treat an absent item in its capped output as proof that the member does not exist.


## v0.1.3 metadata probe design

See the repository document `01 — Research/v0.1.3 Metadata Probe Implementation Plan.md` for the design-only type allowlist, metadata-only member coverage, deterministic bounded output, duplicate handling, truncation reporting, exception isolation, safety checklist, acceptance tests M01–M12, and release gates G0–G5.

Status: design-only. The plan does not supply missing target-build signatures or IL semantics. It does not authorize C# implementation, package creation, Landmark registration, or world mutation.
