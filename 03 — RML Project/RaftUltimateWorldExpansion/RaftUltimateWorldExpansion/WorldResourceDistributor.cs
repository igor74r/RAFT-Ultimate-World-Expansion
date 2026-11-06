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
            if (Single.IsNaN(transitionWidthRadians) || Single.IsInfinity(transitionWidthRadians) ||
                transitionWidthRadians < 0f || transitionWidthRadians >= 1.57079632679f)
                throw new ArgumentOutOfRangeException("transitionWidthRadians");
            if (regions == null || regions.Regions == null || regions.Regions.Length == 0)
                throw new ArgumentException("A non-empty region plan is required.", "regions");
            if (regions.WorldSeed != worldSeed)
                throw new InvalidOperationException("Resource distribution and regional layout must use the same world seed.");

            List<WorldResourceNode> nodes = new List<WorldResourceNode>();
            HashSet<string> placements = new HashSet<string>(StringComparer.Ordinal);
            HashSet<int> islandSlots = new HashSet<int>();

            for (int i = 0; i < islands.Length; i++)
            {
                IslandSpawnCandidate island = islands[i];
                if (island == null || island.Island == null)
                    throw new ArgumentException("Island candidates cannot contain null entries.", "islands");
                if (island.Slot < 0)
                    throw new ArgumentException("Island candidate slots cannot be negative.", "islands");
                if (!islandSlots.Add(island.Slot))
                    throw new ArgumentException("Island candidate slots must be unique within a distribution batch.", "islands");
                if (Single.IsNaN(island.X) || Single.IsInfinity(island.X) ||
                    Single.IsNaN(island.Z) || Single.IsInfinity(island.Z))
                    throw new ArgumentException("Island candidate coordinates must be finite.", "islands");
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

                    bool placed = false;
                    for (int attempt = 0; attempt < 24; attempt++)
                    {
                        float angle = NextUnit(ref state) * 6.28318530718f;
                        float radius = spreadRadius * (float)Math.Sqrt(NextUnit(ref state));
                        float x = island.X + (float)Math.Cos(angle) * radius;
                        float z = island.Z + (float)Math.Sin(angle) * radius;
                        WorldRegionSelection nodeRegion = WorldRegionResolver.Resolve(
                            regions, x, z, transitionWidthRadians);
                        if (!nodeRegion.IsInsideRegion ||
                            String.IsNullOrEmpty(nodeRegion.PrimaryBiomeId))
                            continue;

                        bool nodePrimaryAllowed = Contains(resource.Biomes, nodeRegion.PrimaryBiomeId);
                        bool nodeTransitionAllowed = nodeRegion.IsTransition &&
                            Contains(resource.Biomes, nodeRegion.TransitionBiomeId);
                        bool nodeIslandBiomeMatches =
                            String.Equals(island.Island.Biome, nodeRegion.PrimaryBiomeId, StringComparison.Ordinal) ||
                            (nodeRegion.IsTransition &&
                             String.Equals(island.Island.Biome, nodeRegion.TransitionBiomeId, StringComparison.Ordinal));
                        if (!nodeIslandBiomeMatches || (!nodePrimaryAllowed && !nodeTransitionAllowed))
                            continue;

                        string key = island.Slot.ToString() + ":" + resource.Id;
                        if (!placements.Add(key))
                            break;

                        nodes.Add(new WorldResourceNode(island.Slot, resource.Id, x, z,
                            nodeRegion.PrimaryBiomeId,
                            nodeRegion.IsTransition ? nodeRegion.TransitionBiomeId : null,
                            nodeRegion.TransitionWeight));
                        placed = true;
                        break;
                    }
                    if (!placed)
                        continue;
                }
            }

            return new WorldResourceDistributionPlan(worldSeed, spreadRadius, transitionWidthRadians, nodes.ToArray());
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
        public float SpreadRadius { get; private set; }
        public float TransitionWidthRadians { get; private set; }
        public WorldResourceNode[] Nodes { get; private set; }

        internal WorldResourceDistributionPlan(int worldSeed, float spreadRadius,
            float transitionWidthRadians, WorldResourceNode[] nodes)
        {
            WorldSeed = worldSeed;
            SpreadRadius = spreadRadius;
            TransitionWidthRadians = transitionWidthRadians;
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
        public float TransitionWeight { get; private set; }

        internal WorldResourceNode(int islandSlot, string resourceId, float x, float z,
            string primaryBiomeId, string transitionBiomeId, float transitionWeight)
        {
            IslandSlot = islandSlot;
            ResourceId = resourceId;
            X = x;
            Z = z;
            PrimaryBiomeId = primaryBiomeId;
            TransitionBiomeId = transitionBiomeId;
            TransitionWeight = transitionWeight;
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
            WorldResourceDistributionPlan plan, WorldRegionPlan regions,
            IslandSpawnCandidate[] islands, WorldCatalogLoadResult catalog,
            int maxNodesPerIsland)
        {
            WorldResourceDistributionValidationResult result =
                new WorldResourceDistributionValidationResult();
            if (maxNodesPerIsland < 1 || maxNodesPerIsland > 64)
                result.Errors.Add("Maximum nodes per island must be between 1 and 64.");
            if (plan == null || plan.Nodes == null || regions == null ||
                regions.Regions == null || islands == null ||
                catalog == null || !catalog.Success ||
                catalog.Resources == null || catalog.Resources.Resources == null ||
                catalog.Biomes == null || catalog.Biomes.Biomes == null)
            {
                result.Errors.Add("Plan, islands, and validated resource catalog are required.");
                return result;
            }

            if (Single.IsNaN(plan.SpreadRadius) || Single.IsInfinity(plan.SpreadRadius) ||
                plan.SpreadRadius < 0f)
                result.Errors.Add("Resource spread radius must be finite and non-negative.");
            if (Single.IsNaN(plan.TransitionWidthRadians) ||
                Single.IsInfinity(plan.TransitionWidthRadians) ||
                plan.TransitionWidthRadians < 0f ||
                plan.TransitionWidthRadians >= 1.57079632679f)
                result.Errors.Add("Resource transition width must be finite and between zero and PI/2.");
            if (regions != null && regions.WorldSeed != plan.WorldSeed)
                result.Errors.Add("Resource plan and regional layout must use the same world seed.");

            HashSet<string> resourceIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < catalog.Resources.Resources.Length; i++)
                if (catalog.Resources.Resources[i] != null)
                    resourceIds.Add(catalog.Resources.Resources[i].Id);

            HashSet<string> biomeIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < catalog.Biomes.Biomes.Length; i++)
                if (catalog.Biomes.Biomes[i] != null)
                    biomeIds.Add(catalog.Biomes.Biomes[i].Id);

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
                else if (!biomeIds.Contains(node.PrimaryBiomeId))
                    result.Errors.Add("Resource node " + i + " references an unknown primary biome.");

                if (!String.IsNullOrEmpty(node.TransitionBiomeId))
                {
                    if (!biomeIds.Contains(node.TransitionBiomeId))
                        result.Errors.Add("Resource node " + i + " references an unknown transition biome.");
                    if (String.Equals(node.PrimaryBiomeId, node.TransitionBiomeId, StringComparison.Ordinal))
                        result.Errors.Add("Resource node " + i + " repeats its primary biome as transition biome.");
                }

                if (definition != null && !String.IsNullOrEmpty(node.PrimaryBiomeId))
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
                if (Single.IsNaN(node.TransitionWeight) || Single.IsInfinity(node.TransitionWeight) ||
                    node.TransitionWeight < 0f || node.TransitionWeight > 1f)
                    result.Errors.Add("Resource node " + i + " has a transition weight outside [0, 1].");

                bool finiteCoordinates = !Single.IsNaN(node.X) && !Single.IsInfinity(node.X) &&
                    !Single.IsNaN(node.Z) && !Single.IsInfinity(node.Z);
                if (!finiteCoordinates)
                    result.Errors.Add("Resource node " + i + " has non-finite coordinates.");
                else if (regions != null && regions.Regions != null &&
                    !Single.IsNaN(plan.TransitionWidthRadians) &&
                    !Single.IsInfinity(plan.TransitionWidthRadians) &&
                    plan.TransitionWidthRadians >= 0f &&
                    plan.TransitionWidthRadians < 1.57079632679f)
                {
                    try
                    {
                        WorldRegionSelection resolved = WorldRegionResolver.Resolve(
                            regions, node.X, node.Z, plan.TransitionWidthRadians);
                        string expectedTransition = resolved.IsTransition
                            ? resolved.TransitionBiomeId : null;
                        if (!resolved.IsInsideRegion ||
                            !String.Equals(resolved.PrimaryBiomeId, node.PrimaryBiomeId, StringComparison.Ordinal) ||
                            !String.Equals(expectedTransition, node.TransitionBiomeId, StringComparison.Ordinal) ||
                            Math.Abs((double)resolved.TransitionWeight - node.TransitionWeight) > 0.0001d)
                            result.Errors.Add("Resource node " + i +
                                " biome context or transition weight does not exactly match its resolved world position.");
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add("Resource node " + i +
                            " could not be resolved against the regional map: " +
                            ex.GetType().Name + ".");
                    }
                }

                if (owner != null && owner.Island != null &&
                    !Single.IsNaN(node.X) && !Single.IsInfinity(node.X) &&
                    !Single.IsNaN(node.Z) && !Single.IsInfinity(node.Z) &&
                    !Single.IsNaN(plan.SpreadRadius) && !Single.IsInfinity(plan.SpreadRadius) &&
                    plan.SpreadRadius >= 0f)
                {
                    double dx = (double)node.X - owner.X;
                    double dz = (double)node.Z - owner.Z;
                    double radiusSquared = (double)plan.SpreadRadius * plan.SpreadRadius;
                    if (dx * dx + dz * dz > radiusSquared + 0.01d)
                        result.Errors.Add("Resource node " + i +
                            " lies outside its owning island's configured distribution radius.");
                }

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
