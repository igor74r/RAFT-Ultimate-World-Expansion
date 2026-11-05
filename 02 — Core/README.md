[Reading 24 lines from start (total: 24 lines, 0 remaining)]

# Core — read-only World diagnostics

## Files
- RaftUltimateWorldExpansion.cs — RML entry point, lifecycle logging and read-only discovery of vanilla world managers.
- build-core.cmd — legacy .NET Framework compiler preflight for the minimal Core source.
- artifacts/RaftUltimateWorldExpansion.Core.dll — earlier compile artifact; not a packaged mod.
- RML-Project-Seed/modinfo.json.template — metadata draft.

## Current runtime behavior
- Start() logs mod name/version and marks diagnostics mode READ-ONLY.
- WorldEvent_WorldLoaded() logs world load and inspects existing ChunkManager, SceneLoader, AssetBundleManager and AssetBundleHooker objects/fields through reflection.
- WorldEvent_WorldSaved() logs save events.
- The probe does not spawn, remove, create, or modify game objects, chunk points, AssetBundles or save data.

## Validation
- The diagnostic source compiled locally with the .NET Framework csc.exe using /warnaserror and references to AssemblyLoader, UnityEngine.CoreModule and Assembly-CSharp.
- A source-based .rmod was rebuilt and its ZIP entries and source contents were verified.
- The updated probe has NOT yet been installed or executed in-game. The currently installed Raft/mods package remains the previous lifecycle-only build.

## Limitations
- A preflight DLL is not a .rmod and is not installed in the game.
- Static discovery of manager classes does not prove that runtime instances/fields will be available in a live world.
- No custom AssetBundle, island, biome, spawn rule, persistence or multiplayer behavior is implemented.
- Do not copy a new package to the game mods folder until the user explicitly approves replacing the installed test package.

[executed on device: DESKTOP-3MKBODB (8088b63f-2a05-4f71-a7a4-ad740486ff11)]