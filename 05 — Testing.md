
## Read-only World probe build — 2026-11-06
| Check | Result | Evidence |
|---|---|---|
| Source preflight compile | PASS | Framework csc.exe with /warnaserror and references to AssemblyLoader, UnityEngine.CoreModule and Assembly-CSharp; output DLL 6,144 bytes. |
| Source package build | PASS | RML build.bat created a 31,937-byte .rmod. |
| ZIP structure | PASS | Four expected root entries: banner.jpg, icon.png, modinfo.json, RaftUltimateWorldExpansion.cs. |
| Packaged source integrity | PASS | Extracted source text matches current project source; SHA-256 package AD3D2F4870C345F44DC2CF32868A302729A5C321492E25514D95C680C81EA575. |
| In-game execution of updated probe | NOT RUN | New package has not been installed; currently installed package remains lifecycle-only. |

The probe is designed to read manager presence and selected fields only. It must not be described as a runtime success until the updated package is loaded and its F10 output is inspected.


## v0.1.1 in-game World probe — 2026-11-06

| Check | Result | Evidence |
|---|---|---|
| RML mod activation | PASS | Mod Manager changed from NOT LOADED to RUNNING after Load Selected Mods |
| Core initialization | PASS | F10 output from v0.1.1 diagnostic session |
| WorldLoaded callback | PASS | World probe field/type output followed by completion line |
| ChunkManager discovery | PASS | 17 spawners; 9 chunks; 3 ChunkPoint dictionary keys; ChunkSize=1000 |
| SceneLoader discovery | PASS | loadedLandmarks count=38 |
| AssetBundle manager types | PARTIAL | Both types found; no active-scene component instances found |
| Probe completion | PASS | Completion line states no chunk points, landmarks, bundles, or save data were changed |
| Custom world generation | NOT TESTED | No custom points, landmarks, bundles, persistence or network behavior was exercised |

Evidence source: user-provided in-game F10 screenshot. Runtime observations apply to the tested configuration only: Raft 1.1.01 / Steam build 22312909 / RML 2.8.10.


## v0.1.1 in-game World probe — 2026-11-06
| Check | Result | Evidence |
|---|---|---|
| RML activation | PASS | Status changed from NOT LOADED to RUNNING |
| Core initialization and WorldLoaded | PASS | F10 probe output |
| ChunkManager | PASS | 17 spawners; 9 chunks; 3 ChunkPoint dictionary keys; ChunkSize=1000 |
| SceneLoader | PASS | loadedLandmarks count=38 |
| AssetBundle types | PARTIAL | Both types found; no active-scene component instances |
| Probe completion | PASS | Explicit completion line states no chunk points, landmarks, bundles or save data were changed |
| Custom world generation | NOT TESTED | No custom point, landmark, bundle, persistence or network test |

Evidence: user-provided F10 screenshot. Tested setup: Raft 1.1.01 / Steam build 22312909 / RML 2.8.10.


## v0.1.2 build validation — 2026-11-06
| Check | Result | Evidence |
|---|---|---|
| v0.1.1 source backup | PASS | Core, RML source and modinfo copied to versioned backup |
| Core compile | PASS | build-core.cmd; `/warnaserror`; produced Core DLL |
| RML package build | PASS | build.bat produced `.rmod` |
| Package extraction | PASS | Copied `.rmod` to `.zip`, extracted 4 root files |
| Metadata version | PASS | Extracted modinfo.json reports 0.1.2 |
| Packaged source integrity | PASS | SHA-256 matches project C# source |
| In-game v0.1.2 probe | NOT RUN | Raft.exe remained running; installed mod file was not overwritten |

Candidate package SHA-256: `A3EC96A043CC1F8DFB73C7BD02742EBE7522955A5005C636F9BB1AA52996577A`.


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

## Repository-only evidence reconciliation — 2026-11-06

This section supersedes earlier v0.1.2 test rows that say the candidate was not installed or that Raft.exe prevented staging.

- v0.1.1 read-only World probe: **PASS** for runtime execution on the tested configuration, based on the user's F10 screenshot.
- v0.1.2 candidate build/package validation: **PASS historically**, as recorded in the prior local build and integrity reports.
- v0.1.2 file staging: **reported PASS historically**; the prior package was backed up and the candidate hash was reported to match the staged file.
- v0.1.2 in-game load and F10 probe: **NOT RUN / NOT CONFIRMED**.
- Current repository-only continuation: no local files, process state, game installation, or package hashes were rechecked; no compilation or game launch was performed.

Do not interpret historical file-staging evidence as a successful RML load or runtime probe.


## v0.1.3 design-review regression coverage — 2026-11-06

The following cases were added during the G0 cross-review. They are acceptance requirements only; no fixture harness or tests have been executed.

| ID | Scenario | Expected result |
|---|---|---|
| TD21 | More than 8 matching candidates across assemblies | All candidates examined/counted where possible; at most 8 deterministic identities emitted; TRUNCATED/PARTIAL; no resolution |
| TD22 | Property/event accessors also appear in declared method enumeration | Accessors embedded only in PROPERTY/EVENT records; no duplicate METHOD records; property/event counted once |
| TD23 | Enum constants are also represented by literal fields | Constants emitted only in ENUM records and charged only to enum budget |
| TD24 | Assembly inspection cap prevents complete candidate inventory | Observed count separated from total UNKNOWN; PARTIAL; no false NOT_FOUND/unique resolution |
| TD25 | Stable candidate identity tie-breaker unavailable | Bounded fallback identity and ordering uncertainty/PARTIAL; no discovery-order selection |

G0 disposition: REVIEWED — APPROVAL PENDING. TD01–TD25 remain unexecuted design tests. No source implementation or runtime test is claimed.


## v0.1.3 UWE-META/1 schema acceptance coverage — 2026-11-06

The following are design requirements only; no parser, writer or fixture suite has been implemented or executed.

| ID | Scenario | Expected result |
|---|---|---|
| OS01 | Valid stream with every record kind | Parses and validates |
| OS02 | Missing/extra field | Reject record without positional drift |
| OS03 | Pipe, backslash, tab, CR and LF in text | Exact round-trip after escaping |
| OS04 | Literal `-` and `UNKNOWN` | Distinguished from sentinels |
| OS05 | Invalid escape | Reject malformed record |
| OS06 | Missing, duplicate or non-final SUMMARY | Stream incomplete/FAILED |
| OS07 | Unknown record kind under v1 | Reject; do not guess schema |
| OS08 | Counts disagree with records | Report integrity mismatch |
| OS09 | Escaped output exceeds physical ceiling | TRUNCATED/PARTIAL or FAILED if SUMMARY unavailable |
| OS10 | Non-invariant current culture | Numeric fields remain invariant |
| OS11 | Same fixture scanned twice | Byte-equivalent output except explicitly variable evidence |
| OS12 | Hostile/multiline exception message | Never serialized |
| OS13 | Unknown protocol version | Unsupported version; no guessed parsing |
| OS14 | Writer fails before SUMMARY | FAILED; never false COMPLETE |
| OS15 | UTF-8 BOM or invalid byte sequence | Enforce BOM policy; reject invalid UTF-8 |

Schema source: [v0.1.3 UWE-META/1 Output Schema](v0.1.3%20UWE-META-1%20Output%20Schema.md). G0 remains REVIEWED — APPROVAL PENDING. No test execution is claimed.


## World catalog startup integration — 2026-11-06

| Check | Result | Evidence |
|---|---|---|
| Catalogs mirrored into RML project | PASS (repository check) | Three nested JSON files match the canonical Content/World catalogs |
| Project content entries | PASS (static check) | .csproj includes all three paths with CopyToOutputDirectory=PreserveNewest |
| Embedded loader adapter | PASS (source check) | WorldCatalog.LoadFromEmbeddedFiles accepts a byte-reader delegate and validates all catalogs |
| Startup invocation | IMPLEMENTED; runtime pending | Mod Start calls LoadFromEmbeddedFiles(GetEmbeddedFileBytes) and logs bounded errors |
| Embedded nested-path resolution | NOT CONFIRMED | Requires actual RML runtime test |
| C# compilation/package | NOT RUN | Repository-only workflow |
| In-game catalog log | NOT RUN | No new package loaded in Raft |

The startup path is read-only and does not invoke world mutation, save, or network APIs.


## Deterministic biome resolver — 2026-11-06

| Check | Result | Evidence |
|---|---|---|
| Resolver source created | PASS (repository check) | WorldBiomeResolver.cs added as a Unity/Raft-independent content resolver |
| Project compile inclusion | PASS (static check) | WorldBiomeResolver.cs included in the RML .csproj |
| Startup diagnostic hook | PASS (source check) | Validated catalogs are resolved once per biome with deterministic seed 0 |
| Tropical mapping | PASS (JSON fixture check) | 2 island candidates; 4 biome resources; seed 0 selects mangrove-cove |
| Arctic mapping | PASS (JSON fixture check) | 2 island candidates; 4 biome resources; seed 0 selects ice-cave |
| Volcanic mapping | PASS (JSON fixture check) | 2 island candidates; 4 biome resources; seed 0 selects caldera-island |
| Deep-ocean mapping | PASS (JSON fixture check) | 2 island candidates; 4 biome resources; seed 0 selects sunken-laboratory |
| C# compile / in-game run | NOT RUN | Repository-only workflow; resolver has not been compiled or executed in Raft |

Resolver scope: select a mod-owned biome, intersect its landmark pool with island definitions for that biome, collect resources associated with that biome, and select one island deterministically from biome ID + seed. This is a content planning result only; it does not place islands, alter ChunkManager, or claim to use the actual Raft world seed.


## Repository CI and data-only world-generation self-tests — 2026-11-09

- Workflow: `.github/workflows/world-generation-self-tests.yml`.
- Triggers: push to `main`, pull request to `main`, and manual dispatch.
- Runner/toolchain: `ubuntu-latest`, .NET SDK 8.0.x.
- Command: `dotnet run --project tests/WorldGeneration.SelfTests/WorldGeneration.SelfTests.csproj --configuration Release`.
- The self-test project compiles the Unity/Raft-independent catalog, biome resolver, region planner/validator, region resolver, and spawn planner/validator source files.
- Covered scenarios include catalog loading/validation, deterministic region generation for a fixed seed, deterministic island/resource selection and coordinates, structural plan validation, seed mismatch rejection, and the 256-island input ceiling.

**Scope limitation:** these tests exercise content-only planning code. They do not prove RML package loading, Unity compatibility, AssetBundle loading, actual island placement, save/reload, or multiplayer authority.

## v0.1.3 output-budget design acceptance — 2026-11-09

- BR11: SUMMARY self-accounting fixed point, including the SUMMARY record itself and terminal TRUNCATED when present; parser and writer must agree at decimal digit boundaries.
- BR12: more than 32 ERROR descriptors under permuted discovery order produce the same deterministic retained top-32 set and post-sort sequence.
- OS16: `SUMMARY.characters_emitted` includes its own final escaped record payload and matches independent parser recomputation.
- OS17: ERROR sequence values are assigned after deterministic sorting; shuffled discovery order does not change the serialized diagnostic set.
- M13–M14: corresponding writer/accounting and deterministic-retention implementation requirements.

These are design acceptance cases only. No UWE-META/1 writer, parser, fixture harness, or v0.1.3 runtime probe has been implemented or executed.
