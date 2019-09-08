using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// System that updates the current status of left and right click. The agents are now able to access the specific left/right click bool to see if one of both was clicked. 
/// The System also takes a look on different keys.
/// </summary>
[UpdateBefore(typeof(UnitSpawnerSystem))]
public class ManagerSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem; // For creating the commandBuffer

    /// <summary>
    /// Initialize The EndSimulationEntityCommandBufferSystem commandBufferSystem.
    /// </summary>
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //[BurstCompile] Burst does not support World.Active ...
    struct ManagerJob : IJobForEachWithEntity<InputComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer; // instantiating and deleting of Entitys can only gets done on the main thread, save commands in buffer for main thread
        public void Execute(Entity entity, int index, ref InputComponent inputComponent)
        {
            // If key 1 is pressed down. Add a allowToSpawn component to the spawn system, to allow spawning agents when pressing key 1 up.
            if (inputComponent.keyOnePressedDown)
            {
                World.Active.GetExistingSystem<RemoveAgentsSystem>().Enabled = false;
                World.Active.GetExistingSystem<UnitSpawnerSystem>().Enabled = true;
            }
            else if (inputComponent.keyFivePressedUp || inputComponent.keySixPressedUp || inputComponent.keyThreePressedUp || inputComponent.keyFourPressedUp)
            {
                // Add/Remove Barriers
                // Enable System that updates the spawn objects from the border component
                World.Active.GetExistingSystem<UpdateBordersSystem>().Enabled = true;
            }
            else if (inputComponent.keyTwoPressedUp)
            {
                World.Active.GetExistingSystem<RemoveAgentsSystem>().Enabled = true;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var managerJob = new ManagerJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), // Create the commandBuffer
        }.Schedule(this, inputDeps);

        m_EntityCommandBufferSystem.AddJobHandleForProducer(managerJob); // Execute the commandBuffer commands when spawnJob is finished
        return managerJob;
    }
}