[Reading 60 lines from start (total: 60 lines, 0 remaining)]



## Current verified project state — 2026-11-06 reconciliation
The diagnostic source-based mod was loaded by RML 2.8.10 on Raft 1.1.01 / Steam build 22312909. User screenshots confirmed the mod's Start log, World loaded log, and World save event log on a subsequent save. This verifies only the diagnostic lifecycle callbacks for this configuration.

A separate read-only World probe source was then prepared, preflight-compiled successfully with /warnaserror against AssemblyLoader, UnityEngine.CoreModule and Assembly-CSharp, and packaged into a local .rmod. Package SHA-256: AD3D2F4870C345F44DC2CF32868A302729A5C321492E25514D95C680C81EA575; size 31,937 bytes. The archive contains banner.jpg, icon.png, modinfo.json and RaftUltimateWorldExpansion.cs; packaged source matches the current project source. This updated package has NOT been installed or runtime-tested.

The current installed package in Raft/mods is still the earlier lifecycle-only build. The new probe only reads existing managers/fields and must be runtime-validated before it is used as evidence about live chunk/landmark counts.


## Runtime confirmation addendum — 2026-11-06
The in-game F10 screenshot from the user confirms that the v0.1.1 read-only World probe ran to completion.

Observed values:
- ChunkManager.allChunkSpawners: count 17 (List<ObjectSpawner_Chunk>)
- ChunkManager.chunks: count 9 (Chunk[])
- ChunkManager.chunkPoints: count 3 (Dictionary<ChunkPointType, List<ChunkPoint>>)
- ChunkManager.ChunkSize: 1000
- SceneLoader.loadedLandmarks: count 38 (List<SceneLoader+LandmarkData>)
- AssetBundleManager and AssetBundleHooker types were found, but Resources-based scene lookup returned 0 loaded objects / 0 scene instances for each.

Interpretation limits:
- chunkPoints count=3 is the dictionary entry count, not the total number of ChunkPoint instances across its lists.
- allChunkSpawners count=17 is the list length, not proof that all 17 are active or used in the current world.
- loadedLandmarks count=38 confirms loaded landmark metadata entries, not 38 spawned landmarks in the current world.
- Zero active-scene component instances for the two AssetBundle types does not prove they are unused; they may be static managers, non-Component classes, inactive/persistent objects, or accessed through another owner.
- No custom ChunkPointType, AssetBundle registration, save migration, or multiplayer authority was tested.


## Runtime confirmation — 2026-11-06
User-provided F10 output confirms v0.1.1 probe completion. Observed: ChunkManager.allChunkSpawners=17 (List<ObjectSpawner_Chunk>), chunks=9 (Chunk[]), chunkPoints=3 (Dictionary<ChunkPointType,List<ChunkPoint>> keys), ChunkSize=1000; SceneLoader.loadedLandmarks=38 (metadata entries). AssetBundleManager and AssetBundleHooker types were found, but the probe reported 0 loaded objects and 0 active-scene instances for each.

Interpretation limits: dictionary count 3 is key count, not total ChunkPoint count; spawner count 17 is list length, not proof all are active; loadedLandmarks 38 is metadata count, not spawned Landmark count. No custom type registration, bundle registration, persistence, or multiplayer behavior was tested.


## v0.1.2 diagnostic extension — 2026-11-06
The second diagnostic pass extends v0.1.1 without changing the single RML entry point. It reads the ChunkManager.chunkPoints dictionary via IDictionary, enumerates bounded point lists, enumerates allChunkSpawners with a hard cap, and inspects bounded instance fields using reflection. It also records metadata-only field and method signatures for ChunkPoint, ObjectSpawner_Chunk, SO_ChunkSpawnRuleAsset, AssetBundleManager, AssetBundleHooker, and enum values for ChunkPointType.

Safety limits: max 8 dictionary keys; max 12 points per key; max 20 spawners; max 18 fields per object; max 24 type fields and 32 declared method signatures. No properties/getters or game methods are invoked. No spawn, bundle load, save mutation, or network method is called.

Build evidence: Core preflight compile passed with `/warnaserror` against the installed Raft/RML references. RML `.rmod` built, extracted successfully after copying to `.zip` for verification, package metadata reports version 0.1.2, and packaged C# source SHA-256 matches the project source (`3784A29AF9DD8B375045822933C82E9F58B283133F5652EF2A028244D4FBBF52`). Package SHA-256: `A3EC96A043CC1F8DFB73C7BD02742EBE7522955A5005C636F9BB1AA52996577A`.

Runtime status: NOT TESTED. Raft.exe was still running at the time of packaging, so the installed `Raft\mods\RaftUltimateWorldExpansion.rmod` was deliberately not replaced. Candidate is stored in `05 — Releases/v0.1.2-world-probe-candidate`.


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

## Repository-only status reconciliation — 2026-11-06

This section supersedes contradictory earlier status paragraphs in this file:

- **v0.1.1:** the first read-only World probe was runtime-confirmed by the user's F10 screenshot. Its observed counts are documented above and are limited to the tested Raft 1.1.01 / Steam build 22312909 / RML 2.8.10 setup.
- **v0.1.2:** the final candidate source/package was built and hash-verified on the user's Windows machine, and the candidate was staged into the local Raft mods directory after backing up the prior package. This repository-only session did not inspect that machine, re-verify the installed file, compile, or launch Raft. Therefore the repository records the prior staging report as historical evidence, but **v0.1.2 runtime validation remains pending**.
- The repository contains the selected source and documentation, not the local game assemblies, full Unity project, generated metadata dump, or packaged release artifacts. Static IL/ownership analysis cannot be completed from this repository alone.
- The portable MSBuild path changes were reviewed as text in the committed project file. They have not been compiled on a second machine in this workflow.
- Custom AssetBundle registration, custom Landmark registration, custom ChunkPointType support, persistence, and multiplayer remain unproven.

**Operating boundary:** current work is GitHub-repository-only. Do not claim fresh local build, installation, package hash verification, or runtime testing unless new evidence is supplied.
