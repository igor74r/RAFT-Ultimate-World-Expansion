[Reading 49 lines from start (total: 49 lines, 0 remaining)]



## Current verified status — 2026-11-06
- Diagnostic Core v0.1.0 was loaded in Raft 1.1.01 / Steam build 22312909 with RML 2.8.10.
- Start(), WorldEvent_WorldLoaded() and WorldEvent_WorldSaved() were confirmed in-game; the save callback also fired on a subsequent save.
- Current development focus: read-only World Generation API probe. Static API audit is in [[01 — Research/World Generation API Audit]].
- The updated read-only probe source compiles and is packaged locally, but this new package has not been installed or runtime-tested. The currently installed game package remains the earlier lifecycle-only build.


## Latest verified state — 2026-11-06
- v0.1.1 is loaded and its read-only World probe has completed in-game on the tested Raft 1.1.01 / Steam build 22312909 / RML 2.8.10 setup.
- Runtime observations: ChunkManager has 17 spawner entries, 9 chunks, 3 ChunkPointType dictionary keys and ChunkSize 1000; SceneLoader has 38 loadedLandmarks metadata entries.
- AssetBundleManager and AssetBundleHooker types were found, but no active-scene component instances were found by the current probe.
- The probe did not modify chunk points, landmarks, bundles or save data.
- Next development task: bounded read-only enumeration of ChunkPoint dictionary keys and spawner entries, followed by AssetBundle ownership/registration research. Custom world generation is still not implemented.


## Latest verified state — 2026-11-06
- v0.1.1 read-only World probe completed in-game on Raft 1.1.01 / Steam build 22312909 / RML 2.8.10.
- Observed: 17 ChunkManager spawner entries, 9 chunks, 3 ChunkPointType dictionary keys, ChunkSize 1000, and 38 SceneLoader loadedLandmarks metadata entries.
- AssetBundleManager and AssetBundleHooker types were found, but no active-scene component instances were found.
- Probe did not modify chunk points, landmarks, bundles, or save data.
- Next: bounded read-only enumeration of ChunkPoint keys and spawner entries, then AssetBundle ownership/registration research. Custom generation remains unimplemented.


## Latest development state — v0.1.2 candidate (2026-11-06)
- v0.1.1 runtime probe confirmed successfully in-game.
- v0.1.2 adds bounded read-only enumeration of ChunkPoint dictionary keys/lists and chunk spawners, plus metadata-only API surface inspection for ChunkPoint, spawners, spawn rules and AssetBundle managers.
- Core strict preflight compile passed; RML `.rmod` package built and verified (version 0.1.2, packaged source matches project source).
- Candidate files: `05 — Releases/v0.1.2-world-probe-candidate`.
- v0.1.2 is NOT yet installed or runtime-tested. Raft.exe was still running, so the current installed mod was left untouched.
- Next: close Raft fully, back up the installed `.rmod`, stage v0.1.2, load a disposable/test world, and capture F10 output.


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