---
title: AssetBundle and Landmark Integration Plan
tags: [raft, research, assetbundle, landmark, world-generation]
status: research
date: 2026-11-06
---

# AssetBundle and Landmark Integration Plan

## Purpose

Define the next static-research and proof sequence for adding one custom, isolated Landmark to Raft. This document records verified local API observations separately from hypotheses. It does not claim that custom Landmark registration is currently supported.

## Known facts from the local Raft 1.1.01 / RML 2.8.10 audit

- `SceneLoader` exposes metadata and scene-related methods including `GetLandmarksFromAssetBundleKey(String)`, `LoadScenes()`, `AddLoadedLandmark(GameObject, Landmark)`, `TrackAddressableLandmark(String, LandmarkData)`, and `InitializeLoadedLandmark(Landmark)`.
- `Landmark` is a game type with a unique index and lifecycle methods including `Initialize()`, `OnSpawn()`, and `OnRemove()`.
- `AssetBundleManager` has fields including `assetBundleTag`, `fileDirectory`, `bundleLandmark`, and `asyncedLandmarks`; inspected method names include `LoadAssetBundle()`, `AddLandmarksToHooker()`, `OnLoadGame()`, `StartSceneLoading()`, `LoadSpecificScene(String)`, `LoadScenePaths()`, `GetGameObjectsFromScene(String)`, and `UnloadBundle(Boolean)`.
- `AssetBundleHooker` stores a string-keyed dictionary of `AsyncedLandmark` lists and exposes lookup by bundle key or unique index.
- `HMLLibrary.Mod.GetEmbeddedFileBytes(String)` exists in the local RML assembly.
- Vanilla chunk points and landmarks participate in save/network data structures. `ChunkPointType` is a compiled enum with vanilla values; no dynamic enum-registration API was found in the inspected surface.

These are static metadata findings. Method names alone do not establish call order, valid arguments, required object ownership, or a supported mod extension point.

## Unproven integration hypothesis

A possible path is:

1. Include a custom AssetBundle as an embedded file in the mod package.
2. Obtain its bytes through the mod API.
3. Load the bundle using Unity's AssetBundle API.
4. Register or expose the contained scene/Landmark through the game's expected manager/hooker path.
5. Let the game's normal scene and Landmark lifecycle initialize it.

Every step after file-byte retrieval remains unproven. In particular, do not assume that calling `AssetBundle.LoadFromMemory` alone registers a Landmark with Raft.

## Static investigation checklist

Before writing integration code, inspect the local metadata/IL for:

- Exact declaring type, static/instance status, visibility, overloads, and parameter/return types for every relevant method.
- Constructors and field types for `AssetBundleManager`, `AssetBundleHooker`, `AsyncedLandmark`, `LandmarkData`, and `Landmark`.
- Where the manager/hooker instances are created, stored, and accessed; whether they are persistent, static, or owned by another component.
- The call order from game startup/world load through bundle loading, hook registration, scene lookup, Landmark initialization, spawn, and unload.
- How bundle keys and scene paths are formed and compared (case sensitivity, prefixes, extensions, and normalization).
- How Landmark unique indices are allocated and checked for collisions.
- Whether `AddLoadedLandmark` and `TrackAddressableLandmark` are registration methods, bookkeeping methods, or only post-load notifications.
- Which fields are serialized into `RGD_World`, `RGD_Landmark`, and network messages, and whether custom IDs can survive save/reload and join-in-progress.

Use a metadata/IL reader or offline disassembler. Do not invoke game methods or instantiate game objects during this research pass.

## Evidence levels

| Level | Required evidence | What it permits |
|---|---|---|
| A — Metadata | Signatures, fields, call sites and IL control flow | Document a plausible integration path; no implementation claim |
| B — Offline asset | A minimal test bundle opens in a compatible Unity toolchain and contains the expected scene/assets | Validate asset pipeline only |
| C — Controlled runtime | One isolated test Landmark registers and initializes in a disposable world without errors | Claim a single-player prototype works on the tested build |
| D — Persistence | Save, close, reload, and confirm stable identity and no duplication/loss | Claim tested save/reload behavior |
| E — Multiplayer | Host creates/loads it; a joining client receives consistent state; disconnect/reconnect remains correct | Claim compatibility only for the tested network configuration |

No level may be skipped by inference from method names.

## First prototype constraints

When and only when levels A and B establish a viable route:

- Use one small, original test Landmark with no borrowed proprietary assets.
- Use a reserved mod-owned stable identifier; never collide with vanilla indices.
- Keep the prototype behind a configuration flag and limit it to a disposable test world.
- Do not modify vanilla `ChunkPointType` values or vanilla spawn rules.
- Prefer host/server-authoritative registration if the game architecture supports it.
- Add explicit cleanup/unload behavior and diagnostics.
- Do not test on a valuable save.

## Current decision

**No custom island registration code should be written yet.** The next task is static IL/ownership analysis of the AssetBundle and SceneLoader path. The v0.1.2 read-only probe remains file-staged but not runtime-validated; this research plan does not require launching Raft.
