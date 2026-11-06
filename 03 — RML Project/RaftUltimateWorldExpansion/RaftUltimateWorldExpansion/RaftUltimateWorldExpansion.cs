[Reading 404 lines from start (total: 404 lines, 0 remaining)]

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using HMLLibrary;

/// <summary>
/// Read-only runtime diagnostics for Raft: Ultimate World Expansion.
/// This module never invokes game spawn, bundle-load, save-write, or network methods.
/// </summary>
public class RaftUltimateWorldExpansion : Mod
{
    private bool initialized;
    private bool worldProbeCompleted;
    private bool worldCatalogLoadCompleted;

    private const int MaxDictionaryKeys = 8;
    private const int MaxChunkPointsPerKey = 12;
    private const int MaxSpawners = 20;
    private const int MaxFieldsPerObject = 18;

    public void Start()
    {
        if (initialized)
            return;

        initialized = true;
        JsonModInfo info = GetModInfo();
        Log("Core initialized: " + info.name + " v" + info.version);
        Log("World diagnostics mode: READ-ONLY v0.1.2.");
        LoadWorldCatalogs();
    }

    private void LoadWorldCatalogs()
    {
        if (worldCatalogLoadCompleted)
            return;

        worldCatalogLoadCompleted = true;
        Log("World catalog load started (read-only content validation).");

        try
        {
            RaftUltimateWorldExpansion.WorldCatalogLoadResult result =
                RaftUltimateWorldExpansion.WorldCatalog.LoadFromEmbeddedFiles(GetEmbeddedFileBytes);

            if (result.Success)
            {
                Log("World catalogs loaded and validated: biomes=" +
                    result.Biomes.Biomes.Length + ", resources=" +
                    result.Resources.Resources.Length + ", islands=" +
                    result.Islands.Islands.Length + ".");

                WorldRegionPlan regionPlan = WorldRegionPlanner.Create(result, 0, 1000f, 5000f);
                WorldRegionPlanValidationResult regionValidation = WorldRegionPlanValidator.Validate(regionPlan, result);
                Log("Region map validation: " + (regionValidation.Success ? "PASS" : "FAIL") + ", errors=" + regionValidation.Errors.Count + ".");
                Log("World region plan: seed=" + regionPlan.WorldSeed +
                    ", regions=" + regionPlan.Regions.Length +
                    " (data-only angular biome sectors; no world mutation).");
                for (int regionIndex = 0; regionIndex < regionPlan.Regions.Length; regionIndex++)
                {
                    WorldRegion region = regionPlan.Regions[regionIndex];
                    Log("World region[" + region.Index + "]: biome=" + region.BiomeId +
                        ", radius=" + region.InnerRadius + "-" + region.OuterRadius +
                        ", neighbors=" + region.PreviousBiomeId + "/" + region.NextBiomeId + ".");
                }

                for (int sampleIndex = 0; sampleIndex < regionPlan.Regions.Length; sampleIndex++)
                {
                    WorldRegion sampleRegion = regionPlan.Regions[sampleIndex];
                    float sampleAngle = sampleRegion.StartAngleRadians +
                        (sampleRegion.EndAngleRadians >= sampleRegion.StartAngleRadians
                            ? sampleRegion.EndAngleRadians - sampleRegion.StartAngleRadians
                            : 6.28318530718f - sampleRegion.StartAngleRadians + sampleRegion.EndAngleRadians) * 0.5f;
                    float sampleRadius = (sampleRegion.InnerRadius + sampleRegion.OuterRadius) * 0.5f;
                    WorldRegionSelection sample = WorldRegionResolver.Resolve(regionPlan,
                        (float)Math.Cos(sampleAngle) * sampleRadius,
                        (float)Math.Sin(sampleAngle) * sampleRadius, 0.05f);
                    Log("Region coordinate probe: expected=" + sampleRegion.BiomeId +
                        ", resolved=" + sample.PrimaryBiomeId +
                        ", transition=" + sample.IsTransition + ".");
                }

                for (int i = 0; i < result.Biomes.Biomes.Length; i++)
                {
                    BiomeDefinition biome = result.Biomes.Biomes[i];
                    if (biome == null)
                        continue;

                    BiomeResolution resolution = WorldBiomeResolver.Resolve(result, biome.Id, 0);
                    Log("Biome resolver check: " + biome.Id +
                        " -> island=" + resolution.SelectedIsland.Id +
                        ", candidates=" + resolution.IslandCandidates.Length +
                        ", resources=" + resolution.Resources.Length +
                        " (deterministic seed 0; no world placement).");

                    WorldSpawnPlan plan = WorldSpawnPlanner.CreateInRegions(result, regionPlan, biome.Id, 0, 3, 1100f, 4500f, 150f);
                    Log("World spawn plan: biome=" + plan.BiomeId + ", seed=" + plan.WorldSeed +
                        ", candidates=" + plan.Islands.Length + " (data-only; no game objects created).");
                    WorldSpawnPlanValidationResult validation =
                        WorldSpawnPlanValidator.Validate(plan, 1100f, 4500f);
                    Log("World spawn plan validation: " + (validation.Success ? "PASS" : "FAIL") +
                        ", errors=" + validation.Errors.Count + ".");
                    for (int e = 0; e < Math.Min(validation.Errors.Count, 4); e++)
                        Log("World spawn plan validation error[" + e + "]: " + validation.Errors[e]);
                    WorldResourceDistributionPlan resourcePlan =
                        WorldResourceDistributor.Create(result, regionPlan, plan.Islands, 0, 3, 35f, 0.05f);
                    WorldResourceDistributionValidationResult resourceValidation =
                        WorldResourceDistributionValidator.Validate(resourcePlan, plan.Islands, result, 3);
                    Log("Resource distribution: biome=" + biome.Id +
                        ", nodes=" + resourcePlan.Nodes.Length +
                        ", validation=" + (resourceValidation.Success ? "PASS" : "FAIL") +
                        ", errors=" + resourceValidation.Errors.Count +
                        " (deterministic data-only placements).");
                    for (int re = 0; re < Math.Min(resourceValidation.Errors.Count, 4); re++)
                        Log("Resource distribution error[" + re + "]: " + resourceValidation.Errors[re]);
                    const float auditReferenceChunkSize = 1000f;
                    WorldChunkPlacementPlan chunkPlan = WorldChunkPlacementPlanner.Create(
                        new WorldSpawnPlan[] { plan },
                        new WorldResourceDistributionPlan[] { resourcePlan },
                        auditReferenceChunkSize);
                    WorldChunkPlacementValidationResult chunkValidation =
                        WorldChunkPlacementValidator.Validate(chunkPlan);
                    WorldGenerationPlanValidationResult generationValidation =
                        WorldGenerationPlanValidator.Validate(result, regionPlan,
                            new WorldSpawnPlan[] { plan },
                            new WorldResourceDistributionPlan[] { resourcePlan },
                            chunkPlan);
                    Log("Complete generation validation: " +
                        (generationValidation.Success ? "PASS" : "FAIL") +
                        ", errors=" + generationValidation.Errors.Count + ".");
                    for (int ge = 0; ge < Math.Min(generationValidation.Errors.Count, 4); ge++)
                        Log("Generation validation error[" + ge + "]: " +
                            generationValidation.Errors[ge]);
                    Log("Chunk coordinate plan: biome=" + biome.Id +
                        ", placements=" + chunkPlan.Placements.Length +
                        ", validation=" + (chunkValidation.Success ? "PASS" : "FAIL") +
                        ", referenceChunkSize=" + auditReferenceChunkSize +
                        " (prior v0.1.1 runtime observation; not read from live ChunkManager; data-only).");
                    for (int ce = 0; ce < Math.Min(chunkValidation.Errors.Count, 4); ce++)
                        Log("Chunk placement error[" + ce + "]: " + chunkValidation.Errors[ce]);
                    for (int p = 0; p < plan.Islands.Length; p++)
                    {
                        IslandSpawnCandidate candidate = plan.Islands[p];
                        Log("Spawn candidate[" + candidate.Slot + "]: island=" + candidate.Island.Id +
                            ", x=" + candidate.X.ToString("F1") + ", z=" + candidate.Z.ToString("F1") +
                            ", resourceNodes=" + candidate.ResourceNodes.Length + ".");
                    }
                }
                return;
            }

            Log("World catalog validation FAILED; catalogs will not be used. errors=" +
                result.Errors.Count + ".");
            int count = Math.Min(result.Errors.Count, 8);
            for (int i = 0; i < count; i++)
                Log("World catalog error[" + i + "]: " + result.Errors[i]);
            if (result.Errors.Count > count)
                Log("World catalog errors truncated in log.");
        }
        catch (Exception ex)
        {
            Log("World catalog load failed safely: " +
                ex.GetType().Name + ": " + ex.Message);
        }
    }

    public override void WorldEvent_WorldLoaded()
    {
        Log("World loaded.");
        ProbeWorldManagers();
    }

    public override void WorldEvent_WorldSaved()
    {
        Log("World save event received.");
    }

    private void ProbeWorldManagers()
    {
        if (worldProbeCompleted)
            return;

        worldProbeCompleted = true;
        Log("World probe v0.1.2 started (read-only; bounded enumeration).");

        object chunkManager = InspectManager("ChunkManager",
            "numberOfChunks", "currentSpawners", "allChunkPointRules",
            "allChunkSpawners", "chunks", "chunkPoints", "ChunkSize");

        if (chunkManager != null)
        {
            InspectChunkPointDictionary(chunkManager);
            InspectSpawnerList(chunkManager);
        }

        InspectManager("SceneLoader", "loadedLandmarks", "landmarks", "landmarkScenes");
        InspectTypeSurface("ChunkPoint");
        InspectTypeSurface("ObjectSpawner_Chunk");
        InspectTypeSurface("SO_ChunkSpawnRuleAsset");
        InspectTypeSurface("AssetBundleManager");
        InspectTypeSurface("AssetBundleHooker");
        InspectLandmarkPipelineSurface("AssetBundleManager", "assetBundleTag", "fileDirectory", "bundleLandmark", "asyncedLandmarks", "LoadAssetBundle", "AddLandmarksToHooker", "OnLoadGame", "StartSceneLoading", "LoadSpecificScene", "LoadScenePaths", "GetGameObjectsFromScene", "UnloadBundle");
        InspectLandmarkPipelineSurface("AssetBundleHooker", "GetLandmarksFromAssetBundleKey", "GetLandmarkFromUniqueIndex", "GetGameObjectsFromScene");
        InspectLandmarkPipelineSurface("SceneLoader", "loadedLandmarks", "GetLandmarksFromAssetBundleKey", "LoadScenes", "AddLoadedLandmark", "TrackAddressableLandmark", "InitializeLoadedLandmark", "SpawnLandmarkFromUniqueIndex");
        InspectLandmarkPipelineSurface("Landmark", "uniqueIndex", "Initialize", "OnSpawn", "OnRemove");
        InspectTypeSurface("LandmarkData");
        InspectTypeSurface("AsyncedLandmark");
        InspectTypeSurface("RGD_Landmark");
        InspectTypeSurface("RGD_ChunkPoint");
        InspectTypeSurface("RGD_World");
        InspectTypeSurface("Message_WorldManager_CreateLandmarks");
        InspectTypeSurface("WorldManager");
        InspectTypeSurface("ChunkPointType");

        Log("World probe v0.1.2 completed. Read-only inspection only; no game state was changed.");
    }

    private object InspectManager(string typeName, params string[] fieldNames)
    {
        try
        {
            Type managerType = FindLoadedType(typeName);
            if (managerType == null)
            {
                Log("Probe manager: type not found: " + typeName);
                return null;
            }

            Log("Probe manager: type found: " + managerType.FullName);
            object instance = null;

            if (typeof(UnityEngine.Object).IsAssignableFrom(managerType))
            {
                UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(managerType);
                int sceneInstances = 0;
                for (int i = 0; i < objects.Length; i++)
                {
                    Component component = objects[i] as Component;
                    if (component != null && component.gameObject != null &&
                        component.gameObject.scene.IsValid())
                    {
                        sceneInstances++;
                        if (instance == null)
                            instance = component;
                    }
                }

                Log("Probe manager: " + typeName + " loadedObjects=" + objects.Length +
                    ", sceneInstances=" + sceneInstances);
            }
            else
            {
                Log("Probe manager: " + typeName + " is not a UnityEngine.Object; checking static fields only.");
            }

            for (int i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo field = managerType.GetField(fieldNames[i],
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null)
                {
                    Log("Probe field: " + typeName + "." + fieldNames[i] + " not present.");
                    continue;
                }

                if (!field.IsStatic && instance == null)
                {
                    Log("Probe field: " + typeName + "." + fieldNames[i] +
                        " is instance field; no scene instance available.");
                    continue;
                }

                object value = field.GetValue(field.IsStatic ? null : instance);
                LogFieldSummary(typeName + "." + field.Name, value);
            }

            return instance;
        }
        catch (Exception ex)
        {
            Log("Probe manager failed: " + typeName + ": " +
                ex.GetType().Name + ": " + ex.Message);
            return null;
        }
    }

    private void LogFieldSummary(string label, object value)
    {
        if (value == null)
        {
            Log("Probe field: " + label + " = null");
            return;
        }

        ICollection collection = value as ICollection;
        if (collection != null)
        {
            Log("Probe field: " + label + " count=" + collection.Count +
                ", type=" + value.GetType().FullName);
            return;
        }

        Log("Probe field: " + label + " " + SafeValue(value));
    }

    private void InspectChunkPointDictionary(object chunkManager)
    {
        try
        {
            FieldInfo field = chunkManager.GetType().GetField("chunkPoints",
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
                return;

            object raw = field.GetValue(field.IsStatic ? null : chunkManager);
            IDictionary dictionary = raw as IDictionary;
            if (dictionary == null)
            {
                Log("ChunkPoint map: unavailable or not IDictionary.");
                return;
            }

            Log("ChunkPoint map: keyCount=" + dictionary.Count);
            IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
            int keyIndex = 0;
            try
            {
                while (enumerator.MoveNext() && keyIndex < MaxDictionaryKeys)
                {
                    object key = enumerator.Key;
                    object value = enumerator.Value;
                    ICollection points = value as ICollection;
                    Log("ChunkPoint map key[" + keyIndex + "]=" + SafeValue(key) +
                        ", valueType=" + (value == null ? "null" : value.GetType().FullName) +
                        ", pointCount=" + (points == null ? -1 : points.Count));

                    IEnumerable sequence = value as IEnumerable;
                    if (sequence != null)
                        InspectSequence("ChunkPoint[" + SafeValue(key) + "]", sequence,
                            MaxChunkPointsPerKey, true);
                    keyIndex++;
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            if (dictionary.Count > MaxDictionaryKeys)
                Log("ChunkPoint map: output capped at " + MaxDictionaryKeys + " keys.");
        }
        catch (Exception ex)
        {
            Log("ChunkPoint map inspection failed: " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private void InspectSpawnerList(object chunkManager)
    {
        try
        {
            FieldInfo field = chunkManager.GetType().GetField("allChunkSpawners",
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
                return;

            object raw = field.GetValue(field.IsStatic ? null : chunkManager);
            IEnumerable sequence = raw as IEnumerable;
            if (sequence == null)
            {
                Log("Spawner list: unavailable or not enumerable.");
                return;
            }

            ICollection collection = raw as ICollection;
            Log("Spawner list: count=" + (collection == null ? -1 : collection.Count) +
                ", type=" + raw.GetType().FullName);
            InspectSequence("Spawner", sequence, MaxSpawners, true);
        }
        catch (Exception ex)
        {
            Log("Spawner inspection failed: " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private void InspectSequence(string label, IEnumerable sequence, int limit, bool inspectFields)
    {
        IEnumerator enumerator = null;
        int index = 0;
        try
        {
            enumerator = sequence.GetEnumerator();
            while (index < limit && enumerator.MoveNext())
            {
                object item = enumerator.Current;
                if (item == null)
                {
                    Log(label + " item[" + index + "] = null");
                }
                else
                {
                    Log(label + " item[" + index + "] type=" + item.GetType().FullName);
                    if (inspectFields)
                        InspectObjectFields(label + "[" + index + "]", item, MaxFieldsPerObject);
                }
                index++;
            }

            if (index == limit)
                Log(label + ": output capped at " + limit + " entries.");
        }
        catch (Exception ex)
        {
            Log(label + " enumeration stopped at " + index + ": " +
                ex.GetType().Name + ": " + ex.Message);
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }

    private void InspectObjectFields(string label, object instance, int limit)
    {
        try
        {
            Type type = instance.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            int count = Math.Min(fields.Length, limit);
            for (int i = 0; i < count; i++)
            {
                object value = null;
                try
                {
                    value = fields[i].GetValue(instance);
                    Log(label + "." + fields[i].Name + " = " + SafeValue(value));
                }
                catch (Exception ex)
                {
                    Log(label + "." + fields[i].Name + " read failed: " + ex.GetType().Name);
                }
            }

            if (fields.Length > limit)
                Log(label + ": field output capped at " + limit + " of " + fields.Length + ".");
        }
        catch (Exception ex)
        {
            Log(label + " field inspection failed: " + ex.GetType().Name);
        }
    }

    private string SafeValue(object value)
    {
        if (value == null)
            return "null";

        Type type = value.GetType();
        if (value is string || type.IsPrimitive || type.IsEnum || value is decimal)
            return type.Name + ":" + value.ToString();

        return "type:" + type.FullName;
    }

    private void InspectTypeSurface(string typeName)
    {
        try
        {
            Type type = FindLoadedType(typeName);
            if (type == null)
            {
                Log("Type surface: not found: " + typeName);
                return;
            }

            Log("Type surface: " + type.FullName + ", base=" +
                (type.BaseType == null ? "none" : type.BaseType.FullName));

            if (type.IsEnum)
            {
                Array values = Enum.GetValues(type);
                for (int i = 0; i < values.Length; i++)
                    Log("Type enum: " + typeName + "." + values.GetValue(i));
                return;
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance |
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);
            int fieldCount = Math.Min(fields.Length, 24);
            for (int i = 0; i < fieldCount; i++)
                Log("Type field: " + typeName + "." + fields[i].Name +
                    " : " + fields[i].FieldType.FullName +
                    (fields[i].IsStatic ? " [static]" : " [instance]"));
            if (fields.Length > fieldCount)
                Log("Type surface: fields capped at " + fieldCount + " of " + fields.Length + ".");

            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance |
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);
            int methodCount = Math.Min(methods.Length, 32);
            for (int i = 0; i < methodCount; i++)
            {
                ParameterInfo[] parameters = methods[i].GetParameters();
                string signature = methods[i].Name + "(";
                for (int p = 0; p < parameters.Length; p++)
                {
                    if (p > 0) signature += ", ";
                    signature += parameters[p].ParameterType.Name;
                }
                signature += ") : " + methods[i].ReturnType.Name;
                Log("Type method: " + typeName + "." + signature);
            }
            if (methods.Length > methodCount)
                Log("Type surface: methods capped at " + methodCount + " of " + methods.Length + ".");
        }
        catch (Exception ex)
        {
            Log("Type surface failed: " + typeName + ": " +
                ex.GetType().Name + ": " + ex.Message);
        }
    }

    // Captures exact runtime metadata for the Landmark pipeline without invoking game methods.
    private void InspectLandmarkPipelineSurface(string typeName, params string[] memberNames)
    {
        try
        {
            Type type = FindLoadedType(typeName);
            if (type == null)
            {
                Log("Landmark metadata: type not found: " + typeName);
                return;
            }

            Log("Landmark metadata: type=" + type.FullName + ", assembly=" +
                type.Assembly.FullName + ", base=" +
                (type.BaseType == null ? "none" : type.BaseType.FullName));

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (int i = 0; i < memberNames.Length; i++)
            {
                string memberName = memberNames[i];
                FieldInfo field = type.GetField(memberName, flags);
                if (field != null)
                {
                    Log("Landmark field: " + typeName + "." + field.Name +
                        " : " + field.FieldType.FullName +
                        (field.IsStatic ? " [static]" : " [instance]") +
                        " [" + field.Attributes + "]");
                }

                MethodInfo[] methods = type.GetMethods(flags);
                int matches = 0;
                for (int m = 0; m < methods.Length; m++)
                {
                    MethodInfo method = methods[m];
                    if (!String.Equals(method.Name, memberName, StringComparison.Ordinal))
                        continue;

                    ParameterInfo[] parameters = method.GetParameters();
                    string signature = method.ReturnType.FullName + " " + method.Name + "(";
                    for (int p = 0; p < parameters.Length; p++)
                    {
                        if (p > 0) signature += ", ";
                        signature += parameters[p].ParameterType.FullName + " " + parameters[p].Name;
                    }
                    signature += ")";
                    Log("Landmark method: " + typeName + "." + signature +
                        (method.IsStatic ? " [static]" : " [instance]") +
                        " [" + method.Attributes + "]");
                    matches++;
                }

                if (field == null && matches == 0)
                    Log("Landmark metadata: member not found: " + typeName + "." + memberName);
            }
        }
        catch (Exception ex)
        {
            Log("Landmark metadata inspection failed: " + typeName + ": " +
                ex.GetType().Name + ": " + ex.Message);
        }
    }

    private Type FindLoadedType(string typeName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            try
            {
                Type found = assemblies[i].GetType(typeName, false);
                if (found != null)
                    return found;
            }
            catch
            {
                // Continue searching other loaded assemblies.
            }
        }
        return null;
    }
}

[executed on device: DESKTOP-3MKBODB (8088b63f-2a05-4f71-a7a4-ad740486ff11)]