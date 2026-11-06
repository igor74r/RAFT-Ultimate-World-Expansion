using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace RaftUltimateWorldExpansion
{
    /// <summary>
    /// Loads and validates UWE-owned world catalog JSON.
    /// This class is deliberately independent from Unity and Raft runtime APIs.
    /// </summary>
    public static class WorldCatalog
    {
        private const int MaxCatalogBytes = 1024 * 1024;

        public static WorldCatalogLoadResult LoadFromEmbeddedFiles(Func<string, byte[]> readFile)
        {
            WorldCatalogLoadResult result = new WorldCatalogLoadResult();
            if (readFile == null)
            {
                result.Errors.Add("Embedded file reader is null.");
                return result;
            }

            try
            {
                result.Biomes = ReadEmbedded<BiomeCatalog>(
                    "Content/World/biomes.json", readFile);
                result.Resources = ReadEmbedded<ResourceCatalog>(
                    "Content/World/resources.json", readFile);
                result.Islands = ReadEmbedded<IslandCatalog>(
                    "Content/World/islands.json", readFile);
                Validate(result);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.GetType().Name + ": " + ex.Message);
            }

            result.Success = result.Errors.Count == 0;
            return result;
        }

        private static T ReadEmbedded<T>(string embeddedPath, Func<string, byte[]> readFile)
            where T : class
        {
            byte[] bytes = readFile(embeddedPath);
            if (bytes == null || bytes.Length == 0 || bytes.Length > MaxCatalogBytes)
                throw new InvalidDataException("Embedded catalog is missing, empty, or too large: " + embeddedPath);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream(bytes, false))
            {
                T value = serializer.ReadObject(stream) as T;
                if (value == null)
                    throw new InvalidDataException("Embedded catalog root is invalid: " + embeddedPath);
                return value;
            }
        }

        public static WorldCatalogLoadResult LoadFromModDirectory(string modDirectory)
        {
            if (String.IsNullOrEmpty(modDirectory))
            {
                WorldCatalogLoadResult invalid = new WorldCatalogLoadResult();
                invalid.Errors.Add("Mod directory is empty.");
                return invalid;
            }

            return LoadDirectory(Path.Combine(modDirectory, "Content", "World"));
        }

        public static WorldCatalogLoadResult LoadDirectory(string directory)
        {
            WorldCatalogLoadResult result = new WorldCatalogLoadResult();
            if (String.IsNullOrEmpty(directory))
            {
                result.Errors.Add("Catalog directory is empty.");
                return result;
            }

            try
            {
                string root = Path.GetFullPath(directory);
                result.Biomes = Read<BiomeCatalog>(Path.Combine(root, "biomes.json"));
                result.Resources = Read<ResourceCatalog>(Path.Combine(root, "resources.json"));
                result.Islands = Read<IslandCatalog>(Path.Combine(root, "islands.json"));
                Validate(result);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.GetType().Name + ": " + ex.Message);
            }

            result.Success = result.Errors.Count == 0;
            return result;
        }

        private static T Read<T>(string path) where T : class
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Required world catalog not found.", path);

            FileInfo info = new FileInfo(path);
            if (info.Length <= 0 || info.Length > MaxCatalogBytes)
                throw new InvalidDataException("Catalog size is outside the allowed range: " + path);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (FileStream stream = File.OpenRead(path))
            {
                T value = serializer.ReadObject(stream) as T;
                if (value == null)
                    throw new InvalidDataException("Catalog root is invalid: " + path);
                return value;
            }
        }

        private static void Validate(WorldCatalogLoadResult result)
        {
            if (result.Biomes == null || result.Resources == null || result.Islands == null)
            {
                result.Errors.Add("One or more catalogs are missing.");
                return;
            }

            ValidateHeader("biomes", result.Biomes.SchemaVersion, result.Biomes.Biomes, result.Errors);
            ValidateHeader("resources", result.Resources.SchemaVersion, result.Resources.Resources, result.Errors);
            ValidateHeader("islands", result.Islands.SchemaVersion, result.Islands.Islands, result.Errors);
            if (result.Biomes.Biomes == null || result.Resources.Resources == null ||
                result.Islands.Islands == null)
                return;

            HashSet<string> biomeIds = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> resourceIds = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < result.Biomes.Biomes.Length; i++)
            {
                BiomeDefinition item = result.Biomes.Biomes[i];
                if (item == null || !AddId(biomeIds, item.Id, "biome", result.Errors))
                    continue;
                if (item.Climate == null)
                {
                    result.Errors.Add("Biome '" + item.Id + "' has no climate definition.");
                }
                else
                {
                    if (String.IsNullOrEmpty(item.Climate.TemperatureBand) ||
                        String.IsNullOrEmpty(item.Climate.WaterDepthBand))
                        result.Errors.Add("Biome '" + item.Id + "' has incomplete climate bands.");
                    ValidateStringArray(item.Climate.WeatherTags, "biome " + item.Id + " climate.weatherTags", result.Errors);
                }
                if (String.IsNullOrEmpty(item.DisplayName))
                    result.Errors.Add("Biome '" + item.Id + "' has no displayName.");
                ValidateStringArray(item.EnvironmentTags, "biome " + item.Id + " environmentTags", result.Errors);
                ValidateStringArray(item.ResourceTags, "biome " + item.Id + " resourceTags", result.Errors);
                ValidateStringArray(item.LandmarkPool, "biome " + item.Id + " landmarkPool", result.Errors);
            }

            for (int i = 0; i < result.Resources.Resources.Length; i++)
            {
                ResourceDefinition item = result.Resources.Resources[i];
                if (item == null || !AddId(resourceIds, item.Id, "resource", result.Errors))
                    continue;
                if (String.IsNullOrEmpty(item.DisplayName) || String.IsNullOrEmpty(item.Category))
                    result.Errors.Add("Resource '" + item.Id + "' is missing displayName or category.");
                ValidateStringArray(item.Biomes, "resource " + item.Id + " biomes", result.Errors);
                ValidateStringArray(item.Sources, "resource " + item.Id + " sources", result.Errors);
                ValidateStringArray(item.Uses, "resource " + item.Id + " uses", result.Errors);
                for (int b = 0; item.Biomes != null && b < item.Biomes.Length; b++)
                    if (!biomeIds.Contains(item.Biomes[b]))
                        result.Errors.Add("Resource '" + item.Id + "' references unknown biome '" + item.Biomes[b] + "'.");
            }

            HashSet<string> islandIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < result.Islands.Islands.Length; i++)
            {
                IslandDefinition item = result.Islands.Islands[i];
                if (item == null)
                {
                    result.Errors.Add("Island entry " + i + " is null.");
                    continue;
                }
                if (!AddId(islandIds, item.Id, "island", result.Errors))
                    continue;
                if (String.IsNullOrEmpty(item.DisplayName) || String.IsNullOrEmpty(item.Archetype))
                    result.Errors.Add("Island '" + item.Id + "' is missing displayName or archetype.");
                if (String.IsNullOrEmpty(item.Biome) || !biomeIds.Contains(item.Biome))
                    result.Errors.Add("Island '" + item.Id + "' references unknown biome '" + item.Biome + "'.");
                ValidateStringArray(item.ResourceNodes, "island " + item.Id + " resourceNodes", result.Errors);
                for (int r = 0; item.ResourceNodes != null && r < item.ResourceNodes.Length; r++)
                    if (!resourceIds.Contains(item.ResourceNodes[r]))
                        result.Errors.Add("Island '" + item.Id + "' references unknown resource '" + item.ResourceNodes[r] + "'.");
            }

            for (int i = 0; i < result.Biomes.Biomes.Length; i++)
            {
                BiomeDefinition item = result.Biomes.Biomes[i];
                if (item == null || item.ResourceTags == null) continue;
                for (int r = 0; r < item.ResourceTags.Length; r++)
                    if (!resourceIds.Contains(item.ResourceTags[r]))
                        result.Errors.Add("Biome '" + item.Id + "' references unknown resource '" + item.ResourceTags[r] + "'.");
            }
        }

        private static void ValidateHeader<T>(string name, int version, T[] entries, List<string> errors)
        {
            if (version != 1)
                errors.Add("Unsupported " + name + " schemaVersion: " + version + ".");
            if (entries == null || entries.Length == 0)
                errors.Add("Catalog '" + name + "' has no entries.");
        }

        private static bool AddId(HashSet<string> ids, string id, string kind, List<string> errors)
        {
            if (String.IsNullOrEmpty(id))
            {
                errors.Add("A " + kind + " entry has no id.");
                return false;
            }
            if (!ids.Add(id))
            {
                errors.Add("Duplicate " + kind + " id: " + id + ".");
                return false;
            }
            return true;
        }

        private static void ValidateStringArray(string[] values, string label, List<string> errors)
        {
            if (values == null)
            {
                errors.Add("Missing array: " + label + ".");
                return;
            }
            for (int i = 0; i < values.Length; i++)
                if (String.IsNullOrEmpty(values[i]))
                    errors.Add("Empty value in " + label + ".");
        }
    }

    public sealed class WorldCatalogLoadResult
    {
        public bool Success { get; internal set; }
        public BiomeCatalog Biomes { get; internal set; }
        public ResourceCatalog Resources { get; internal set; }
        public IslandCatalog Islands { get; internal set; }
        public List<string> Errors { get; private set; }

        public WorldCatalogLoadResult()
        {
            Errors = new List<string>();
        }
    }

    [DataContract]
    public sealed class BiomeCatalog
    {
        [DataMember(Name = "schemaVersion")] public int SchemaVersion;
        [DataMember(Name = "biomes")] public BiomeDefinition[] Biomes;
    }

    [DataContract]
    public sealed class BiomeDefinition
    {
        [DataMember(Name = "id")] public string Id;
        [DataMember(Name = "displayName")] public string DisplayName;
        [DataMember(Name = "climate")] public BiomeClimate Climate;
        [DataMember(Name = "environmentTags")] public string[] EnvironmentTags;
        [DataMember(Name = "resourceTags")] public string[] ResourceTags;
        [DataMember(Name = "landmarkPool")] public string[] LandmarkPool;
    }

    [DataContract]
    public sealed class BiomeClimate
    {
        [DataMember(Name = "temperatureBand")] public string TemperatureBand;
        [DataMember(Name = "waterDepthBand")] public string WaterDepthBand;
        [DataMember(Name = "weatherTags")] public string[] WeatherTags;
    }

    [DataContract]
    public sealed class ResourceCatalog
    {
        [DataMember(Name = "schemaVersion")] public int SchemaVersion;
        [DataMember(Name = "resources")] public ResourceDefinition[] Resources;
    }

    [DataContract]
    public sealed class ResourceDefinition
    {
        [DataMember(Name = "id")] public string Id;
        [DataMember(Name = "displayName")] public string DisplayName;
        [DataMember(Name = "category")] public string Category;
        [DataMember(Name = "biomes")] public string[] Biomes;
        [DataMember(Name = "sources")] public string[] Sources;
        [DataMember(Name = "uses")] public string[] Uses;
    }

    [DataContract]
    public sealed class IslandCatalog
    {
        [DataMember(Name = "schemaVersion")] public int SchemaVersion;
        [DataMember(Name = "islands")] public IslandDefinition[] Islands;
    }

    [DataContract]
    public sealed class IslandDefinition
    {
        [DataMember(Name = "id")] public string Id;
        [DataMember(Name = "biome")] public string Biome;
        [DataMember(Name = "displayName")] public string DisplayName;
        [DataMember(Name = "archetype")] public string Archetype;
        [DataMember(Name = "resourceNodes")] public string[] ResourceNodes;
    }
}
