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
            List<string> errors = new List<string>();
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
                    }

                    WorldResourceDistributionValidationResult resourceValidation =
                        WorldResourceDistributionValidator.Validate(resources,
                            islands.Islands, catalog, 64);
                    AddErrors(errors, "Resource pair " + i, resourceValidation.Errors);
                }
            }

            WorldChunkPlacementValidationResult chunkValidation =
                WorldChunkPlacementValidator.Validate(chunkPlan);
            AddErrors(errors, "Chunk placement", chunkValidation.Errors);

            return new WorldGenerationPlanValidationResult(errors);
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
