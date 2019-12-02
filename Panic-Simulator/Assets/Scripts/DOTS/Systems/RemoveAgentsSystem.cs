using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

/// <summary>
/// Handles the job to remove all Agent entities.
/// </summary>
[UpdateBefore(typeof(UnitSpawnerSystem))]
public class RemoveAgentsSystem : JobComponentSystem
{
    // For creating the commandBuffer
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem; 

    /// <summary>
    /// Initialize The EndSimulationEntityCommandBufferSystem commandBufferSystem.
    /// </summary>
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    /// <summary>
    /// Remove all entities with AgentComponent and InputComponent.
    /// </summary>
    [BurstCompile]
    struct RemoveAgentsJob : IJobForEachWithEntity<AgentComponent, InputComponent>
    {
        // Data from main thread
        // instantiating and deleting of Entitys can only gets done on the main thread, save commands in buffer for main thread
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref AgentComponent _agentComponent, [ReadOnly] ref InputComponent _inputComponent)
        {
            CommandBuffer.DestroyEntity(index, entity);
        }
    }

    /// <summary>
    /// Job that handles commands that cannot be done inside a [BurstCompile] Job.
    /// This is the reason why here is no [BurstCompile] Tag.
    /// </summary>
    struct DisableRemoveAgentsSystemJob : IJobForEach<AgentComponent>
    {
        public void Execute(ref AgentComponent _agentComponent)
        {
            // Disable this whole System
            World.Active.GetExistingSystem<RemoveAgentsSystem>().Enabled = false;
        }
    }

    /// <summary>
    /// This OnUpdate only runs one time.
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>jobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new JobHandle();

        // Creating RemoveAgentsJob
        RemoveAgentsJob removeAgentsJob = new RemoveAgentsJob
        {
            // Create the commandBuffer
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
        };

        // Schedule this job and save the results into jobHandle
        jobHandle = removeAgentsJob.Schedule(this, inputDeps);

        // Execute the commandBuffer commands when removeAgentsJob is finished
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        // Mono Behavior stuff
        // If this is not set, all new agents would instantly set to AgentStatus = AgentsStatus.Running
        ManagerSystem.actionUsed = false; 

        // Get all children of the Actions GameObject and destroy them.
        // The children are all active Actions (burning fires)
        UnityEngine.GameObject actions = UnityEngine.GameObject.Find("Actions");
        int childs = actions.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            UnityEngine.GameObject.Destroy(actions.transform.GetChild(i).gameObject);
        }

        // Creating DisableRemoveAgentsSystemJob to disable this whole System
        DisableRemoveAgentsSystemJob disableRemoveAgentsSystemJob = new DisableRemoveAgentsSystemJob
        {
        };

        // Schedule this job and save the results into jobHandle
        jobHandle = disableRemoveAgentsSystemJob.Schedule(this, jobHandle);

        return jobHandle;
    }
}