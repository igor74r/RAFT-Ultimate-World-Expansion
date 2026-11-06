using System;
using System.Collections.Generic;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Validates data-only spawn plans before any future game integration.
    /// This validator does not inspect or mutate Raft world state.
    /// </summary>
    public static class WorldSpawnPlanValidator
    {
        public static WorldSpawnPlanValidationResult Validate(
            WorldSpawnPlan plan, float minRadius, float maxRadius)
        {
            List<string> errors = new List<string>();
            if (plan == null)
            {
                errors.Add("Spawn plan is null.");
                return new WorldSpawnPlanValidationResult(errors);
            }

            if (String.IsNullOrEmpty(plan.BiomeId))
                errors.Add("Spawn plan has no biome ID.");
            if (plan.Islands == null || plan.Islands.Length == 0)
                errors.Add("Spawn plan contains no island candidates.");
            else if (plan.Islands.Length > 256)
                errors.Add("Spawn plan exceeds the maximum of 256 island candidates.");
            if (Single.IsNaN(plan.MinimumSeparation) || Single.IsInfinity(plan.MinimumSeparation) ||
                plan.MinimumSeparation < 0f)
                errors.Add("Spawn plan minimum separation is invalid.");
            if (Single.IsNaN(minRadius) || Single.IsInfinity(minRadius) ||
                Single.IsNaN(maxRadius) || Single.IsInfinity(maxRadius) ||
                minRadius < 0f || maxRadius <= minRadius)
                errors.Add("Validation radius bounds are invalid.");

            if (plan.Islands != null)
            {
                HashSet<int> slots = new HashSet<int>();
                for (int i = 0; i < plan.Islands.Length; i++)
                {
                    IslandSpawnCandidate candidate = plan.Islands[i];
                    if (candidate == null)
                    {
                        errors.Add("Candidate " + i + " is null.");
                        continue;
                    }

                    if (candidate.Slot < 0 || !slots.Add(candidate.Slot))
                        errors.Add("Candidate slot is negative or duplicated: " + candidate.Slot + ".");
                    if (candidate.Slot != i)
                        errors.Add("Candidate slot does not match its deterministic plan position: index " +
                            i + ", slot " + candidate.Slot + ".");
                    if (candidate.Island == null)
                        errors.Add("Candidate " + i + " has no island definition.");
                    else
                    {
                        if (String.IsNullOrEmpty(candidate.Island.Id))
                            errors.Add("Candidate " + i + " has an island definition without an ID.");
                        if (!String.Equals(candidate.Island.Biome, plan.BiomeId, StringComparison.Ordinal))
                            errors.Add("Candidate " + i + " belongs to a different biome.");
                    }

                    if (Single.IsNaN(candidate.X) || Single.IsInfinity(candidate.X) ||
                        Single.IsNaN(candidate.Z) || Single.IsInfinity(candidate.Z))
                    {
                        errors.Add("Candidate " + i + " has non-finite coordinates.");
                    }
                    else if (maxRadius > minRadius)
                    {
                        double radiusSquared = (double)candidate.X * candidate.X +
                            (double)candidate.Z * candidate.Z;
                        double minSquared = (double)minRadius * minRadius;
                        double maxSquared = (double)maxRadius * maxRadius;
                        if (radiusSquared < minSquared - 0.01d ||
                            radiusSquared > maxSquared + 0.01d)
                            errors.Add("Candidate " + i + " is outside configured radius bounds.");
                    }

                    if (candidate.ResourceNodes == null)
                    {
                        errors.Add("Candidate " + i + " has no resource-node list.");
                    }
                    else
                    {
                        HashSet<string> resourceIds = new HashSet<string>(StringComparer.Ordinal);
                        for (int r = 0; r < candidate.ResourceNodes.Length; r++)
                        {
                            string id = candidate.ResourceNodes[r];
                            if (String.IsNullOrEmpty(id) || !resourceIds.Add(id))
                                errors.Add("Candidate " + i + " has an empty or duplicate resource ID.");
                            if (candidate.Island != null &&
                                !Contains(candidate.Island.ResourceNodes, id))
                                errors.Add("Candidate " + i + " includes a resource not declared by its island: " + id + ".");
                        }
                    }
                }

                if (!Single.IsNaN(plan.MinimumSeparation) &&
                    !Single.IsInfinity(plan.MinimumSeparation) && plan.MinimumSeparation >= 0f)
                {
                    double minimumSquared = (double)plan.MinimumSeparation * plan.MinimumSeparation;
                    for (int a = 0; a < plan.Islands.Length; a++)
                    {
                        IslandSpawnCandidate first = plan.Islands[a];
                        if (first == null || Single.IsNaN(first.X) || Single.IsInfinity(first.X) ||
                            Single.IsNaN(first.Z) || Single.IsInfinity(first.Z))
                            continue;
                        for (int b = a + 1; b < plan.Islands.Length; b++)
                        {
                            IslandSpawnCandidate second = plan.Islands[b];
                            if (second == null || Single.IsNaN(second.X) || Single.IsInfinity(second.X) ||
                                Single.IsNaN(second.Z) || Single.IsInfinity(second.Z))
                                continue;
                            double dx = (double)first.X - second.X;
                            double dz = (double)first.Z - second.Z;
                            if (dx * dx + dz * dz < minimumSquared - 0.01d)
                                errors.Add("Candidates " + a + " and " + b +
                                    " violate minimum separation " + plan.MinimumSeparation + ".");
                        }
                    }
                }
            }

            return new WorldSpawnPlanValidationResult(errors);
        }

        private static bool Contains(string[] values, string expected)
        {
            if (values == null || expected == null)
                return false;
            for (int i = 0; i < values.Length; i++)
                if (String.Equals(values[i], expected, StringComparison.Ordinal))
                    return true;
            return false;
        }
    }

    public sealed class WorldSpawnPlanValidationResult
    {
        public bool Success { get { return Errors.Count == 0; } }
        public IList<string> Errors { get; private set; }

        internal WorldSpawnPlanValidationResult(List<string> errors)
        {
            Errors = errors.AsReadOnly();
        }
    }
}
