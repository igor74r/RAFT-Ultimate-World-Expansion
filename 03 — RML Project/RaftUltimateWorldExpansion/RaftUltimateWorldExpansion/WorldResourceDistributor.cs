using System;
using System.Collections.Generic;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Creates deterministic resource-node placements around planned islands.
    /// Data-only: no prefabs, game objects, chunks, saves, or network state are touched.
    /// </summary>
    public static class WorldResourceDistributor
    {
        public static WorldResourceDistributionPlan Create(
            WorldCatalogLoadResult catalog, WorldRegionPlan regions,
            IslandSpawnCandidate[] islands, int worldSeed,
            int maxNodesPerIsland, float spreadRadius, float transitionWidthRadians)
        {
            if (catalog == null || !catalog.Success)
                throw new ArgumentException("A successfully validated catalog is required.", "catalog");
            if (islands == null)
                throw new ArgumentNullException("islands");
            if (maxNodesPerIsland < 1 || maxNodesPerIsland > 64)
                throw new ArgumentOutOfRangeException("maxNodesPerIsland");
            if (Single.IsNaN(spreadRadius) || Single.IsInfinity(spreadRadius) || spreadRadius < 0f)
                throw new ArgumentOutOfRangeException("spreadRadius");
            if (regions == null || regions.Regions == null || regions.Regions.Length == 0)
                throw new ArgumentException("A non-empty region plan is required.", "regions");

            List<WorldResourceNode> nodes = new List<WorldResourceNode>();
            HashSet<string> placements = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < islands.Length; i++)
            {
                IslandSpawnCandidate island = islands[i];
                if (island == null || island.Island == null)
                    throw new ArgumentException("Island candidates cannot contain null entries.", "islands");
                if (island.ResourceNodes == null)
                    continue;

                WorldRegionSelection region = WorldRegionResolver.Resolve(
                    regions, island.X, island.Z, transitionWidthRadians);
                if (!region.IsInsideRegion || String.IsNullOrEmpty(region.PrimaryBiomeId))
                    continue;

                List<ResourceDefinition> eligible = new List<ResourceDefinition>();
                for (int n = 0; n < island.ResourceNodes.Length; n++)
                {
                    ResourceDefinition resource = FindResource(catalog.Resources.Resources, island.ResourceNodes[n]);
                    if (resource == null)
                        continue;
                    bool primary = Contains(resource.Biomes, region.PrimaryBiomeId);
                    bool transition = region.IsTransition &&
                        Contains(resource.Biomes, region.TransitionBiomeId);
                    bool islandBiomeMatches = String.Equals(island.Island.Biome, region.PrimaryBiomeId, StringComparison.Ordinal) ||
                        (region.IsTransition && String.Equals(island.Island.Biome, region.TransitionBiomeId, StringComparison.Ordinal));
                    if (islandBiomeMatches && (primary || transition))
                        eligible.Add(resource);
                }

                uint state = MakeSeed(worldSeed, island.Slot, island.SelectionSeed);
                int count = Math.Min(maxNodesPerIsland, eligible.Count);
                for (int n = 0; n < count; n++)
                {
                    int selected = (int)(Next(ref state) % (uint)eligible.Count);
                    ResourceDefinition resource = eligible[selected];
                    eligible.RemoveAt(selected);

                    float angle = NextUnit(ref state) * 6.28318530718f;
                    float radius = spreadRadius * (float)Math.Sqrt(NextUnit(ref state));
                    float x = island.X + (float)Math.Cos(angle) * radius;
                    float z = island.Z + (float)Math.Sin(angle) * radius;
                    string key = island.Slot.ToString() + ":" + resource.Id;
                    if (!placements.Add(key))
                        continue;

                    nodes.Add(new WorldResourceNode(island.Slot, resource.Id, x, z,
                        region.PrimaryBiomeId, region.IsTransition ? region.TransitionBiomeId : null));
                }
            }

            return new WorldResourceDistributionPlan(worldSeed, nodes.ToArray());
        }

        private static ResourceDefinition FindResource(ResourceDefinition[] resources, string id)
        {
            if (resources == null || String.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < resources.Length; i++)
                if (resources[i] != null && String.Equals(resources[i].Id, id, StringComparison.Ordinal))
                    return resources[i];
            return null;
        }

        private static bool Contains(string[] values, string value)
        {
            if (values == null || String.IsNullOrEmpty(value)) return false;
            for (int i = 0; i < values.Length; i++)
                if (String.Equals(values[i], value, StringComparison.Ordinal)) return true;
            return false;
        }

        private static uint MakeSeed(int worldSeed, int slot, int selectionSeed)
        {
            unchecked
            {
                uint value = 2166136261;
                value = (value ^ (uint)worldSeed) * 16777619;
                value = (value ^ (uint)slot) * 16777619;
                value = (value ^ (uint)selectionSeed) * 16777619;
                return value == 0 ? 0x9E3779B9u : value;
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

    public sealed class WorldResourceDistributionPlan
    {
        public int WorldSeed { get; private set; }
        public WorldResourceNode[] Nodes { get; private set; }

        internal WorldResourceDistributionPlan(int worldSeed, WorldResourceNode[] nodes)
        {
            WorldSeed = worldSeed;
            Nodes = nodes;
        }
    }

    public sealed class WorldResourceNode
    {
        public int IslandSlot { get; private set; }
        public string ResourceId { get; private set; }
        public float X { get; private set; }
        public float Z { get; private set; }
        public string PrimaryBiomeId { get; private set; }
        public string TransitionBiomeId { get; private set; }

        internal WorldResourceNode(int islandSlot, string resourceId, float x, float z,
            string primaryBiomeId, string transitionBiomeId)
        {
            IslandSlot = islandSlot;
            ResourceId = resourceId;
            X = x;
            Z = z;
            PrimaryBiomeId = primaryBiomeId;
            TransitionBiomeId = transitionBiomeId;
        }
    }

    public sealed class WorldResourceDistributionValidationResult
    {
        public bool Success { get { return Errors.Count == 0; } }
        public List<string> Errors { get; private set; }

        internal WorldResourceDistributionValidationResult()
        {
            Errors = new List<string>();
        }
    }

    public static class WorldResourceDistributionValidator
    {
        private static bool ContainsResource(string[] values, string expected)
        {
            if (values == null || String.IsNullOrEmpty(expected))
                return false;
            for (int i = 0; i < values.Length; i++)
                if (String.Equals(values[i], expected, StringComparison.Ordinal))
                    return true;
            return false;
        }

        private static ResourceDefinition FindResource(ResourceDefinition[] values, string id)
        {
            if (values == null || String.IsNullOrEmpty(id))
                return null;
            for (int i = 0; i < values.Length; i++)
                if (values[i] != null && String.Equals(values[i].Id, id, StringComparison.Ordinal))
                    return values[i];
            return null;
        }

        public static WorldResourceDistributionValidationResult Validate(
            WorldResourceDistributionPlan plan, IslandSpawnCandidate[] islands,
            WorldCatalogLoadResult catalog, int maxNodesPerIsland)
        {
            WorldResourceDistributionValidationResult result =
                new WorldResourceDistributionValidationResult();
            if (maxNodesPerIsland < 1 || maxNodesPerIsland > 64)
                result.Errors.Add("Maximum nodes per island must be between 1 and 64.");
            if (plan == null || plan.Nodes == null || islands == null ||
                catalog == null || !catalog.Success ||
                catalog.Resources == null || catalog.Resources.Resources == null)
            {
                result.Errors.Add("Plan, islands, and validated resource catalog are required.");
                return result;
            }

            HashSet<string> resourceIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < catalog.Resources.Resources.Length; i++)
                if (catalog.Resources.Resources[i] != null)
                    resourceIds.Add(catalog.Resources.Resources[i].Id);

            Dictionary<int, int> counts = new Dictionary<int, int>();
            HashSet<string> unique = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < plan.Nodes.Length; i++)
            {
                WorldResourceNode node = plan.Nodes[i];
                if (node == null)
                {
                    result.Errors.Add("Resource node " + i + " is null.");
                    continue;
                }
                IslandSpawnCandidate owner = null;
                for (int islandIndex = 0; islandIndex < islands.Length; islandIndex++)
                    if (islands[islandIndex] != null && islands[islandIndex].Slot == node.IslandSlot)
                    { owner = islands[islandIndex]; break; }
                if (node.IslandSlot < 0)
                    result.Errors.Add("Resource node " + i + " has a negative island slot.");
                if (owner == null)
                    result.Errors.Add("Resource node " + i + " references an unknown island slot.");
                else if (!ContainsResource(owner.ResourceNodes, node.ResourceId))
                    result.Errors.Add("Resource node " + i + " is not declared by its owning island.");

                ResourceDefinition definition = FindResource(catalog.Resources.Resources, node.ResourceId);
                if (definition == null)
                    result.Errors.Add("Resource node " + i + " references an unknown resource.");
                if (String.IsNullOrEmpty(node.PrimaryBiomeId))
                    result.Errors.Add("Resource node " + i + " has no primary biome identity.");
                else if (definition != null)
                {
                    bool primaryAllowed = ContainsResource(definition.Biomes, node.PrimaryBiomeId);
                    bool transitionAllowed = !String.IsNullOrEmpty(node.TransitionBiomeId) &&
                        ContainsResource(definition.Biomes, node.TransitionBiomeId);
                    if (!primaryAllowed && !transitionAllowed)
                        result.Errors.Add("Resource node " + i + " is not allowed in its recorded biome context.");
                    if (owner != null && owner.Island != null &&
                        !String.Equals(owner.Island.Biome, node.PrimaryBiomeId, StringComparison.Ordinal) &&
                        !String.Equals(owner.Island.Biome, node.TransitionBiomeId, StringComparison.Ordinal))
                        result.Errors.Add("Resource node " + i + " biome context does not match its owning island.");
                }
                if (Single.IsNaN(node.X) || Single.IsInfinity(node.X) ||
                    Single.IsNaN(node.Z) || Single.IsInfinity(node.Z))
                    result.Errors.Add("Resource node " + i + " has non-finite coordinates.");

                string key = node.IslandSlot.ToString() + ":" + node.ResourceId;
                if (!unique.Add(key))
                    result.Errors.Add("Duplicate resource placement: " + key + ".");
                int count;
                counts.TryGetValue(node.IslandSlot, out count);
                counts[node.IslandSlot] = count + 1;
            }

            foreach (KeyValuePair<int, int> pair in counts)
                if (pair.Value > maxNodesPerIsland)
                    result.Errors.Add("Island slot " + pair.Key + " exceeds its resource-node limit.");
            return result;
        }
    }
}
