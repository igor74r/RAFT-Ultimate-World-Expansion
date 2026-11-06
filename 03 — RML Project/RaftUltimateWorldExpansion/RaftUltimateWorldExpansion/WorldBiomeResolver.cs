using System;
using System.Collections.Generic;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Resolves a content-only biome plan from validated catalogs.
    /// Deterministic for the same biome ID and seed; does not touch Raft or Unity APIs.
    /// </summary>
    public static class WorldBiomeResolver
    {
        public static BiomeResolution Resolve(WorldCatalogLoadResult catalog, string biomeId, int seed)
        {
            if (catalog == null || !catalog.Success)
                throw new ArgumentException("A successfully validated world catalog is required.", "catalog");
            if (String.IsNullOrEmpty(biomeId))
                throw new ArgumentException("Biome ID is required.", "biomeId");

            BiomeDefinition biome = FindBiome(catalog.Biomes, biomeId);
            if (biome == null)
                throw new ArgumentException("Unknown biome ID: " + biomeId, "biomeId");

            List<IslandDefinition> islands = new List<IslandDefinition>();
            for (int i = 0; i < catalog.Islands.Islands.Length; i++)
            {
                IslandDefinition island = catalog.Islands.Islands[i];
                if (island == null || !String.Equals(island.Biome, biome.Id, StringComparison.Ordinal))
                    continue;

                if (Contains(biome.LandmarkPool, island.Id))
                    islands.Add(island);
            }

            List<ResourceDefinition> resources = new List<ResourceDefinition>();
            for (int i = 0; i < catalog.Resources.Resources.Length; i++)
            {
                ResourceDefinition resource = catalog.Resources.Resources[i];
                if (resource != null && Contains(resource.Biomes, biome.Id))
                    resources.Add(resource);
            }

            if (islands.Count == 0)
                throw new InvalidOperationException("Biome has no matching island archetypes: " + biome.Id);
            if (resources.Count == 0)
                throw new InvalidOperationException("Biome has no matching resources: " + biome.Id);

            int selectedIndex = StableIndex(biome.Id, seed, islands.Count);
            return new BiomeResolution(biome, islands.ToArray(), resources.ToArray(),
                islands[selectedIndex], seed);
        }

        private static BiomeDefinition FindBiome(BiomeCatalog catalog, string id)
        {
            if (catalog == null || catalog.Biomes == null)
                return null;
            for (int i = 0; i < catalog.Biomes.Length; i++)
                if (catalog.Biomes[i] != null &&
                    String.Equals(catalog.Biomes[i].Id, id, StringComparison.Ordinal))
                    return catalog.Biomes[i];
            return null;
        }

        private static bool Contains(string[] values, string expected)
        {
            if (values == null)
                return false;
            for (int i = 0; i < values.Length; i++)
                if (String.Equals(values[i], expected, StringComparison.Ordinal))
                    return true;
            return false;
        }

        private static int StableIndex(string biomeId, int seed, int count)
        {
            unchecked
            {
                uint hash = 2166136261;
                for (int i = 0; i < biomeId.Length; i++)
                {
                    hash ^= biomeId[i];
                    hash *= 16777619;
                }
                hash ^= (uint)seed;
                hash *= 16777619;
                return (int)(hash % (uint)count);
            }
        }
    }

    public sealed class BiomeResolution
    {
        public BiomeDefinition Biome { get; private set; }
        public IslandDefinition[] IslandCandidates { get; private set; }
        public ResourceDefinition[] Resources { get; private set; }
        public IslandDefinition SelectedIsland { get; private set; }
        public int Seed { get; private set; }

        internal BiomeResolution(BiomeDefinition biome, IslandDefinition[] islands,
            ResourceDefinition[] resources, IslandDefinition selectedIsland, int seed)
        {
            Biome = biome;
            IslandCandidates = islands;
            Resources = resources;
            SelectedIsland = selectedIsland;
            Seed = seed;
        }
    }
}
