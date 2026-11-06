# World content catalogs

The current world content is split into three JSON catalogs:

- `biomes.json` — biome identity, climate bands, environment tags, resource tags, and landmark pools.
- `resources.json` — resource identity, biome associations, sources, and intended uses.
- `islands.json` — island/landmark archetypes and their biome/resource references.

## Runtime loader

The RML project includes a standalone C# loader at:

`03 — RML Project/RaftUltimateWorldExpansion/RaftUltimateWorldExpansion/WorldCatalog.cs`

It exposes:

```csharp
WorldCatalogLoadResult result = WorldCatalog.LoadDirectory(catalogDirectory);
if (!result.Success)
{
    // Report result.Errors; do not use partial catalogs.
    return;
}

// result.Biomes, result.Resources, and result.Islands are validated catalogs.
```

The caller must supply the directory containing all three JSON files. The loader does not guess the mod installation path, access Unity/Raft APIs, spawn objects, or modify world/save/network state.

## Validation behavior

- Requires all three files and limits each file to 1 MiB.
- Supports schema version 1.
- Rejects empty catalogs, missing IDs, duplicate biome/resource/island IDs, and missing required arrays.
- Checks resource-to-biome, island-to-biome, island-to-resource, and biome-to-resource references.
- A failed load returns errors; callers must not consume partially loaded catalogs.

## Current status

**Loader source is included in the project file, but has not been compiled or run in this repository-only workflow.** The JSON catalogs remain content data; no automatic runtime loading or world generation is claimed.
