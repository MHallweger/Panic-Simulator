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
/// System that updates the current status of left and right click. The agents are now able to access the specific left/right click bool to see if one of both was clicked. 
/// The System also takes a look on different keys.
/// </summary>
[UpdateBefore(typeof(UnitSpawnerSystem))]
public class RemoveExitsSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem; // For creating the commandBuffer

    /// <summary>
    /// Initialize The EndSimulationEntityCommandBufferSystem commandBufferSystem.
    /// </summary>
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //[BurstCompile] TODO creaate job that only disables this system. Then enable Burst on this job
    struct RemoveExitsJob : IJobForEachWithEntity<ExitComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer; // instantiating and deleting of Entitys can only gets done on the main thread, save commands in buffer for main thread
        public void Execute(Entity entity, int index, [ReadOnly] ref ExitComponent exitComponent)
        {
            CommandBuffer.DestroyEntity(index, entity);
            World.Active.GetExistingSystem<RemoveExitsSystem>().Enabled = false;
        }
    }

    /// <summary>
    /// This OnUpdate only runs one time, so GameObject.Find will be no problem here.
    /// </summary>
    /// <param name="inputDeps"></param>
    /// <returns></returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Enable the Mesh Renderer of the LOD Barrier.
        //hit.collider.gameObject.transform.parent.gameObject.transform.Find("chainlink_group-1_LOD0").GetComponent<UnityEngine.MeshRenderer>().enabled = true;
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

        var removeExitsJob = new RemoveExitsJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), // Create the commandBuffer
        }.Schedule(this, inputDeps);

        m_EntityCommandBufferSystem.AddJobHandleForProducer(removeExitsJob); // Execute the commandBuffer commands when spawnJob is finished
        return removeExitsJob;
    }
}