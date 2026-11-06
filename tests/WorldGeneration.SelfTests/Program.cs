using System;
using System.IO;
using RaftUltimateWorldExpansion;

internal static class Program
{
    private static int assertions;

    private static int Main()
    {
        try
        {
            string catalogPath = Path.Combine(AppContext.BaseDirectory, "Content", "World");
            WorldCatalogLoadResult catalog = WorldCatalog.LoadDirectory(catalogPath);
            Assert(catalog != null && catalog.Success, "world catalogs load and validate");

            const int seed = 741923;
            WorldRegionPlan regionsA = WorldRegionPlanner.Create(catalog, seed, 1000f, 5000f);
            WorldRegionPlan regionsB = WorldRegionPlanner.Create(catalog, seed, 1000f, 5000f);
            Assert(WorldRegionPlanValidator.Validate(regionsA, catalog).Success,
                "generated region plan passes structural validation");
            Assert(regionsA.Regions.Length == regionsB.Regions.Length,
                "same seed creates same region count");
            for (int i = 0; i < regionsA.Regions.Length; i++)
            {
                Assert(regionsA.Regions[i].BiomeId == regionsB.Regions[i].BiomeId,
                    "same seed preserves region order at index " + i);
                Assert(regionsA.Regions[i].StartAngleRadians == regionsB.Regions[i].StartAngleRadians &&
                    regionsA.Regions[i].EndAngleRadians == regionsB.Regions[i].EndAngleRadians,
                    "same seed preserves region boundaries at index " + i);
            }

            for (int b = 0; b < catalog.Biomes.Biomes.Length; b++)
            {
                string biomeId = catalog.Biomes.Biomes[b].Id;
                WorldSpawnPlan first = WorldSpawnPlanner.CreateInRegions(
                    catalog, regionsA, biomeId, seed, 3, 1100f, 4500f, 150f);
                WorldSpawnPlan second = WorldSpawnPlanner.CreateInRegions(
                    catalog, regionsA, biomeId, seed, 3, 1100f, 4500f, 150f);
                Assert(WorldSpawnPlanValidator.Validate(first, 1100f, 4500f).Success,
                    biomeId + " spawn plan passes validation");
                Assert(first.Islands.Length == 3, biomeId + " creates requested island count");
                for (int i = 0; i < first.Islands.Length; i++)
                {
                    IslandSpawnCandidate x = first.Islands[i];
                    IslandSpawnCandidate y = second.Islands[i];
                    Assert(x.Slot == i && y.Slot == i, biomeId + " preserves slot ordering");
                    Assert(x.Island.Id == y.Island.Id, biomeId + " deterministic island selection " + i);
                    Assert(x.X == y.X && x.Z == y.Z,
                        biomeId + " deterministic coordinates " + i);
                    Assert(String.Join(",", x.ResourceNodes) == String.Join(",", y.ResourceNodes),
                        biomeId + " deterministic resource selection " + i);
                }
            }

            Expect<ArgumentException>(() => WorldSpawnPlanner.CreateInRegions(
                catalog, regionsA, catalog.Biomes.Biomes[0].Id, seed + 1, 3, 1100f, 4500f, 150f),
                "region seed mismatch is rejected");
            Expect<ArgumentOutOfRangeException>(() => WorldSpawnPlanner.CreateInRegions(
                catalog, regionsA, catalog.Biomes.Biomes[0].Id, seed, 257, 1100f, 4500f, 150f),
                "island count above 256 is rejected");

            Console.WriteLine("PASS: " + assertions + " assertions");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex);
            return 1;
        }
    }

    private static void Assert(bool condition, string name)
    {
        assertions++;
        if (!condition)
            throw new InvalidOperationException("Assertion failed: " + name);
    }

    private static void Expect<T>(Action action, string name) where T : Exception
    {
        assertions++;
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ": " + name);
    }
}
