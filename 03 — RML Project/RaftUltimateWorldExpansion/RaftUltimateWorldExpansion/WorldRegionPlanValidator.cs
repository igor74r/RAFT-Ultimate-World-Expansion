using System;
using System.Collections.Generic;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Validates the structure and biome adjacency of a data-only region plan.
    /// </summary>
    public static class WorldRegionPlanValidator
    {
        public static WorldRegionPlanValidationResult Validate(
            WorldRegionPlan plan, WorldCatalogLoadResult catalog)
        {
            List<string> errors = new List<string>();
            if (plan == null)
            {
                errors.Add("Region plan is null.");
                return new WorldRegionPlanValidationResult(errors);
            }
            if (catalog == null || !catalog.Success ||
                catalog.Biomes == null || catalog.Biomes.Biomes == null)
            {
                errors.Add("A successfully validated biome catalog is required.");
                return new WorldRegionPlanValidationResult(errors);
            }
            if (plan.Regions == null || plan.Regions.Length < 2)
            {
                errors.Add("Region plan must contain at least two regions.");
                return new WorldRegionPlanValidationResult(errors);
            }

            HashSet<string> expected = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < catalog.Biomes.Biomes.Length; i++)
                if (catalog.Biomes.Biomes[i] != null)
                    expected.Add(catalog.Biomes.Biomes[i].Id);

            HashSet<string> actual = new HashSet<string>(StringComparer.Ordinal);
            HashSet<int> indices = new HashSet<int>();
            const float fullTurn = 6.28318530718f;
            for (int i = 0; i < plan.Regions.Length; i++)
            {
                WorldRegion region = plan.Regions[i];
                if (region == null)
                {
                    errors.Add("Region " + i + " is null.");
                    continue;
                }
                if (region.Index < 0 || !indices.Add(region.Index))
                    errors.Add("Region index is negative or duplicated: " + region.Index + ".");
                if (String.IsNullOrEmpty(region.BiomeId) || !expected.Contains(region.BiomeId))
                    errors.Add("Region " + i + " references an unknown biome: " + region.BiomeId + ".");
                else if (!actual.Add(region.BiomeId))
                    errors.Add("Biome appears in more than one region: " + region.BiomeId + ".");
                if (Single.IsNaN(region.InnerRadius) || Single.IsInfinity(region.InnerRadius) ||
                    Single.IsNaN(region.OuterRadius) || Single.IsInfinity(region.OuterRadius) ||
                    region.InnerRadius < 0f || region.OuterRadius <= region.InnerRadius)
                    errors.Add("Region " + i + " has invalid radius bounds.");
                if (!IsAngle(region.StartAngleRadians, fullTurn) ||
                    !IsAngle(region.EndAngleRadians, fullTurn) ||
                    Math.Abs(region.EndAngleRadians - region.StartAngleRadians) < 0.00001f)
                    errors.Add("Region " + i + " has invalid angular bounds.");
                if (String.IsNullOrEmpty(region.PreviousBiomeId) ||
                    !expected.Contains(region.PreviousBiomeId) ||
                    String.IsNullOrEmpty(region.NextBiomeId) ||
                    !expected.Contains(region.NextBiomeId))
                    errors.Add("Region " + i + " has invalid neighboring biome references.");
            }

            if (actual.Count != expected.Count)
                errors.Add("Region plan does not cover every catalog biome.");

            if (plan.Regions.Length >= 2)
            {
                double totalAngle = 0d;
                for (int i = 0; i < plan.Regions.Length; i++)
                {
                    WorldRegion current = plan.Regions[i];
                    WorldRegion next = plan.Regions[(i + 1) % plan.Regions.Length];
                    if (current == null || next == null)
                        continue;

                    double span = (double)current.EndAngleRadians - current.StartAngleRadians;
                    if (span <= 0d) span += fullTurn;
                    totalAngle += span;

                    double boundaryGap = (double)next.StartAngleRadians - current.EndAngleRadians;
                    if (boundaryGap < 0d) boundaryGap += fullTurn;
                    if (boundaryGap > 0.0001d && fullTurn - boundaryGap > 0.0001d)
                        errors.Add("Region sectors are not contiguous at boundary " + i + ".");
                    if (current.NextBiomeId != next.BiomeId ||
                        next.PreviousBiomeId != current.BiomeId)
                        errors.Add("Region biome adjacency is inconsistent at boundary " + i + ".");
                }
                if (Math.Abs(totalAngle - fullTurn) > 0.001d)
                    errors.Add("Region sectors do not cover exactly one full turn.");
            }
            return new WorldRegionPlanValidationResult(errors);
        }

        private static bool IsAngle(float angle, float fullTurn)
        {
            return !Single.IsNaN(angle) && !Single.IsInfinity(angle) &&
                angle >= 0f && angle < fullTurn;
        }
    }

    public sealed class WorldRegionPlanValidationResult
    {
        public bool Success { get { return Errors.Count == 0; } }
        public IList<string> Errors { get; private set; }

        internal WorldRegionPlanValidationResult(List<string> errors)
        {
            Errors = errors.AsReadOnly();
        }
    }
}
