using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

/// <summary>
/// System that runs only 1 times to spawn the agent entitys.
/// </summary>
public class UnitSpawnerSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem; // For creating the commandBuffer

    /// <summary>
    /// Initialize The EndSimulationEntityCommandBufferSystem commandBufferSystem.
    /// </summary>
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //[BurstCompile] TODO implement a new job that handles the addComponents. BurstCompile cannot be used when addComponents are inside this job.
    struct SpawnJob : IJobForEachWithEntity<UnitSpawnerComponent, LocalToWorld, BorderComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer; // instantiating and deleting of Entitys can only gets done on the main thread, save commands in buffer for main thread
        public uint BaseSeed; // For generating random values with Unity.Mathematics.Random.NextFloat()

        public void Execute(Entity entity, int index, [ReadOnly] ref UnitSpawnerComponent spawner, [ReadOnly] ref LocalToWorld location, [ReadOnly] ref BorderComponent borderComponent)
        {
            var seed = (uint)(BaseSeed + index); // For Unity.Mathematics.Random --Slightly useless here because there is only one entity which calls this Execute()
            var rnd = new Random(seed); // Random Object for accessing rnd.NextFloat()

            for (int i = 0; i < spawner.CountX; i++)
            {
                for (int j = 0; j < spawner.CountY; j++)
                {
                    // Duplicating the Prefab entity
                    var instance = CommandBuffer.Instantiate(index, spawner.Prefab);

                    // LocalToWorld (float4x4) represents the transform from local space (float3) to world space
                    // returns the result of transforming a float3 point by a float4x4 matrix.
                    // Looking to stage:
                    // Front-Right.x: 134.838f ; Front-Left.x: 215.446f ; Front-Left/Right.z: 367.907f ; Back-Left/Right.z: 506.695
                    var randomPosition = /*math.transform(location.Value, */new float3(
                        rnd.NextFloat(borderComponent.frontRight_x, borderComponent.frontLeft_x),
                        0.5f,
                        rnd.NextFloat(borderComponent.frontLeftRight_z, borderComponent.backLeftRight_z));

                    // Set values of already attached Components
                    CommandBuffer.SetComponent(index, instance, new Translation { Value = randomPosition });

                    // Adding Components to every single Entity
                    CommandBuffer.AddComponent(index, instance, new MoveSpeedComponent { moveSpeed = rnd.NextFloat(3.0f, 6.0f), runningSpeed = rnd.NextFloat(6.0f, 10.0f) });
                    CommandBuffer.AddComponent(index, instance, new AgentComponent { hasTarget = false, target = randomPosition, agentStatus = AgentStatus.Idle });
                    CommandBuffer.AddComponent(index, instance, new BorderComponent { frontRight_x = 134.838f, frontLeft_x = 215.446f, frontLeftRight_z = 367.907f, backLeftRight_z = 506.695f });
                    CommandBuffer.AddComponent(index, instance, new InputComponent { });
                }
            }
            // Destory spawner, so the system only runs once
            CommandBuffer.DestroyEntity(index, entity);
        }
    }

    /// <summary>
    /// Runs on main thread, 1 times per frame. Stops when entity (dstManger) is destroyed.
    /// </summary>
    /// <param name="inputDeps"></param>
    /// <returns></returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        uint Seed = (uint)UnityEngine.Random.Range(1, 100); // Base seed

        // Schedule spawnJob
        var spawnJob = new SpawnJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), // Create the commandBuffer
            BaseSeed = Seed,
        }.Schedule(this, inputDeps);

        m_EntityCommandBufferSystem.AddJobHandleForProducer(spawnJob); // Execute the commandBuffer commands when spawnJob is finished
        return spawnJob;
    }
}
