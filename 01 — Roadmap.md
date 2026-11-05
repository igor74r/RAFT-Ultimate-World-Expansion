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
