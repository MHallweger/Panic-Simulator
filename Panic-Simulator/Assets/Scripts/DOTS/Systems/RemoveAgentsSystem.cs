using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

/// <summary>
/// System that updates the current status of left and right click. The agents are now able to access the specific left/right click bool to see if one of both was clicked. 
/// The System also takes a look on different keys.
/// </summary>
[UpdateBefore(typeof(UnitSpawnerSystem))]
public class RemoveAgentsSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem; // For creating the commandBuffer

    /// <summary>
    /// Initialize The EndSimulationEntityCommandBufferSystem commandBufferSystem.
    /// </summary>
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //[BurstCompile]
    struct RemoveAgentsJob : IJobForEachWithEntity<AgentComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer; // instantiating and deleting of Entitys can only gets done on the main thread, save commands in buffer for main thread
        public void Execute(Entity entity, int index, [ReadOnly] ref AgentComponent agentComponent)
        {
            CommandBuffer.DestroyEntity(index, entity);
            World.Active.GetExistingSystem<RemoveAgentsSystem>().Enabled = false;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var removeAgentsJob = new RemoveAgentsJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), // Create the commandBuffer
        }.Schedule(this, inputDeps);

        m_EntityCommandBufferSystem.AddJobHandleForProducer(removeAgentsJob); // Execute the commandBuffer commands when spawnJob is finished
        return removeAgentsJob;
    }
}