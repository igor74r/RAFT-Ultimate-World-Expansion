using System;
using System.Collections.Generic;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Builds deterministic, data-only island placement candidates.
    /// It does not instantiate prefabs, modify ChunkManager, or touch the world.
    /// </summary>
    public static class WorldSpawnPlanner
    {
        public static WorldSpawnPlan Create(
            WorldCatalogLoadResult catalog,
            string biomeId,
            int worldSeed,
            int islandCount,
            float minRadius,
            float maxRadius)
        {
            return Create(catalog, biomeId, worldSeed, islandCount, minRadius, maxRadius, 100f);
        }

        public static WorldSpawnPlan Create(
            WorldCatalogLoadResult catalog,
            string biomeId,
            int worldSeed,
            int islandCount,
            float minRadius,
            float maxRadius,
            float minSeparation)
        {
            if (catalog == null || !catalog.Success)
                throw new ArgumentException("A successfully validated world catalog is required.", "catalog");
            if (islandCount < 1 || islandCount > 256)
                throw new ArgumentOutOfRangeException("islandCount", "Island count must be between 1 and 256.");
            if (Single.IsNaN(minRadius) || Single.IsInfinity(minRadius) ||
                Single.IsNaN(maxRadius) || Single.IsInfinity(maxRadius) ||
                minRadius < 0f || maxRadius <= minRadius)
                throw new ArgumentOutOfRangeException("minRadius", "Radius bounds must be finite, non-negative, and maxRadius must exceed minRadius.");

            if (Single.IsNaN(minSeparation) || Single.IsInfinity(minSeparation) || minSeparation < 0f)
                throw new ArgumentOutOfRangeException("minSeparation", "Minimum separation must be finite and non-negative.");

            List<IslandSpawnCandidate> candidates = new List<IslandSpawnCandidate>(islandCount);
            uint state = MakeSeed(biomeId, worldSeed);
            double minSeparationSquared = (double)minSeparation * minSeparation;

            for (int i = 0; i < islandCount; i++)
            {
                int slotSeed = unchecked((int)Next(ref state));
                BiomeResolution resolution = WorldBiomeResolver.Resolve(catalog, biomeId, slotSeed);
                bool placed = false;

                for (int attempt = 0; attempt < 128 && !placed; attempt++)
                {
                    float angle = NextUnit(ref state) * 6.28318530718f;
                    float radius = minRadius + NextUnit(ref state) * (maxRadius - minRadius);
                    float x = (float)Math.Cos(angle) * radius;
                    float z = (float)Math.Sin(angle) * radius;
                    bool separated = true;

                    for (int previous = 0; previous < candidates.Count; previous++)
                    {
                        double dx = (double)x - candidates[previous].X;
                        double dz = (double)z - candidates[previous].Z;
                        if (dx * dx + dz * dz < minSeparationSquared)
                        {
                            separated = false;
                            break;
                        }
                    }

                    if (separated)
                    {
                        string[] nodes = FilterIslandResources(resolution.SelectedIsland, resolution.Resources);
                        candidates.Add(new IslandSpawnCandidate(i, resolution.SelectedIsland,
                            x, z, nodes, slotSeed));
                        placed = true;
                    }
                }

                if (!placed)
                    throw new InvalidOperationException("Unable to place island slot " + i +
                        " with minimum separation " + minSeparation +
                        " after 128 deterministic attempts. Increase maxRadius or reduce islandCount/minSeparation.");
            }

            return new WorldSpawnPlan(biomeId, worldSeed, minSeparation, candidates.ToArray());
        }

        private static string[] FilterIslandResources(
            IslandDefinition island, ResourceDefinition[] available)
        {
            List<string> result = new List<string>();
            if (island == null || island.ResourceNodes == null || available == null)
                return result.ToArray();

            for (int i = 0; i < island.ResourceNodes.Length; i++)
            {
                string id = island.ResourceNodes[i];
                for (int r = 0; r < available.Length; r++)
                {
                    if (available[r] != null &&
                        String.Equals(available[r].Id, id, StringComparison.Ordinal))
                    {
                        result.Add(id);
                        break;
                    }
                }
            }
            return result.ToArray();
        }

        private static uint MakeSeed(string biomeId, int seed)
        {
            unchecked
            {
                uint hash = 2166136261;
                if (biomeId != null)
                    for (int i = 0; i < biomeId.Length; i++)
                    {
                        hash ^= biomeId[i];
                        hash *= 16777619;
                    }
                hash ^= (uint)seed;
                hash *= 16777619;
                return hash == 0 ? 0x6D2B79F5u : hash;
            }
        }

        private static uint Next(ref uint state)
        {
            unchecked
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return state;
            }
        }

        private static float NextUnit(ref uint state)
        {
            return (Next(ref state) & 0x00FFFFFFu) / 16777216f;
        }
    }

    public sealed class WorldSpawnPlan
    {
        public string BiomeId { get; private set; }
        public int WorldSeed { get; private set; }
        public float MinimumSeparation { get; private set; }
        public IslandSpawnCandidate[] Islands { get; private set; }

        internal WorldSpawnPlan(string biomeId, int worldSeed, float minimumSeparation,
            IslandSpawnCandidate[] islands)
        {
            BiomeId = biomeId;
            WorldSeed = worldSeed;
            MinimumSeparation = minimumSeparation;
            Islands = islands;
        }
    }

    public sealed class IslandSpawnCandidate
    {
        public int Slot { get; private set; }
        public IslandDefinition Island { get; private set; }
        public float X { get; private set; }
        public float Z { get; private set; }
        public string[] ResourceNodes { get; private set; }
        public int SelectionSeed { get; private set; }

        internal IslandSpawnCandidate(int slot, IslandDefinition island,
            float x, float z, string[] resourceNodes, int selectionSeed)
        {
            Slot = slot;
            Island = island;
            X = x;
            Z = z;
            ResourceNodes = resourceNodes;
            SelectionSeed = selectionSeed;
        }
    }
}
