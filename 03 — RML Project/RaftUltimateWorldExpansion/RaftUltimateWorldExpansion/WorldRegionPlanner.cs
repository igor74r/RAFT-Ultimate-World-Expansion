using System;
using System.Collections.Generic;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Builds a deterministic, data-only regional climate map.
    /// Regions are angular sectors around the origin; neighboring sectors
    /// expose transition candidates without claiming runtime biome registration.
    /// </summary>
    public static class WorldRegionPlanner
    {
        public static WorldRegionPlan Create(WorldCatalogLoadResult catalog, int worldSeed,
            float innerRadius, float outerRadius)
        {
            if (catalog == null || !catalog.Success)
                throw new ArgumentException("A successfully validated world catalog is required.", "catalog");
            if (catalog.Biomes == null || catalog.Biomes.Biomes == null ||
                catalog.Biomes.Biomes.Length < 2)
                throw new InvalidOperationException("At least two biomes are required to build regional transitions.");
            if (Single.IsNaN(innerRadius) || Single.IsInfinity(innerRadius) ||
                Single.IsNaN(outerRadius) || Single.IsInfinity(outerRadius) ||
                innerRadius < 0f || outerRadius <= innerRadius)
                throw new ArgumentOutOfRangeException("innerRadius", "Region radii must be finite, non-negative, and outerRadius must exceed innerRadius.");

            List<BiomeDefinition> biomes = new List<BiomeDefinition>();
            for (int i = 0; i < catalog.Biomes.Biomes.Length; i++)
            {
                BiomeDefinition biome = catalog.Biomes.Biomes[i];
                if (biome != null)
                    biomes.Add(biome);
            }
            if (biomes.Count < 2)
                throw new InvalidOperationException("At least two valid biome definitions are required.");

            uint state = MakeSeed(worldSeed);
            for (int i = biomes.Count - 1; i > 0; i--)
            {
                int j = (int)(Next(ref state) % (uint)(i + 1));
                BiomeDefinition temporary = biomes[i];
                biomes[i] = biomes[j];
                biomes[j] = temporary;
            }

            float sectorSize = 6.28318530718f / biomes.Count;
            float rotation = NextUnit(ref state) * sectorSize;
            WorldRegion[] regions = new WorldRegion[biomes.Count];

            for (int i = 0; i < biomes.Count; i++)
            {
                int previous = (i + biomes.Count - 1) % biomes.Count;
                int next = (i + 1) % biomes.Count;
                float start = rotation + sectorSize * i;
                regions[i] = new WorldRegion(i, biomes[i].Id, innerRadius, outerRadius,
                    NormalizeAngle(start), NormalizeAngle(start + sectorSize),
                    biomes[previous].Id, biomes[next].Id);
            }

            return new WorldRegionPlan(worldSeed, regions);
        }

        private static float NormalizeAngle(float angle)
        {
            const float fullTurn = 6.28318530718f;
            while (angle < 0f) angle += fullTurn;
            while (angle >= fullTurn) angle -= fullTurn;
            return angle;
        }

        private static uint MakeSeed(int seed)
        {
            unchecked
            {
                uint value = 2166136261;
                value ^= (uint)seed;
                value *= 16777619;
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

    public sealed class WorldRegionPlan
    {
        public int WorldSeed { get; private set; }
        public WorldRegion[] Regions { get; private set; }

        internal WorldRegionPlan(int worldSeed, WorldRegion[] regions)
        {
            WorldSeed = worldSeed;
            Regions = regions;
        }
    }

    public sealed class WorldRegion
    {
        public int Index { get; private set; }
        public string BiomeId { get; private set; }
        public float InnerRadius { get; private set; }
        public float OuterRadius { get; private set; }
        public float StartAngleRadians { get; private set; }
        public float EndAngleRadians { get; private set; }
        public string PreviousBiomeId { get; private set; }
        public string NextBiomeId { get; private set; }

        internal WorldRegion(int index, string biomeId, float innerRadius, float outerRadius,
            float startAngle, float endAngle, string previousBiomeId, string nextBiomeId)
        {
            Index = index;
            BiomeId = biomeId;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            StartAngleRadians = startAngle;
            EndAngleRadians = endAngle;
            PreviousBiomeId = previousBiomeId;
            NextBiomeId = nextBiomeId;
        }
    }
}
