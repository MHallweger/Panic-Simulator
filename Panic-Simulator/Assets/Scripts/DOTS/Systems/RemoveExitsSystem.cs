using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using System.Linq;

/// <summary>
/// Handles the Job to remove exits entities.
/// </summary>
[UpdateBefore(typeof(UnitSpawnerSystem))]
public class RemoveExitsSystem : JobComponentSystem
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
    /// Deleting all Entities with ExitComponent.
    /// </summary>
    [BurstCompile]
    struct RemoveExitsJob : IJobForEachWithEntity<ExitComponent>
    {
        // Data from main thread
        // Instantiating and deleting of Entitys can only get done on the main thread. Save commands in buffer for main thread later
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref ExitComponent _exitComponent)
        {
            CommandBuffer.DestroyEntity(index, entity);
        }
    }

    /// <summary>
    /// Job that handles commands that cannot be done inside a [BurstCompile] Job.
    /// This is the reason why here is no [BurstCompile] Tag.
    /// </summary>
    struct DisableRemoveExitsSystemJob : IJobForEach<ExitComponent>
    {
        public void Execute(ref ExitComponent _exitComponent)
        {
            // Disable this whole System
            World.Active.GetExistingSystem<RemoveExitsSystem>().Enabled = false;
        }
    }

    /// <summary>
    /// This OnUpdate only runs one time, so GameObject.Find will be no problem here.
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>jobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new JobHandle();

        if (!InputWindow.instance.inputField.isFocused)
        {
            // If InputField is not focused
            // Get all pole GameObjects
            var poleClones = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.GameObject>().Where(obj => obj.name == "GroundPoleA(Clone)");

            foreach (UnityEngine.GameObject pole in poleClones)
            {
                if (pole != null)
                {
                    // The poles are childs from a parent GameObject.
                    // Get this GameObject, get the first child from this GameObject which holds a MeshRenderer, which displays the whole GameObject
                    // Enable this MeshRenderer.
                    // Not the best implementation but ECS/Jobs do not offer a better solution.
                    pole.transform.parent.GetChild(0).GetComponent<UnityEngine.MeshRenderer>().enabled = true;
                    UnityEngine.Object.Destroy(pole);
                }
            }

            // Creating RemoveExitsJob
            RemoveExitsJob removeExitsJob = new RemoveExitsJob
            {
                // Create the commandBuffer
                CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            };

            // Scheduling RemoveExitsJob and save the results into jobHandle
            jobHandle = removeExitsJob.Schedule(this, inputDeps);

            // Load CommandBuffercommands into main thread
            // Execute the commandBuffer commands when spawnJob is finished
            m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            // Create DisableRemoveExitsSystemJob to disable this whole System
            DisableRemoveExitsSystemJob disableRemoveExitsSystemJob = new DisableRemoveExitsSystemJob
            {
            };

            // Schedule this job and save the results into jobHandle
            jobHandle = disableRemoveExitsSystemJob.Schedule(this, jobHandle);
        }
        return jobHandle;
    }
}