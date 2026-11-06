using System;
using System.Collections.Generic;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Converts planned world coordinates into a configurable chunk-grid address.
    /// The grid size is supplied by the caller; this class does not assume Raft's
    /// runtime ChunkSize or invoke ChunkManager.
    /// </summary>
    public static class WorldChunkPlacementPlanner
    {
        public static WorldChunkPlacementPlan Create(
            WorldSpawnPlan[] islandPlans,
            WorldResourceDistributionPlan[] resourcePlans,
            float chunkSize)
        {
            if (Single.IsNaN(chunkSize) || Single.IsInfinity(chunkSize) || chunkSize <= 0f)
                throw new ArgumentOutOfRangeException("chunkSize");

            List<WorldChunkPlacement> placements = new List<WorldChunkPlacement>();
            HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);

            if (islandPlans != null)
            {
                for (int p = 0; p < islandPlans.Length; p++)
                {
                    WorldSpawnPlan plan = islandPlans[p];
                    if (plan == null || plan.Islands == null)
                        throw new ArgumentException("Island plans cannot contain null plans or candidate arrays.", "islandPlans");
                    for (int i = 0; i < plan.Islands.Length; i++)
                    {
                        IslandSpawnCandidate island = plan.Islands[i];
                        if (island == null || island.Island == null)
                            throw new ArgumentException("Island plans cannot contain null candidates.", "islandPlans");
                        string identity = "island:" + plan.BiomeId + ":" + island.Slot;
                        if (!identities.Add(identity))
                            throw new InvalidOperationException("Duplicate placement identity: " + identity);
                        placements.Add(CreatePlacement(identity, WorldPlacementKind.Island,
                            island.Slot, island.Island.Id, island.X, island.Z, chunkSize));
                    }
                }
            }

            if (resourcePlans != null)
            {
                for (int p = 0; p < resourcePlans.Length; p++)
                {
                    WorldResourceDistributionPlan plan = resourcePlans[p];
                    if (plan == null || plan.Nodes == null)
                        throw new ArgumentException("Resource plans cannot contain null plans or node arrays.", "resourcePlans");
                    for (int i = 0; i < plan.Nodes.Length; i++)
                    {
                        WorldResourceNode node = plan.Nodes[i];
                        if (node == null)
                            throw new ArgumentException("Resource plans cannot contain null nodes.", "resourcePlans");
                        string identity = "resource:" + node.PrimaryBiomeId + ":" + node.IslandSlot + ":" + node.ResourceId;
                        if (!identities.Add(identity))
                            throw new InvalidOperationException("Duplicate placement identity: " + identity);
                        placements.Add(CreatePlacement(identity, WorldPlacementKind.Resource,
                            node.IslandSlot, node.ResourceId, node.X, node.Z, chunkSize));
                    }
                }
            }

            return new WorldChunkPlacementPlan(chunkSize, placements.ToArray());
        }

        private static WorldChunkPlacement CreatePlacement(string identity,
            WorldPlacementKind kind, int ownerSlot, string definitionId,
            float x, float z, float chunkSize)
        {
            if (Single.IsNaN(x) || Single.IsInfinity(x) ||
                Single.IsNaN(z) || Single.IsInfinity(z))
                throw new ArgumentOutOfRangeException("x", "Placement coordinates must be finite.");

            double chunkX = Math.Floor((double)x / chunkSize);
            double chunkZ = Math.Floor((double)z / chunkSize);
            if (chunkX < Int32.MinValue || chunkX > Int32.MaxValue ||
                chunkZ < Int32.MinValue || chunkZ > Int32.MaxValue)
                throw new ArgumentOutOfRangeException("x", "Chunk coordinate exceeds Int32 range.");

            int indexX = (int)chunkX;
            int indexZ = (int)chunkZ;
            float localX = (float)((double)x - chunkX * chunkSize);
            float localZ = (float)((double)z - chunkZ * chunkSize);

            return new WorldChunkPlacement(identity, kind, ownerSlot, definitionId,
                x, z, indexX, indexZ, localX, localZ);
        }
    }

    public enum WorldPlacementKind
    {
        Island = 0,
        Resource = 1
    }

    public sealed class WorldChunkPlacementPlan
    {
        public float ChunkSize { get; private set; }
        public WorldChunkPlacement[] Placements { get; private set; }

        internal WorldChunkPlacementPlan(float chunkSize, WorldChunkPlacement[] placements)
        {
            ChunkSize = chunkSize;
            Placements = placements;
        }

        /// <summary>
        /// Returns placements assigned to one chunk, preserving deterministic plan order.
        /// </summary>
        public WorldChunkPlacement[] GetChunk(int chunkX, int chunkZ)
        {
            if (Placements == null)
                return new WorldChunkPlacement[0];

            List<WorldChunkPlacement> matches = new List<WorldChunkPlacement>();
            for (int i = 0; i < Placements.Length; i++)
            {
                WorldChunkPlacement placement = Placements[i];
                if (placement != null && placement.ChunkX == chunkX && placement.ChunkZ == chunkZ)
                    matches.Add(placement);
            }
            return matches.ToArray();
        }
    }

    public sealed class WorldChunkPlacement
    {
        public string Identity { get; private set; }
        public WorldPlacementKind Kind { get; private set; }
        public int OwnerIslandSlot { get; private set; }
        public string DefinitionId { get; private set; }
        public float WorldX { get; private set; }
        public float WorldZ { get; private set; }
        public int ChunkX { get; private set; }
        public int ChunkZ { get; private set; }
        public float LocalX { get; private set; }
        public float LocalZ { get; private set; }

        internal WorldChunkPlacement(string identity, WorldPlacementKind kind,
            int ownerIslandSlot, string definitionId, float worldX, float worldZ,
            int chunkX, int chunkZ, float localX, float localZ)
        {
            Identity = identity;
            Kind = kind;
            OwnerIslandSlot = ownerIslandSlot;
            DefinitionId = definitionId;
            WorldX = worldX;
            WorldZ = worldZ;
            ChunkX = chunkX;
            ChunkZ = chunkZ;
            LocalX = localX;
            LocalZ = localZ;
        }
    }

    public sealed class WorldChunkPlacementValidationResult
    {
        public bool Success { get { return Errors.Count == 0; } }
        public IList<string> Errors { get; private set; }

        internal WorldChunkPlacementValidationResult(List<string> errors)
        {
            Errors = errors.AsReadOnly();
        }
    }

    public static class WorldChunkPlacementValidator
    {
        public static WorldChunkPlacementValidationResult Validate(WorldChunkPlacementPlan plan)
        {
            List<string> errors = new List<string>();
            if (plan == null || plan.Placements == null)
            {
                errors.Add("Chunk placement plan and placement array are required.");
                return new WorldChunkPlacementValidationResult(errors);
            }
            if (Single.IsNaN(plan.ChunkSize) || Single.IsInfinity(plan.ChunkSize) || plan.ChunkSize <= 0f)
                errors.Add("Chunk size must be finite and positive.");

            HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < plan.Placements.Length; i++)
            {
                WorldChunkPlacement placement = plan.Placements[i];
                if (placement == null)
                {
                    errors.Add("Placement " + i + " is null.");
                    continue;
                }
                if (String.IsNullOrEmpty(placement.Identity) || !identities.Add(placement.Identity))
                    errors.Add("Placement " + i + " has an empty or duplicate identity.");
                if (String.IsNullOrEmpty(placement.DefinitionId) || placement.OwnerIslandSlot < 0)
                    errors.Add("Placement " + i + " has invalid ownership or definition identity.");
                if (placement.Kind != WorldPlacementKind.Island &&
                    placement.Kind != WorldPlacementKind.Resource)
                    errors.Add("Placement " + i + " has an unknown placement kind.");
                if (Single.IsNaN(placement.WorldX) || Single.IsInfinity(placement.WorldX) ||
                    Single.IsNaN(placement.WorldZ) || Single.IsInfinity(placement.WorldZ) ||
                    Single.IsNaN(placement.LocalX) || Single.IsInfinity(placement.LocalX) ||
                    Single.IsNaN(placement.LocalZ) || Single.IsInfinity(placement.LocalZ))
                {
                    errors.Add("Placement " + i + " contains non-finite coordinates.");
                    continue;
                }
                if (placement.LocalX < 0f || placement.LocalX >= plan.ChunkSize ||
                    placement.LocalZ < 0f || placement.LocalZ >= plan.ChunkSize)
                    errors.Add("Placement " + i + " has local coordinates outside its chunk.");
                if (plan.ChunkSize > 0f &&
                    (!Single.IsNaN(plan.ChunkSize) && !Single.IsInfinity(plan.ChunkSize)))
                {
                    int expectedX = (int)Math.Floor((double)placement.WorldX / plan.ChunkSize);
                    int expectedZ = (int)Math.Floor((double)placement.WorldZ / plan.ChunkSize);
                    if (expectedX != placement.ChunkX || expectedZ != placement.ChunkZ)
                        errors.Add("Placement " + i + " has inconsistent chunk indices.");
                    else
                    {
                        double reconstructedX = (double)placement.ChunkX * plan.ChunkSize + placement.LocalX;
                        double reconstructedZ = (double)placement.ChunkZ * plan.ChunkSize + placement.LocalZ;
                        if (Math.Abs(reconstructedX - placement.WorldX) > 0.02d ||
                            Math.Abs(reconstructedZ - placement.WorldZ) > 0.02d)
                            errors.Add("Placement " + i + " cannot be reconstructed from its chunk address.");
                    }
                }
            }
            return new WorldChunkPlacementValidationResult(errors);
        }
    }
}
