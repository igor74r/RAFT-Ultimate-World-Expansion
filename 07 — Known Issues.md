[Reading 48 lines from start (total: 48 lines, 0 remaining)]



## Current issue status reconciliation — 2026-11-06
This section supersedes stale statuses in the original table above.
- ISSUE-001: Partially resolved — launcher UI reports RML 2.8.10; only the tested configuration is confirmed.
- ISSUE-002: Open — legacy RML tutorial Unity editor version differs from installed Raft runtime; asset workflow not confirmed.
- ISSUE-003: Resolved for tested configuration only — lifecycle-only mod loaded on Raft build 22312909 / RML 2.8.10.
- ISSUE-004: Open — separate Raft_Ultimate_Survival prototype remains out of scope and unverified.
- ISSUE-005: Resolved — official Mod Creator project exists.
- ISSUE-006: Partially resolved — direct source compilation works with framework csc.exe; no .NET SDK/MSBuild project build established.
- ISSUE-007: Resolved — Launcher/Mod Creator available.
- ISSUE-008: Open — item/recipe/network API details remain unverified.
- ISSUE-009: Open — generated csproj reference paths remain unvalidated.
- ISSUE-010: Partially resolved — metadata accepted by RML; author remains blank for release.
- ISSUE-011: Resolved — diagnostic Core source integrated and loaded.
- ISSUE-012: Risk documented — build.bat is packaging-only and deletes same-name package; always preserve/verify old artifact first.
- ISSUE-013: Resolved for diagnostic lifecycle smoke test.
- ISSUE-014 through ISSUE-017: Open research blockers for custom AssetBundle registration, custom point types, persistence/network authority and Unity asset workflow.
- Updated read-only World probe package is built locally but not installed/runtime-tested; do not close this test item yet.


## Runtime probe reconciliation — 2026-11-06
- Supersedes earlier note that v0.1.1 was not runtime-tested: user-provided F10 output confirms the probe completed.
- Custom AssetBundle registration remains OPEN; type discovery is not proof of registration support.
- Custom ChunkPointType registration remains OPEN; vanilla enum appears closed in static audit.
- Persistence and multiplayer authority remain OPEN.
- Count caveats: dictionary count=3 means keys, not total points; spawner count=17 is list length; loadedLandmarks=38 is metadata, not spawned objects.


## v0.1.2 candidate limitations — 2026-11-06
- v0.1.2 has not yet been loaded in-game; do not treat its expected output as runtime evidence.
- Reflection enumeration depends on the runtime collections implementing non-generic IDictionary/IEnumerable; unsupported implementations will log a bounded diagnostic failure without mutation.
- Field inspection reports non-scalar object references by type only; it does not traverse arbitrary graphs.
- The probe does not invoke methods, so it cannot prove the actual behavior of AssetBundle registration or landmark initialization.
- The current installed package was not overwritten because Raft.exe was still present in the process list.


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