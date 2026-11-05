[Reading 53 lines from start (total: 53 lines, 0 remaining)]



### Read-only World probe source and package — 2026-11-06
- Created documentation/project backup at 05 — Releases/backup-docs-20261106-worldgen-audit and backup-rml-project-20261106-pre-worldprobe before edits.
- Added a read-only world-manager inspection path to the current project source (manager discovery and field/count inspection only; no world mutation).
- Preflight compile passed with framework csc.exe /warnaserror and local AssemblyLoader, UnityEngine.CoreModule and Assembly-CSharp references; output 6,144 bytes.
- Rebuilt local source-based .rmod. Size 31,937 bytes; SHA-256 AD3D2F4870C345F44DC2CF32868A302729A5C321492E25514D95C680C81EA575.
- Verified ZIP root entries and confirmed packaged source matches current project source.
- The updated package was NOT copied to Raft/mods and has NOT been tested in-game. Existing installed lifecycle-only package remains untouched.


## v0.1.1 runtime World probe confirmed — 2026-11-06
### Confirmed
- User activated the mod in RML; Mod Manager status changed from NOT LOADED to RUNNING.
- In-game F10 output confirmed World probe completion.
- Runtime counts: ChunkManager allChunkSpawners=17, chunks=9, chunkPoints dictionary count=3, ChunkSize=1000; SceneLoader.loadedLandmarks=38.
- AssetBundleManager and AssetBundleHooker types were found, with zero active-scene component instances reported.
- Probe completion explicitly confirmed that no chunk points, landmarks, bundles or save data were changed.

### Next
- Add a bounded, read-only second-level inspection of ChunkPoint dictionary keys and spawner entries.
- Investigate AssetBundle manager ownership and registration API without loading or mutating bundles.


## v0.1.1 runtime World probe confirmed — 2026-11-06
- User activated the mod in RML; status changed from NOT LOADED to RUNNING.
- F10 output confirmed successful read-only probe completion.
- Observed counts: 17 spawners, 9 chunks, 3 ChunkPoint dictionary keys, ChunkSize 1000, and 38 loaded Landmark metadata entries.
- AssetBundleManager and AssetBundleHooker types were found; no active-scene component instances were reported.
- Next: bounded read-only inspection of ChunkPoint keys/spawners and AssetBundle manager ownership.


## v0.1.2 — bounded world diagnostics candidate — 2026-11-06
- Added bounded enumeration of ChunkManager.chunkPoints dictionary keys and contained ChunkPoint records.
- Added bounded enumeration and field inspection of allChunkSpawners.
- Added metadata-only field/method surface inspection for ChunkPoint, ObjectSpawner_Chunk, SO_ChunkSpawnRuleAsset, AssetBundleManager, AssetBundleHooker and ChunkPointType.
- Added hard output caps to avoid unbounded log spam.
- Preserved read-only behavior and single-entry-point architecture.
- Core preflight compile and RML package integrity validation passed.
- In-game validation remains pending; installed package was intentionally not replaced while Raft.exe was running.


## v0.1.2 final rebuild correction — 2026-11-06
The final candidate was rebuilt after enabling bounded field inspection for each spawner entry (the first draft enumerated spawner types only). Final strict Core preflight compile passed. Final extracted package reports version 0.1.2 and source integrity matches the project source. Final source SHA-256: `383255F3EA7D814DE9AB07A99D15411B6C6D89F5E89645D162A13D0691065025`. Final package SHA-256: `03283EAAF08126C1B1AF11DCD85A49BCE80A996EA19F9F026E6965CD81CB11C2`. These hashes supersede the earlier first-draft hashes in this document. Runtime test is still pending; installed package remains untouched because Raft.exe is running.


## v0.1.2 installation staging — 2026-11-06
- User confirmed Raft was closed; process inventory contained no `Raft.exe` process.
- Installed mod backup created at `05 — Releases/backup-installed-before-v0.1.2-20261106/RaftUltimateWorldExpansion-installed-before-v0.1.2.rmod`.
- Previous installed package SHA-256: `AD3D2F4870C345F44DC2CF32868A302729A5C321492E25514D95C680C81EA575`.
- Candidate copied to `C:\Program Files (x86)\Steam\steamapps\common\Raft\mods\RaftUltimateWorldExpansion.rmod`.
- Installed package SHA-256 verified as `03283EAAF08126C1B1AF11DCD85A49BCE80A996EA19F9F026E6965CD81CB11C2`, exactly matching v0.1.2 candidate.
- This verifies file staging only. RML loading and the v0.1.2 in-game probe are still pending until the user launches Raft and loads a test world.

[executed on device: DESKTOP-3MKBODB (8088b63f-2a05-4f71-a7a4-ad740486ff11)]

## Repository-only research continuation — 2026-11-06

- Added `01 — Research/AssetBundle and Landmark Integration Plan.md`.
- Documented the known static API surface for SceneLoader, Landmark, AssetBundleManager, AssetBundleHooker and `GetEmbeddedFileBytes(String)`.
- Added evidence gates for metadata/IL, offline asset validation, controlled runtime, save/reload and multiplayer.
- Linked the new plan from README.
- Reviewed portable MSBuild property definitions and HintPath usage in the repository; no compilation was performed.
- No game files, local project files, saves, or runtime state were accessed or changed.


## Evidence/status reconciliation — 2026-11-06

- Corrected Technical Audit and Modules status descriptions to distinguish v0.1.1 runtime evidence from v0.1.2 historical build/staging evidence.
- Added an explicit repository-only evidence boundary: no fresh local verification, compilation, or game launch occurred.
- Clarified that custom Landmark/AssetBundle registration, custom chunk types, persistence and multiplayer remain unproven.


## First Landmark prototype specification — 2026-11-06

- Added `01 — Research/First Landmark Prototype Specification.md`.
- Defined the first isolated Landmark's provisional identity, minimal original scene scope, logical responsibilities, lifecycle proof obligations, safety rules and P0–P6 acceptance gates.
- Linked the specification from README and Architecture.
- No C# implementation, asset bundle, game file, save, or runtime state was changed.


## Landmark API evidence matrix — 2026-11-06

- Added `01 — Research/Landmark API Evidence Matrix.md` with recorded member observations, runtime/static evidence boundaries, missing proof and an IL capture template.
- Linked the matrix from README, Roadmap and Architecture.
- No game API calls, C# implementation, local build, game launch, or save modification occurred.


## v0.1.2 diagnostic source review — 2026-11-06

- Added `01 — Research/v0.1.2 Diagnostic Source Review.md`.
- Audited the checked-in diagnostic source for coverage limits and defined a safe metadata-only v0.1.3 research scope.
- No code, package version, local build, game process, save or runtime state was changed.


## Repository status and README correction — 2026-11-06

- Added an authoritative status block to Roadmap to supersede conflicting historical checklist entries.
- Corrected README's current status to distinguish v0.1.1 runtime confirmation from v0.1.2 historical build/staging reports and unconfirmed in-game validation.
- Repaired README research links and added the missing prototype specification link.
- Added the same evidence boundary to Technical Audit.
- No code, package, local installation, game process, or save was changed.


## v0.1.3 metadata probe design — 2026-11-06

- Added 01 — Research/v0.1.3 Metadata Probe Implementation Plan.md.
- Specified metadata-only type/member inventory, deterministic output, bounded limits, duplicate handling, truncation and exception isolation.
- Added source safety checklist, acceptance tests M01–M12, and release gates G0–G5.
- Linked the plan from README, Roadmap and Landmark API Evidence Matrix.
- Design only: no C# source, package, local installation, game process, save or runtime state was changed.


## v0.1.2 diagnostic boundedness audit — 2026-11-06

- Added `01 — Research/v0.1.2 Diagnostic Boundedness Audit.md` after static review of the checked-in RML source.
- Identified full-array materialization before output caps, missing global output budget, first-match type lookup, runtime-value output and early completion-latch risks.
- Recorded v0.1.3 design corrections and regression cases B-T01–B-T10.
- No C# source, package, local installation, game process, or save was changed.


## v0.1.3 resource budget refinement — 2026-11-06

- Refined the metadata-only probe plan with provisional global/per-type budgets, a versioned line-oriented output contract, and explicit Reflection allocation/time-bound caveats.
- Added regression requirements RB01–RB15 for global caps, escaping, deterministic output, error handling and honest completion status.
- Design only: no C# source, package, local installation, game process, save or runtime state was changed.


## v0.1.3 build-compatibility constraints — 2026-11-06

- Recorded repository project settings: legacy MSBuild ToolsVersion 15.0, .NET Framework target v4.8, C# language version 7.3, library output and deterministic compilation.
- Clarified that project target framework does not prove the game's actual runtime or compiler version.
- Added source-compatibility guardrails for the planned metadata probe; no project format, dependency, C# source or package was changed.
