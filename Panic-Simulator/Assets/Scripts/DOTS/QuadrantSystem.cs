using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;

/// <summary>
/// Needed for calculating Quadrant Data. Helps to spread panic.
/// </summary>
public struct QuadrantEntity : IComponentData
{
    public TypeEnum typeEnum;

    /// <summary>
    /// Identify which entity is a Agent and which one is a exit.
    /// </summary>
    public enum TypeEnum
    {
        Agent,
        Exit
    }
}

/// <summary>
/// These are saved for each Quadrant.
/// </summary>
public struct QuadrantData
{
    public Entity entity;
    public float3 position;
    public AgentComponent agentComponent;
    public QuadrantEntity quadrantEntity;
}

/// <summary>
/// System that handles the whole Quadrant System.
/// </summary>
public class QuadrantSystem : JobComponentSystem
{
    #region Variables
    public const int quadrantYMultiplier = 1000; // Enough for this level/Simulation size
    private const float quadrantCellSize = 1f; // Cell Size of each quadrant
    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap; // For accessing the quadrantMultiHashMap from other Systems and MonoBehavior classes
    #endregion // Variables

    /// <summary>
    /// Funktion that takes a position to calculate a individual key with basic math. This way, each position has an individual key to save and load data from.
    /// </summary>
    /// <param name="position">position that is used to calculate an individual key</param>
    /// <returns></returns>
    public static int GetPositionHashMapKey(float3 position) // Video Anleitung
    {
        return (int)(math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.z / quadrantCellSize)));
    }

    #region Debug
    /// <summary>
    /// Debug Method that can be used to display a quadrant. The Quadrant can be seen when enabling DebugCube in the Inspector.
    /// A static GameObject cube is more accurate than the current global mouse position.
    /// </summary>
    /// <param name="position">Current position of the DebugCube</param>
    private static void DebugDrawQuadrant(Vector3 position)
    {
        Vector3 lowerLeft = new Vector3(math.floor(position.x / quadrantCellSize) * quadrantCellSize, 0.5f, math.floor(position.z / quadrantCellSize) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +0, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+1, +0, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +0, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +0, +1) * quadrantCellSize, lowerLeft + new Vector3(+1, +0, +1) * quadrantCellSize);
        //Debug.Log(GetPositionHashMapKey(position) + " " + position);
    }
    #endregion // Debug
    /// <summary>
    /// Function that calculates how many Entitys are inside a quadrant.
    /// </summary>
    /// <param name="quadrantMultiHashMap">All keys with all QuadrantData</param>
    /// <param name="hashMapKey">A specific key</param>
    /// <returns>entity count inside a quadrant</returns>
    public static int GetEntityCountInHashMap(NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap, int hashMapKey)
    {
        QuadrantData quadrantData;
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
        int count = 0;
        if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
        {
            do
            {
                count++;
            } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
        }
        return count;
    }

    /// <summary>
    /// Job that runs on each entity with AgentComponent, Translation and QuadrantEntity Component.
    /// Calculates a quadrant key for their actual position.
    /// Use this key to save a quadrant with specific data.
    /// Specific for Agents.
    /// </summary>
    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<AgentComponent, Translation, QuadrantEntity>
    {
        public NativeMultiHashMap<int, QuadrantData>.Concurrent quadrantMultiHashMap;

        public void Execute(Entity entity, int index, [ReadOnly] ref AgentComponent agentComponent, [ReadOnly] ref Translation translation, [ReadOnly] ref QuadrantEntity quadrantEntity)
        {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMap.Add(hashMapKey, new QuadrantData
            {
                entity = entity,
                position = translation.Value,
                agentComponent = agentComponent,
                quadrantEntity = quadrantEntity
            });
        }
    }

    /// <summary>
    /// Job that runs on each Entity with ExitComponent, Translation and QuadrantEntity Component.
    /// Calculates a quadrant key for their actual position.
    /// Use this key to save a quadrant with specific data.
    /// Specific for exits.
    /// </summary>
    [BurstCompile]
    private struct SetQuadrantDataHashMapJobForExits : IJobForEachWithEntity<ExitComponent, Translation, QuadrantEntity>
    {
        public NativeMultiHashMap<int, QuadrantData>.Concurrent quadrantMultiHashMap;

        public void Execute(Entity entity, int index, [ReadOnly] ref ExitComponent exitComponent, [ReadOnly] ref Translation translation, [ReadOnly] ref QuadrantEntity quadrantEntity)
        {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMap.Add(hashMapKey, new QuadrantData
            {
                entity = entity,
                position = translation.Value,
                quadrantEntity = quadrantEntity
            });
        }
    }

    /// <summary>
    /// Creates the main quadrantMultiHashMap for the quadrant System at the beginning.
    /// </summary>
    protected override void OnCreate()
    {
        quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    /// <summary>
    /// Disposes the main quadrantMultiHashMap for the quadrant System at the end.
    /// </summary>
    protected override void OnDestroy()
    {
        quadrantMultiHashMap.Dispose();
        base.OnDestroy();
    }

    /// <summary>
    /// Main Thread section, where Jobs are called and connected.
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>JobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Get All entitys with QuadrantEntity and Translation Component
        EntityQuery entityQuery = GetEntityQuery(typeof(QuadrantEntity), typeof(Translation));

        quadrantMultiHashMap.Clear(); // because of the persistent hasMap
        if (entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity) 
        {
            //  prevent hasMap is full error, hashmaps cannot dynamically grow in jobs. need to to this here instead, before starting the specifify job
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        // Create SetQuadrantDataHashMapJob to load every Agent Quadrant into the quadrantMultiHashMap
        SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
        {
            quadrantMultiHashMap = quadrantMultiHashMap.ToConcurrent()
        };

        // Schedule this job and save deps into jobHandle
        JobHandle jobHandle = setQuadrantDataHashMapJob.Schedule(this, inputDeps);

        // Create SetQuadrantDataHashMapJobForExits to load every Exit Quadrant into the quadrantMultiHashMap
        SetQuadrantDataHashMapJobForExits setQuadrantDataHashMapJobForExits = new SetQuadrantDataHashMapJobForExits
        {
            quadrantMultiHashMap = quadrantMultiHashMap.ToConcurrent()
        };

        // Schedule this job, use the current jobHandle as deps and save the new deps into jobHandle again
        jobHandle = setQuadrantDataHashMapJobForExits.Schedule(this, jobHandle);
        //jobHandle.Complete();

        #region Debug
        //var mousePosition = Input.mousePosition;
        //mousePosition.z = Mathf.Abs(Camera.main.gameObject.transform.position.z);
        //var worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //DebugDrawQuadrant(GameObject.Find("QuadrantDebugCube").transform.position);
        //Debug.Log(GetEntityCountInHashMap(quadrantMultiHashMap, GetPositionHashMapKey(GameObject.Find("QuadrantDebugCube").transform.position)));
        #endregion // Debug
        return jobHandle;
    }
}
