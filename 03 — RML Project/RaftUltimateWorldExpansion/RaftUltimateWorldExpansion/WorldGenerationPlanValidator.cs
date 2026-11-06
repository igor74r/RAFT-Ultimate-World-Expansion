using System;
using System.Collections.Generic;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Cross-validates the data-only world-generation pipeline.
    /// It never creates game objects or changes Raft world state.
    /// Island and resource plan arrays are paired by index.
    /// </summary>
    public static class WorldGenerationPlanValidator
    {
        public static WorldGenerationPlanValidationResult Validate(
            WorldCatalogLoadResult catalog,
            WorldRegionPlan regionPlan,
            WorldSpawnPlan[] islandPlans,
            WorldResourceDistributionPlan[] resourcePlans,
            WorldChunkPlacementPlan chunkPlan)
        {
            return Validate(catalog, regionPlan, islandPlans, resourcePlans, chunkPlan, 64);
        }

        public static WorldGenerationPlanValidationResult Validate(
            WorldCatalogLoadResult catalog,
            WorldRegionPlan regionPlan,
            WorldSpawnPlan[] islandPlans,
            WorldResourceDistributionPlan[] resourcePlans,
            WorldChunkPlacementPlan chunkPlan,
            int maxResourceNodesPerIsland)
        {
            List<string> errors = new List<string>();
            if (maxResourceNodesPerIsland < 1 || maxResourceNodesPerIsland > 64)
                errors.Add("Maximum resource nodes per island must be between 1 and 64.");
            if (catalog == null || !catalog.Success)
                errors.Add("A successfully validated world catalog is required.");
            if (regionPlan == null)
                errors.Add("A regional layout is required.");
            else
            {
                WorldRegionPlanValidationResult regionValidation =
                    WorldRegionPlanValidator.Validate(regionPlan, catalog);
                AddErrors(errors, "Region", regionValidation.Errors);
            }

            if (islandPlans == null || resourcePlans == null)
                errors.Add("Island and resource plan arrays are required.");
            else if (islandPlans.Length != resourcePlans.Length)
                errors.Add("Island and resource plan arrays must have matching lengths.");

            if (islandPlans != null && resourcePlans != null)
            {
                int count = Math.Min(islandPlans.Length, resourcePlans.Length);
                HashSet<string> biomePlans = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < count; i++)
                {
                    WorldSpawnPlan islands = islandPlans[i];
                    WorldResourceDistributionPlan resources = resourcePlans[i];
                    if (islands == null || resources == null)
                    {
                        errors.Add("Generation pair " + i + " contains a null plan.");
                        continue;
                    }
                    if (islands.WorldSeed != resources.WorldSeed)
                        errors.Add("Generation pair " + i + " has mismatched island/resource seeds.");
                    if (regionPlan != null && regionPlan.WorldSeed != islands.WorldSeed)
                        errors.Add("Generation pair " + i + " does not use the regional layout seed.");
                    if (String.IsNullOrEmpty(islands.BiomeId) || !biomePlans.Add(islands.BiomeId))
                        errors.Add("Generation pair " + i + " has an empty or duplicated biome plan.");

                    WorldRegion matchingRegion = FindRegion(regionPlan, islands.BiomeId);
                    if (matchingRegion == null)
                        errors.Add("Generation pair " + i + " has no matching biome region.");
                    else
                    {
                        WorldSpawnPlanValidationResult spawnValidation =
                            WorldSpawnPlanValidator.Validate(islands,
                                matchingRegion.InnerRadius, matchingRegion.OuterRadius);
                        AddErrors(errors, "Island pair " + i, spawnValidation.Errors);
                        ValidateIslandRegionMembership(errors, islands, regionPlan);
                    }

                    WorldResourceDistributionValidationResult resourceValidation =
                        WorldResourceDistributionValidator.Validate(resources,
                            regionPlan, islands.Islands, catalog, maxResourceNodesPerIsland);
                    AddErrors(errors, "Resource pair " + i, resourceValidation.Errors);

                }

                ValidateBiomePlanCoverage(errors, catalog, biomePlans);
                ValidateResourcePolicyConsistency(errors, resourcePlans);
            }

            WorldChunkPlacementValidationResult chunkValidation =
                WorldChunkPlacementValidator.Validate(chunkPlan);
            AddErrors(errors, "Chunk placement", chunkValidation.Errors);
            ValidateChunkPlanCoverage(errors, islandPlans, resourcePlans, chunkPlan);

            return new WorldGenerationPlanValidationResult(errors);
        }


        private static void ValidateResourcePolicyConsistency(
            List<string> errors, WorldResourceDistributionPlan[] plans)
        {
            if (plans == null || plans.Length < 2)
                return;

            WorldResourceDistributionPlan baseline = plans[0];
            if (baseline == null)
                return;

            for (int i = 1; i < plans.Length; i++)
            {
                WorldResourceDistributionPlan current = plans[i];
                if (current == null)
                    continue;

                if (Single.IsNaN(baseline.SpreadRadius) || Single.IsInfinity(baseline.SpreadRadius) ||
                    Single.IsNaN(current.SpreadRadius) || Single.IsInfinity(current.SpreadRadius) ||
                    Math.Abs((double)baseline.SpreadRadius - current.SpreadRadius) > 0.0001d)
                    errors.Add("Resource plans do not use a consistent spread radius.");

                if (Single.IsNaN(baseline.TransitionWidthRadians) ||
                    Single.IsInfinity(baseline.TransitionWidthRadians) ||
                    Single.IsNaN(current.TransitionWidthRadians) ||
                    Single.IsInfinity(current.TransitionWidthRadians) ||
                    Math.Abs((double)baseline.TransitionWidthRadians - current.TransitionWidthRadians) > 0.000001d)
                    errors.Add("Resource plans do not use a consistent biome transition width.");

                if (Single.IsNaN(baseline.MinimumNodeSeparation) ||
                    Single.IsInfinity(baseline.MinimumNodeSeparation) ||
                    Single.IsNaN(current.MinimumNodeSeparation) ||
                    Single.IsInfinity(current.MinimumNodeSeparation) ||
                    Math.Abs((double)baseline.MinimumNodeSeparation - current.MinimumNodeSeparation) > 0.0001d)
                    errors.Add("Resource plans do not use a consistent minimum node separation.");
            }
        }

        private static void ValidateBiomePlanCoverage(
            List<string> errors, WorldCatalogLoadResult catalog, HashSet<string> plannedBiomeIds)
        {
            if (catalog == null || !catalog.Success ||
                catalog.Biomes == null || catalog.Biomes.Biomes == null ||
                plannedBiomeIds == null)
                return;

            HashSet<string> catalogBiomeIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < catalog.Biomes.Biomes.Length; i++)
            {
                BiomeDefinition biome = catalog.Biomes.Biomes[i];
                if (biome == null || String.IsNullOrEmpty(biome.Id))
                    continue;
                catalogBiomeIds.Add(biome.Id);
                if (!plannedBiomeIds.Contains(biome.Id))
                    errors.Add("Generation plans are missing biome '" + biome.Id + "'.");
            }

            foreach (string plannedBiomeId in plannedBiomeIds)
                if (!catalogBiomeIds.Contains(plannedBiomeId))
                    errors.Add("Generation plans reference unknown biome '" + plannedBiomeId + "'.");
        }

        private static void ValidateIslandRegionMembership(
            List<string> errors, WorldSpawnPlan plan, WorldRegionPlan regions)
        {
            if (plan == null || plan.Islands == null || regions == null)
                return;

            for (int i = 0; i < plan.Islands.Length; i++)
            {
                IslandSpawnCandidate island = plan.Islands[i];
                if (island == null || island.Island == null ||
                    Single.IsNaN(island.X) || Single.IsInfinity(island.X) ||
                    Single.IsNaN(island.Z) || Single.IsInfinity(island.Z))
                    continue;

                WorldRegionSelection resolved;
                try
                {
                    resolved = WorldRegionResolver.Resolve(regions, island.X, island.Z, 0f);
                }
                catch (Exception ex)
                {
                    errors.Add("Island pair " + plan.BiomeId + " candidate " +
                        island.Slot + " could not be resolved against the regional map: " +
                        ex.GetType().Name + ".");
                    continue;
                }

                if (!resolved.IsInsideRegion ||
                    !String.Equals(resolved.PrimaryBiomeId, plan.BiomeId, StringComparison.Ordinal))
                    errors.Add("Island pair " + plan.BiomeId + " candidate " +
                        island.Slot + " is outside its assigned biome sector.");
            }
        }

        private sealed class ExpectedPlacement
        {
            public WorldPlacementKind Kind;
            public int OwnerSlot;
            public string DefinitionId;
            public float X;
            public float Z;
        }

        private static void ValidateChunkPlanCoverage(List<string> errors,
            WorldSpawnPlan[] islandPlans, WorldResourceDistributionPlan[] resourcePlans,
            WorldChunkPlacementPlan chunkPlan)
        {
            if (islandPlans == null || resourcePlans == null ||
                chunkPlan == null || chunkPlan.Placements == null)
                return;

            Dictionary<string, ExpectedPlacement> expected =
                new Dictionary<string, ExpectedPlacement>(StringComparer.Ordinal);
            for (int p = 0; p < islandPlans.Length; p++)
            {
                WorldSpawnPlan plan = islandPlans[p];
                if (plan == null || plan.Islands == null) continue;
                for (int i = 0; i < plan.Islands.Length; i++)
                {
                    IslandSpawnCandidate island = plan.Islands[i];
                    if (island == null || island.Island == null) continue;
                    string identity = "island:" + plan.BiomeId + ":" + island.Slot;
                    if (expected.ContainsKey(identity))
                    {
                        errors.Add("Source plans contain duplicate placement identity: " + identity + ".");
                        continue;
                    }
                    expected.Add(identity, new ExpectedPlacement {
                        Kind = WorldPlacementKind.Island, OwnerSlot = island.Slot,
                        DefinitionId = island.Island.Id, X = island.X, Z = island.Z
                    });
                }
            }
            for (int p = 0; p < resourcePlans.Length; p++)
            {
                WorldResourceDistributionPlan plan = resourcePlans[p];
                if (plan == null || plan.Nodes == null) continue;
                for (int i = 0; i < plan.Nodes.Length; i++)
                {
                    WorldResourceNode node = plan.Nodes[i];
                    if (node == null) continue;
                    string identity = "resource:" + node.PrimaryBiomeId + ":" +
                        node.IslandSlot + ":" + node.ResourceId;
                    if (expected.ContainsKey(identity))
                    {
                        errors.Add("Source plans contain duplicate placement identity: " + identity + ".");
                        continue;
                    }
                    expected.Add(identity, new ExpectedPlacement {
                        Kind = WorldPlacementKind.Resource, OwnerSlot = node.IslandSlot,
                        DefinitionId = node.ResourceId, X = node.X, Z = node.Z
                    });
                }
            }

            HashSet<string> actual = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < chunkPlan.Placements.Length; i++)
            {
                WorldChunkPlacement placement = chunkPlan.Placements[i];
                if (placement == null || String.IsNullOrEmpty(placement.Identity) ||
                    !actual.Add(placement.Identity))
                    continue;

                ExpectedPlacement source;
                if (!expected.TryGetValue(placement.Identity, out source))
                {
                    errors.Add("Chunk placement contains an item absent from the source plans: " +
                        placement.Identity + ".");
                    continue;
                }
                if (placement.Kind != source.Kind ||
                    placement.OwnerIslandSlot != source.OwnerSlot ||
                    !String.Equals(placement.DefinitionId, source.DefinitionId, StringComparison.Ordinal))
                    errors.Add("Chunk placement metadata differs from its source plan: " +
                        placement.Identity + ".");
                if (!SameCoordinate(placement.WorldX, source.X) ||
                    !SameCoordinate(placement.WorldZ, source.Z))
                    errors.Add("Chunk placement coordinates differ from its source plan: " +
                        placement.Identity + ".");
            }

            foreach (KeyValuePair<string, ExpectedPlacement> item in expected)
                if (!actual.Contains(item.Key))
                    errors.Add("Chunk placement is missing a source-plan item: " + item.Key + ".");
            if (actual.Count != expected.Count)
                errors.Add("Chunk placement coverage count does not match the source plans.");
        }

        private static bool SameCoordinate(float actual, float expected)
        {
            if (Single.IsNaN(actual) || Single.IsInfinity(actual) ||
                Single.IsNaN(expected) || Single.IsInfinity(expected))
                return false;
            return Math.Abs((double)actual - expected) <= 0.01d;
        }

        private static WorldRegion FindRegion(WorldRegionPlan plan, string biomeId)
        {
            if (plan == null || plan.Regions == null || String.IsNullOrEmpty(biomeId))
                return null;
            for (int i = 0; i < plan.Regions.Length; i++)
                if (plan.Regions[i] != null &&
                    String.Equals(plan.Regions[i].BiomeId, biomeId, StringComparison.Ordinal))
                    return plan.Regions[i];
            return null;
        }

        private static void AddErrors(List<string> target, string prefix, IList<string> source)
        {
            if (source == null) return;
            for (int i = 0; i < source.Count; i++)
                target.Add(prefix + ": " + source[i]);
        }
    }

    public sealed class WorldGenerationPlanValidationResult
    {
        public bool Success { get { return Errors.Count == 0; } }
        public IList<string> Errors { get; private set; }

        internal WorldGenerationPlanValidationResult(List<string> errors)
        {
            Errors = errors.AsReadOnly();
        }
    }
}
