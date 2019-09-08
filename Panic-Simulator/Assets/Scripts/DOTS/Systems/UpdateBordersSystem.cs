using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>

/// </summary>
public class UpdateBordersSystem : JobComponentSystem
{
    //[BurstCompile] Burst does not support World.Active ...
    struct UpdateBordersJob : IJobForEachWithEntity<BorderComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer; // instantiating and deleting of Entitys can only gets done on the main thread, save commands in buffer for main thread
        public float3 frontLeft;
        public float3 frontRight;
        public float3 backLeft;
        public float3 backRight;
        public void Execute(Entity entity, int index, ref BorderComponent _borderComponent)
        {
            _borderComponent.frontLeft = frontLeft;
            _borderComponent.frontRight = frontRight;
            _borderComponent.backLeft = backLeft;
            _borderComponent.backRight = backRight;

            World.Active.GetExistingSystem<UpdateBordersSystem>().Enabled = false;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var updateBordersJob = new UpdateBordersJob
        {
            frontLeft = UnityEngine.GameObject.Find("SpawnPoint_Front_Left").transform.position,
            frontRight = UnityEngine.GameObject.Find("SpawnPoint_Front_Right").transform.position,
            backLeft = UnityEngine.GameObject.Find("SpawnPoint_Back_Left_1").transform.position,
            backRight = UnityEngine.GameObject.Find("SpawnPoint_Back_Right_1").transform.position
        }.Schedule(this, inputDeps);

        return updateBordersJob;
    }
}