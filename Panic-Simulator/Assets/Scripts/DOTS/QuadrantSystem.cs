using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;

public struct QuadrantEntity : IComponentData
{
    public TypeEnum typeEnum;

    public enum TypeEnum
    {
        Agent,
        Exit
    }
}

public struct QuadrantData
{
    public Entity entity;
    public float3 position;
    public AgentComponent agentComponent;
    public QuadrantEntity quadrantEntity;
}

public class QuadrantSystem : JobComponentSystem
{
    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
    public const int quadrantYMultiplier = 1000; // Enough for this level/Simulation size
    private const float quadrantCellSize = 1.5f; // vorher int 1

    public static int GetPositionHashMapKey(float3 position) // Video Anleitung
    {
        return (int)(math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.z / quadrantCellSize)));
    }

    private static void DebugDrawQuadrant(Vector3 position)
    {
        Vector3 lowerLeft = new Vector3(math.floor(position.x / quadrantCellSize) * quadrantCellSize, 0.5f, math.floor(position.z / quadrantCellSize) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +0, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+1, +0, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +0, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +0, +1) * quadrantCellSize, lowerLeft + new Vector3(+1, +0, +1) * quadrantCellSize);
        //Debug.Log(GetPositionHashMapKey(position) + " " + position);
    }

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

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<AgentComponent, Translation, QuadrantEntity>
    {
        public NativeMultiHashMap<int, QuadrantData>.Concurrent quadrantMultiHashMap;

        public void Execute(Entity entity, int index, ref AgentComponent agentComponent, ref Translation translation, ref QuadrantEntity quadrantEntity)
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

    [BurstCompile]
    private struct SetQuadrantDataHashMapJobForExits : IJobForEachWithEntity<ExitComponent, Translation, QuadrantEntity>
    {
        public NativeMultiHashMap<int, QuadrantData>.Concurrent quadrantMultiHashMap;

        public void Execute(Entity entity, int index, [ReadOnly] ref ExitComponent exitComponent, [ReadOnly] ref Translation translation, ref QuadrantEntity quadrantEntity)
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

    protected override void OnCreate()
    {
        quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        quadrantMultiHashMap.Dispose();
        base.OnDestroy();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(QuadrantEntity), typeof(Translation));

        quadrantMultiHashMap.Clear(); // because of the persistent hasMap
        if (entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity) //  prevent hasMap is full error, hashmaps cannot dynamically grow in jobs. need to to this here instead, before starting the specifify job
        {
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
        {
            quadrantMultiHashMap = quadrantMultiHashMap.ToConcurrent() // TODO: deprecated
        };

        JobHandle jobHandle = setQuadrantDataHashMapJob.Schedule(this, inputDeps);/* JobForEachExtensions.Schedule(setQuadrantDataHashMapJob, entityQuery);*/

        SetQuadrantDataHashMapJobForExits setQuadrantDataHashMapJobForExits = new SetQuadrantDataHashMapJobForExits
        {
            quadrantMultiHashMap = quadrantMultiHashMap.ToConcurrent()
        };

        jobHandle = setQuadrantDataHashMapJobForExits.Schedule(this, jobHandle);
        //jobHandle.Complete();

        //var mousePosition = Input.mousePosition;
        //mousePosition.z = Mathf.Abs(Camera.main.gameObject.transform.position.z);
        //var worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        //DebugDrawQuadrant(GameObject.Find("QuadrantDebugCube").transform.position);

        //Debug.Log(GetEntityCountInHashMap(quadrantMultiHashMap, GetPositionHashMapKey(GameObject.Find("QuadrantDebugCube").transform.position)));
        return jobHandle;

    }
}
