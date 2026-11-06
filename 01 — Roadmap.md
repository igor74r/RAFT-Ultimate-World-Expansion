# Authoritative current status — 2026-11-06

This section is the single current status summary. Older entries below are retained as historical session records and must not be read as current instructions or status when they conflict with this section.

## Operating mode
- Repository-only. No local Windows access, compilation, package installation, or Raft launch unless the user explicitly changes this constraint.
- All local build/staging/runtime facts below are historical reports from earlier sessions, not fresh verification in this session.

## Version and test evidence
| Item | Current status | Evidence boundary |
|---|---|---|
| v0.1.1 lifecycle/read-only probe | Runtime-confirmed PASS | User-provided F10 output; Raft 1.1.01 / Steam 22312909 / RML 2.8.10 |
| v0.1.2 source | Present in repository | Source review only |
| v0.1.2 strict compile/package | Historically reported PASS | Prior local report; not rerun |
| v0.1.2 file staging | Historically reported SHA match | Prior local report; not rechecked on device |
| v0.1.2 in-game load/probe | NOT CONFIRMED | No v0.1.2 runtime output recorded |
| v0.1.3 | NOT IMPLEMENTED | Research requirements only |
| Custom Landmark/AssetBundle registration | NOT PROVEN | No verified call graph or IL evidence |
| Custom placement, persistence, multiplayer | NOT IMPLEMENTED / NOT TESTED | Contracts unresolved |

## Current research deliverables
- First Landmark Prototype Specification — design only.
- Landmark API Evidence Matrix — partial evidence transcribed from prior audit notes; not a fresh assembly inspection.
- v0.1.2 Diagnostic Source Review — source-level limitations and proposed v0.1.3 metadata-only scope.
- v0.1.3 Type Discovery and Reflection Safety Specification — design-only contract for candidate identity, ambiguity, exception isolation, member metadata, safety and regression tests.

## Immediate next milestone
[x] Added `01 — Research/v0.1.3 Metadata Probe Implementation Plan.md` with metadata-only scope, safety checklist, acceptance tests M01–M12 and release gates G0–G5. [ ] Review the design and obtain target-build metadata/IL before any C# implementation. Do not build/package v0.1.3 while operating repository-only.

## World-generation implementation gate
No registration adapter, custom numeric Landmark ID, custom ChunkPointType, AssetBundle load, scene instantiation, world mutation, save write, or network message is approved until manager ownership, call order, key format, identity allocation and host authority are established from exact target-build evidence.

---

[Reading 64 lines from start (total: 64 lines, 0 remaining)]



## Current project status reconciliation — 2026-11-06
This status block supersedes earlier pending checklists above where they conflict.
- PHASE 0 compatibility for the tested setup: PASS at diagnostic entry-point/lifecycle level (Raft 1.1.01, Steam build 22312909, RML 2.8.10).
- PHASE 1 diagnostic Core: PASS for Start(), WorldEvent_WorldLoaded() and WorldEvent_WorldSaved(), including a subsequent save callback.
- World Generation: research only. Vanilla chunk/landmark APIs mapped; custom AssetBundle registration, custom point type registration, custom persistence and multiplayer authority are unresolved.
- Read-only world probe source: written, preflight-compiled with /warnaserror and packaged locally. It has not been installed or tested in-game.
- Installed package in Raft/mods remains the previous lifecycle-only build. Do not claim the read-only probe has run until a new package is explicitly staged and tested.
- No Unity Editor/SDK was installed; no game files or saves were modified during static research.


## Runtime World Probe confirmed — 2026-11-06
This block supersedes earlier notes saying the v0.1.1 probe was only staged or not installed.
- [x] RML status changed from NOT LOADED to RUNNING after the user selected the mod and pressed Load Selected Mods.
- [x] v0.1.1 emitted Core/World diagnostic output in the in-game F10 console.
- [x] World probe completed successfully in a loaded world; no mutation warning was emitted.
- [x] Runtime observations: ChunkManager allChunkSpawners=17, chunks=9, chunkPoints dictionary count=3, ChunkSize=1000; SceneLoader loadedLandmarks=38.
- [x] AssetBundleManager and AssetBundleHooker types were found, but no active-scene component instances were found by this probe.
- [ ] Inspect the three ChunkPoint dictionary keys and bounded summaries of their ChunkPoint lists.
- [ ] Inspect the 17 spawner entries and map each to its ChunkPointType / spawn rule without invoking spawn methods.
- [ ] Verify AssetBundleManager/Hooker ownership and lifecycle via type/field/method metadata; absence of active-scene instances is not proof that the bundle pipeline is unavailable.
- [ ] Only after this mapping, design an isolated, disposable-world landmark registration experiment.

**Current phase:** v0.1.1 read-only runtime diagnostics PASS on the observed Raft 1.1.01 / Steam build 22312909 / RML 2.8.10 configuration. Custom generation remains unimplemented.


## Runtime World Probe confirmed — 2026-11-06
- v0.1.1 was activated in RML and reached RUNNING.
- User-provided F10 output confirms the read-only probe completed.
- Runtime observations: ChunkManager allChunkSpawners=17, chunks=9, chunkPoints dictionary entries=3, ChunkSize=1000; SceneLoader loadedLandmarks=38.
- AssetBundleManager and AssetBundleHooker types were found, but no active-scene component instances were found.
- Next: bounded read-only enumeration of the three ChunkPoint dictionary keys and 17 spawner entries. Custom generation remains unimplemented.


## v0.1.2 bounded runtime inspection — 2026-11-06
- [x] Re-read current Master Prompt, Roadmap, Technical Audit, RML API map, World Generation API Audit, Core source, RML source/project, and runtime Player.log before continuing.
- [x] Backed up v0.1.1 Core/RML source and modinfo under `05 — Releases/backup-world-probe-v0.1.1-20261106`.
- [x] Implemented bounded read-only inspection of ChunkPoint dictionary keys/lists, spawner entries, ChunkPoint/ObjectSpawner/SpawnRule field metadata, ChunkPointType enum values, and AssetBundle manager/hooker fields/method signatures.
- [x] Strict Core preflight compile passed with `/warnaserror` against installed Raft/RML assemblies.
- [x] RML package built as v0.1.2 and archive extraction/source-integrity check passed.
- [x] Candidate artifacts saved under `05 — Releases/v0.1.2-world-probe-candidate`.
- [ ] Runtime test v0.1.2 in-game; this is NOT yet done.
- [ ] Only replace the installed package after Raft.exe is closed and the previous installed package is backed up.

**Safety:** this pass enumerates at most 8 dictionary keys, 12 ChunkPoints per key, 20 spawners, 18 fields per object, 24 type fields and 32 method signatures. It reads metadata and values only. It never invokes game methods, changes world state, loads bundles, writes saves or sends network messages.
**Current phase:** v0.1.2 candidate built and preflight-compiled; runtime validation pending.


## v0.1.2 final rebuild correction — 2026-11-06
The final candidate was rebuilt after enabling bounded field inspection for each spawner entry (the first draft enumerated spawner types only). Final strict Core preflight compile passed. Final extracted package reports version 0.1.2 and source integrity matches the project source. Final source SHA-256: `383255F3EA7D814DE9AB07A99D15411B6C6D89F5E89645D162A13D0691065025`. Final package SHA-256: `03283EAAF08126C1B1AF11DCD85A49BCE80A996EA19F9F026E6965CD81CB11C2`. These hashes supersede the earlier first-draft hashes in this document. Runtime test is still pending; installed package remains untouched because Raft.exe is running.


## v0.1.2 installation staging — 2026-11-06
- User confirmed Raft was closed; process inventory contained no `Raft.exe` process.
- Installed mod backup created at `05 — Releases/backup-installed-before-v0.1.2-20261106/RaftUltimateWorldExpansion-installed-before-v0.1.2.rmod`.
- Previous installed package SHA-256: `AD3D2F4870C345F44DC2CF32868A302729A5C321492E25514D95C680C81EA575`.
- Candidate copied to `C:\Program Files (x86)\Steam\steamapps\common\Raft\mods\RaftUltimateWorldExpansion.rmod`.
- Installed package SHA-256 verified as `03283EAAF08126C1B1AF11DCD85A49BCE80A996EA19F9F026E6965CD81CB11C2`, exactly matching v0.1.2 candidate.
- This verifies file staging only. RML loading and the v0.1.2 in-game probe are still pending until the user launches Raft and loads a test world.


## Current state correction — after v0.1.2 staging
The later installation record supersedes older paragraphs in this file that say the installed package is still the lifecycle-only build or that Raft.exe was running during staging. The user closed Raft; the prior installed package was backed up; v0.1.2 was copied into the Raft mods directory and its SHA-256 matched the verified candidate. Current truth: v0.1.2 is staged/installed at file level; no v0.1.2 in-game load has yet been observed. User requested continuation without launching the game, so current work remains static/offline research and code review.

[executed on device: DESKTOP-3MKBODB (8088b63f-2a05-4f71-a7a4-ad740486ff11)]

## Repository-only continuation — 2026-11-06

- [x] Reconciled the current repository state against README, Technical Audit, World Generation API Audit, source and build configuration.
- [x] Added `01 — Research/AssetBundle and Landmark Integration Plan.md` with evidence levels and a static investigation checklist.
- [x] Added the new research plan to the README.
- [x] Reviewed the portable MSBuild properties in the RML project: `RaftDir`, `RaftManagedDir`, `RmlDataDir`, `RmlPublicizedDir`, and `RmlBinariesDir` are defined and used by reference HintPaths. This is source inspection only; no new build was run.
- [ ] Inspect AssetBundleManager/AssetBundleHooker/SceneLoader IL and ownership/call order from repository-accessible metadata. The raw game assemblies are not stored in this repository, so this step requires a metadata dump or disassembly artifact to be added before code-level conclusions can be made.
- [ ] Validate the v0.1.2 read-only probe in-game only when the user explicitly chooses to launch Raft.

**Current operating constraint:** repository-only work. Do not access the local Windows project, compile, install packages, or launch Raft unless the user changes this instruction.

**Current technical decision:** custom Landmark/AssetBundle registration remains unproven. Do not implement a guessed registration call or custom ChunkPointType.


## First Landmark prototype design — repository-only update (2026-11-06)

- [x] Defined a minimal original test Landmark and provisional mod-owned content keys in `01 — Research/First Landmark Prototype Specification.md`.
- [x] Defined explicit non-goals: no custom ChunkPointType, no vanilla spawn-rule mutation, no production gameplay systems, no valuable-save testing.
- [x] Defined lifecycle proof obligations and acceptance gates P0–P6: static contract, offline asset package, registration, placement, cleanup, persistence and multiplayer.
- [x] Linked the specification from README and Architecture.
- [ ] Build the API evidence matrix from actual metadata/IL for AssetBundleManager, AssetBundleHooker, SceneLoader, Landmark and LandmarkData. Required raw assemblies/IL are not in this repository.
- [ ] Do not implement registration or placement adapters until the relevant call order, ownership, ID rules and authority are proven.

**Status correction:** v0.1.1 diagnostic runtime evidence is confirmed. v0.1.2 build/package and file staging are historical reports; v0.1.2 in-game load/probe remains unconfirmed. This work session made no local verification, compilation, installation, or game launch.


## Landmark API evidence matrix — repository-only update (2026-11-06)

- [x] Consolidated recorded API observations into `01 — Research/Landmark API Evidence Matrix.md`.
- [x] Separated exact runtime observations from member-name-only/static audit notes.
- [x] Listed missing proof for manager ownership, AssetBundle call graph, registration semantics, Landmark ID allocation, scene/bundle key conventions, persistence and network authority.
- [x] Added a capture template for exact signatures, assembly hashes, call sites and relevant IL behavior.
- [ ] Obtain text-only metadata/IL evidence for the target build; raw game assemblies are not in this repository.
- [ ] Do not implement registration/placement code until the missing contracts are proven.

This update is documentation-only. It does not independently verify local DLLs or the v0.1.2 runtime candidate.


## v0.1.2 source review — repository-only update (2026-11-06)

- [x] Reviewed the checked-in Core/RML diagnostic source without executing it.
- [x] Documented coverage boundaries: short-name type discovery, first valid-scene component selection, declared-member caps, abbreviated signatures, omitted LandmarkData/AsyncedLandmark types, and lack of IL/call-site analysis.
- [x] Defined a proposed v0.1.3 metadata-only scope, explicitly excluding all game mutations and AssetBundle/scene registration calls.
- [ ] Do not implement or package v0.1.3 while operating repository-only.
- [ ] Obtain exact target-build metadata/IL to resolve manager ownership, registration semantics, stable ID allocation, persistence and network authority.

The source review is not a runtime test and does not change the v0.1.2 status.


## v0.1.3 metadata probe plan — repository-only update (2026-11-06)

- [x] Added the design-only v0.1.3 Metadata Probe Implementation Plan.
- [x] Defined target type allowlist, full type/member identity requirements, deterministic bounded output, truncation reporting and exception isolation.
- [x] Added explicit safety prohibitions and acceptance tests M01–M12.
- [x] Added release gates G0–G5 and clarified that metadata does not replace IL/call-site analysis.
- [ ] Review the design; obtain exact target-build metadata/IL before source implementation.
- [ ] Keep C# implementation, build, packaging and runtime validation deferred under repository-only mode.

No code or package was changed, and no local machine or game process was accessed.


## v0.1.2 diagnostic boundedness audit — repository-only update (2026-11-06)

- [x] Reviewed the checked-in v0.1.2 RML source for allocation, reflection, enumeration and output limits.
- [x] Documented that several output caps occur after APIs have already materialized full arrays.
- [x] Recorded missing global output budget, duplicate-type first-match behavior, runtime-value output and early completion-latch risks.
- [x] Added required v0.1.3 design corrections and regression cases B-T01–B-T10 in `01 — Research/v0.1.2 Diagnostic Boundedness Audit.md`.
- [ ] Carry these requirements into v0.1.3 acceptance before source implementation.

This is a static source review only. No code, package, local installation, game process or save was changed.


## v0.1.3 type discovery and reflection safety — repository-only update (2026-11-06)

- [x] Added the Type Discovery and Reflection Safety Specification for all nine required target types.
- [x] Defined loaded-assembly-only scanning, deterministic identities, duplicate/ambiguity semantics, and `ReflectionTypeLoadException` partial recovery.
- [x] Defined metadata-only member inventory, side-effect prohibitions, bounded errors/output, COMPLETE/PARTIAL/FAILED semantics, and acceptance tests TD01–TD20.
- [x] Linked the specification from README.
- [ ] Review/approve G0 and obtain exact target-build metadata/IL before any C# implementation.
- [ ] Keep v0.1.3 implementation, compilation, packaging and runtime testing deferred in repository-only mode.

Documentation-only update. No local machine, compiler, game process, save or runtime state was accessed or changed.


## v0.1.3 G0 cross-review — repository-only update (2026-11-06)

- [x] Cross-reviewed the plan/specification against the boundedness audit, resource budgets and C# 7.3/.NET Framework 4.8 constraints.
- [x] Clarified candidate counting/output cap semantics, accessor deduplication/member accounting and enum constant accounting.
- [x] Added regression cases TD21–TD25 to the reflection safety specification.
- [ ] G0 formal approval remains pending; exact target-build metadata/IL is unavailable and fixture tests have not been executed.
- [ ] Keep implementation, compilation, packaging and runtime testing deferred under repository-only mode.

The review is documentation-only. No local machine, compiler, game process, save or runtime state was accessed or changed.


## v0.1.3 G0.2 output schema — repository-only update (2026-11-06)

- [x] Added the normative UWE-META/1 output schema for all twelve record families.
- [x] Defined UTF-8/LF framing, field order, escaping, sentinels, IDs, deterministic record ordering and parser validation.
- [x] Added OS01–OS15 acceptance tests and explicit terminal status/count validation.
- [ ] Review five open schema decisions and obtain exact target-build metadata/IL before G0 approval.
- [ ] Keep writer/parser implementation, compilation, packaging and runtime testing deferred in repository-only mode.

Documentation-only. No local machine, compiler, game process, save or runtime state was accessed or changed.
