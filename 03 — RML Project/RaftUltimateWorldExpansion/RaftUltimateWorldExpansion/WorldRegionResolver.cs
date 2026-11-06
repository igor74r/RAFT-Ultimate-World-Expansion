using System;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Resolves a world coordinate to a region and optional neighboring-biome transition.
    /// Pure data logic; it does not alter game terrain, chunks, or runtime biome state.
    /// </summary>
    public static class WorldRegionResolver
    {
        public static WorldRegionSelection Resolve(WorldRegionPlan plan, float x, float z,
            float transitionWidthRadians)
        {
            if (plan == null || plan.Regions == null || plan.Regions.Length == 0)
                throw new ArgumentException("A non-empty region plan is required.", "plan");
            if (Single.IsNaN(x) || Single.IsInfinity(x) ||
                Single.IsNaN(z) || Single.IsInfinity(z))
                throw new ArgumentOutOfRangeException("x", "World coordinates must be finite.");
            if (Single.IsNaN(transitionWidthRadians) || Single.IsInfinity(transitionWidthRadians) ||
                transitionWidthRadians < 0f || transitionWidthRadians >= 1.57079632679f)
                throw new ArgumentOutOfRangeException("transitionWidthRadians",
                    "Transition width must be finite and between zero and PI/2.");
            float maximumWidth = GetMaximumNonOverlappingTransitionWidth(plan);
            if (transitionWidthRadians > maximumWidth + 0.000001f)
                throw new ArgumentOutOfRangeException("transitionWidthRadians",
                    "Transition width cannot exceed half the narrowest region sector.");

            float radius = (float)Math.Sqrt((double)x * x + (double)z * z);
            float angle = NormalizeAngle((float)Math.Atan2(z, x));
            WorldRegion selected = null;

            for (int i = 0; i < plan.Regions.Length; i++)
            {
                WorldRegion candidate = plan.Regions[i];
                if (candidate == null || radius < candidate.InnerRadius ||
                    radius > candidate.OuterRadius)
                    continue;
                if (ContainsAngle(angle, candidate.StartAngleRadians, candidate.EndAngleRadians))
                {
                    selected = candidate;
                    break;
                }
            }

            if (selected == null)
                return new WorldRegionSelection(null, null, 0f, radius, angle, false);

            if (transitionWidthRadians <= 0f)
                return new WorldRegionSelection(selected, null, 0f, radius, angle, true);

            float fromStart = AngularDistance(angle, selected.StartAngleRadians);
            float fromEnd = AngularDistance(angle, selected.EndAngleRadians);
            if (fromStart <= transitionWidthRadians)
            {
                float blend = 1f - fromStart / transitionWidthRadians;
                return new WorldRegionSelection(selected, selected.PreviousBiomeId,
                    Clamp01(blend), radius, angle, true);
            }
            if (fromEnd <= transitionWidthRadians)
            {
                float blend = 1f - fromEnd / transitionWidthRadians;
                return new WorldRegionSelection(selected, selected.NextBiomeId,
                    Clamp01(blend), radius, angle, true);
            }

            return new WorldRegionSelection(selected, null, 0f, radius, angle, true);
        }

        public static float GetMaximumNonOverlappingTransitionWidth(WorldRegionPlan plan)
        {
            if (plan == null || plan.Regions == null || plan.Regions.Length == 0)
                throw new ArgumentException("A non-empty region plan is required.", "plan");

            const double fullTurn = 6.28318530718d;
            double minimumSpan = fullTurn;
            for (int i = 0; i < plan.Regions.Length; i++)
            {
                WorldRegion region = plan.Regions[i];
                if (region == null ||
                    Single.IsNaN(region.StartAngleRadians) || Single.IsInfinity(region.StartAngleRadians) ||
                    Single.IsNaN(region.EndAngleRadians) || Single.IsInfinity(region.EndAngleRadians))
                    throw new ArgumentException("Region sectors must have finite angular bounds.", "plan");

                double span = (double)region.EndAngleRadians - region.StartAngleRadians;
                if (span <= 0d) span += fullTurn;
                if (span <= 0d || span >= fullTurn)
                    throw new ArgumentException("Region sector angular span is invalid.", "plan");
                if (span < minimumSpan) minimumSpan = span;
            }
            return (float)(minimumSpan * 0.5d);
        }

        private static bool ContainsAngle(float angle, float start, float end)
        {
            if (start <= end)
                return angle >= start && angle < end;
            return angle >= start || angle < end;
        }

        private static float AngularDistance(float first, float second)
        {
            float difference = Math.Abs(first - second);
            const float fullTurn = 6.28318530718f;
            return difference > fullTurn - difference ? fullTurn - difference : difference;
        }

        private static float NormalizeAngle(float angle)
        {
            const float fullTurn = 6.28318530718f;
            while (angle < 0f) angle += fullTurn;
            while (angle >= fullTurn) angle -= fullTurn;
            return angle;
        }

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }

    public sealed class WorldRegionSelection
    {
        public WorldRegion Region { get; private set; }
        public string PrimaryBiomeId { get { return Region == null ? null : Region.BiomeId; } }
        public string TransitionBiomeId { get; private set; }
        public float TransitionWeight { get; private set; }
        public float Radius { get; private set; }
        public float AngleRadians { get; private set; }
        public bool IsInsideRegion { get; private set; }
        public bool IsTransition { get { return !String.IsNullOrEmpty(TransitionBiomeId); } }

        internal WorldRegionSelection(WorldRegion region, string transitionBiomeId,
            float transitionWeight, float radius, float angleRadians, bool isInsideRegion)
        {
            Region = region;
            TransitionBiomeId = transitionBiomeId;
            TransitionWeight = transitionWeight;
            Radius = radius;
            AngleRadians = angleRadians;
            IsInsideRegion = isInsideRegion;
        }
    }
}
