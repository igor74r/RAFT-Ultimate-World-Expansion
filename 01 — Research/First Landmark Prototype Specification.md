---
title: First Landmark Prototype Specification
tags: [raft, world, landmark, prototype, specification]
status: design
date: 2026-11-06
---

# First Landmark Prototype Specification

## 1. Goal and non-goals

The first prototype is a tiny, original, isolated Landmark whose only purpose is to prove the complete custom-content path in Raft.

It is not the first production island and it is not a procedural world-generation feature. It must prove one step at a time that a mod-owned scene can be packaged, discovered, registered, initialized, spawned, removed, and—only after that—persisted.

**Non-goals for this prototype:**
- No new biome or ChunkPointType.
- No edits to vanilla spawn rules, vanilla Landmark assets, or original game files.
- No custom creatures, loot tables, quests, NPCs, economy, or multiplayer features.
- No integration with a valuable existing save.
- No claim that a method name or a successful AssetBundle load equals successful Landmark registration.

## 2. Prototype identity

| Property | Proposed value | Status |
|---|---|---|
| Internal name | UWE_TestAtoll | Design choice |
| Display name | UWE Test Atoll | Temporary |
| Bundle key | uwe_test_atoll | Provisional; must match verified game conventions |
| Scene name | UWE_TestAtoll_Scene | Provisional |
| Mod namespace | com.igor74r.raftuwe | Proposed; confirm repository owner preference before public release |
| Stable content key | com.igor74r.raftuwe.landmark.test-atoll.v1 | Proposed; not a vanilla numeric index |
| Numeric Landmark ID | Not assigned | Must be resolved from verified allocation/collision behavior |
| Chunk point type | None | Explicitly no custom enum value |

Names and string keys are mod-owned design identifiers, not claims about the game's required naming convention. Bundle key, scene naming and numeric ID remain subject to static API/IL verification.

## 3. Minimal scene content

The first scene should contain only:
- one small original island terrain mesh or primitive placeholder;
- a clearly visible landmark marker/object;
- a simple collision surface sufficient for safe player interaction;
- one spawn/arrival reference only if the game's verified Landmark contract requires it;
- no gameplay loot, scripted enemies, or destructive triggers.

All content must be original or explicitly licensed for redistribution. No extracted vanilla meshes, textures, audio, prefabs, or scenes are required.

## 4. Proposed logical components

These are project responsibilities, not existing Raft API types.

| Component | Responsibility | Allowed in first prototype |
|---|---|---|
| WorldContentManifest | Mod-owned key, version, scene and asset declarations | Yes |
| LandmarkAssetProvider | Read embedded bundle bytes and own bundle lifetime | Only after API verification |
| LandmarkRegistrationAdapter | Translate mod manifest to the verified Raft registration path | Only after registration path is proven |
| LandmarkLifecycleObserver | Log registration/initialize/spawn/remove milestones | Yes, read-only |
| LandmarkPlacementPolicy | Decide whether/where host may place content | No placement until host authority and chunk mapping are proven |
| LandmarkPersistenceAdapter | Store stable mod-owned state and migrations | Deferred |
| LandmarkNetworkAdapter | Synchronize host-created state | Deferred |

Do not create these classes merely to imitate this table before the integration contract is known. Keep the existing single RML entry point until the smallest vertical slice is proven.

## 5. Lifecycle contract to prove

The exact order is unknown. Research must establish whether the game's actual path is equivalent to this conceptual sequence:

1. Mod loads and validates its manifest.
2. Embedded asset bytes are obtained.
3. Bundle is loaded and owned by a well-defined lifetime.
4. Game-compatible scene and Landmark metadata are discovered.
5. The game's expected manager/hooker registration path is invoked.
6. Landmark receives the expected initialization callback.
7. Placement/spawn occurs under the correct authority.
8. Removal/unload cleans up spawned objects and bundle resources.

This is a list of proof obligations, not an implementation recipe. Do not call game methods in an assumed order.

## 6. Safety and ownership rules

- Every mod-owned identifier must be namespaced and stable across releases.
- Never allocate a numeric Landmark ID by guessing an unused vanilla value.
- Never mutate vanilla ChunkPointType, spawner rules, or save arrays.
- Bundle loading must have explicit ownership and unload semantics; avoid unloading assets still referenced by live scene objects.
- Registration must be idempotent or guarded against duplicate initialization.
- Failure at any step must leave the vanilla world unchanged.
- Prototype tests must use a new disposable world and a backup/rollback plan.
- Host/server authority must be established before any world placement or network-visible state change.
- If no safe registration route exists, stop and document the blocker rather than patching arbitrary internals.

## 7. Acceptance gates

| Gate | Test | Pass condition |
|---|---|---|
| P0 — Static contract | Metadata and IL review | Exact signatures, ownership, call order, key format and ID behavior documented |
| P1 — Asset package | Offline bundle inspection | Bundle opens in a compatible toolchain; expected scene and assets are present |
| P2 — Registration | Disposable single-player world | One Landmark is discovered and initialized once; no errors or vanilla changes |
| P3 — Placement | Disposable world, controlled test | Placement follows verified chunk rules and does not duplicate or collide |
| P4 — Cleanup | Remove/unload/reload test | No orphaned objects, duplicate registrations, or invalid asset references |
| P5 — Persistence | Save/close/reload | Stable mod-owned identity; no loss or duplication |
| P6 — Multiplayer | Host plus joining client | Consistent host-authoritative state across join/reconnect |

A gate is not passed by source inspection alone if it requires a runtime test. Record game build, RML version, test world type, logs, screenshots and exact artifact hashes for each runtime gate.

## 8. Current blockers

1. Raw game assemblies and IL dumps are not present in this GitHub repository.
2. Manager/hooker ownership and creation path are not established.
3. Exact AssetBundle key and scene lookup conventions are not established.
4. Landmark numeric ID allocation/collision rules are not established.
5. Registration-vs-bookkeeping semantics of AddLoadedLandmark and TrackAddressableLandmark are not established.
6. Custom persistence and network authority are not established.
7. Compatible Unity asset authoring workflow for this exact Raft build is not validated.

## 9. Next repository-only work

- Keep this specification as the design baseline.
- Add a metadata/IL evidence artifact only when its source and exact game build are recorded.
- Build an API evidence matrix: member signature, declaring type, static/instance, call sites, relevant IL behavior, confidence and unresolved questions.
- Do not write a guessed registration adapter or placement code.
- Do not change the v0.1.2 runtime status without actual in-game evidence.

## Current decision

The first prototype is specified but **not implemented**. The next technical deliverable is the API evidence matrix for AssetBundleManager, AssetBundleHooker, SceneLoader, Landmark and LandmarkData. If the required IL cannot be obtained without leaving the repository-only workflow, record that as an external evidence dependency and continue with repository documentation only.


## Related API evidence

See [[Landmark API Evidence Matrix]] for the current member-by-member evidence boundary and the exact metadata/IL questions that must be answered before implementation.
