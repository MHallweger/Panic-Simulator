using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// JobComponentSystems can run on worker threads.
// However, creating and removing Entities can only be done on the main thread to prevent race conditions.
// The system uses an EntityCommandBuffer to defer tasks that can't be done inside the Job.
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class UnitSpawnerSystem : JobComponentSystem
{
    // BeginInitializationEntityCommandBufferSystem is used to create a command buffer which will then be played back
    // when that barrier system executes.
    // Though the instantiation command is recorded in the SpawnJob, it's not actually processed (or "played back")
    // until the corresponding EntityCommandBufferSystem is updated. To ensure that the transform system has a chance
    // to run on the newly-spawned entities before they're rendered for the first time, the HelloSpawnerSystem
    // will use the BeginSimulationEntityCommandBufferSystem to play back its commands. This introduces a one-frame lag
    // between recording the commands and instantiating the entities, but in practice this is usually not noticeable.
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    struct SpawnJob : IJobForEachWithEntity<UnitSpawnerComponent, LocalToWorld> // Cannot use the EntityManager in a c# Job (UnitSpawner is needed) -> Reason for using IJobForEachWithEntity
    {
        public EntityCommandBuffer CommandBuffer;
        [ReadOnly] public uint BaseSeed;

        public void Execute(Entity entity, int index, [ReadOnly] ref UnitSpawnerComponent spawner, [ReadOnly] ref LocalToWorld location)
        {
            var seed = (uint) (BaseSeed + index); // For Unity.Mathematics.Random --Slightly useless here because there is only one entity which calls this Execute()
            var rnd = new Random(seed); // Random Object for accessing rnd.NextFloat()

            for (int i = 0; i < 50000; i++)
            {
                var instance = CommandBuffer.Instantiate(spawner.Prefab);

                // LocalToWorld (float4x4) represents the transform from local space (float3) to world space
                // returns the result of transforming a float3 point by a float4x4 matrix.
                var randomPosition = math.transform(location.Value, new float3(rnd.NextFloat(134.838f, 215.446f), 0.5f, rnd.NextFloat(367.907f, 506.695f)));

                CommandBuffer.SetComponent(instance, new Translation { Value = randomPosition });
            }
            // Destory spawner, so the system only runs once
            CommandBuffer.DestroyEntity(entity);
        }
    }

    uint Seed = (uint) UnityEngine.Random.Range(1, 100); // prevent to run 1 times per frame in OnUpdate
    /// <summary>
    /// Runs on main thread, 1 times per frame
    /// </summary>
    /// <param name="inputDeps"></param>
    /// <returns></returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //Instead of performing structural changes directly, a Job can add a command to an EntityCommandBuffer to perform such changes on the main thread after the Job has finished.
        //Command buffers allow you to perform any, potentially costly, calculations on a worker thread, while queuing up the actual insertions and deletions for later.
        // Schedule the job that will add Instantiate commands to the EntityCommandBuffer.
        var job = new SpawnJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
            BaseSeed = Seed
        }.ScheduleSingle(this, inputDeps);

        // SpawnJob runs in parallel with no sync point until the barrier system executes.
        // When the barrier system executes we want to complete the SpawnJob and then play back the commands (Creating the entities and placing them).
        // We need to tell the barrier system which job it needs to complete before it can play back the commands.
        m_EntityCommandBufferSystem.AddJobHandleForProducer(job);
        return job;
    }
}
