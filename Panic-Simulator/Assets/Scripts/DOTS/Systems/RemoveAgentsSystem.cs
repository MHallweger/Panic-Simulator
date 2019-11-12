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
    struct RemoveAgentsJob : IJobForEachWithEntity<AgentComponent, InputComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer; // instantiating and deleting of Entitys can only gets done on the main thread, save commands in buffer for main thread
        public void Execute(Entity entity, int index, [ReadOnly] ref AgentComponent agentComponent, [ReadOnly] ref InputComponent inputComponent)
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

        // Mono Behavior stuff
        ManagerSystem.actionUsed = false; // If this is not set, all new agents would instantly set to AgentStatus = AgentsStatus.Running

        // Get all children of the Actions GameObject and destroy them.
        // The children are all active Actions (burning fires)
        UnityEngine.GameObject actions = UnityEngine.GameObject.Find("Actions");
        int childs = actions.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            UnityEngine.GameObject.Destroy(actions.transform.GetChild(i).gameObject);
        }
        return removeAgentsJob;
    }
}