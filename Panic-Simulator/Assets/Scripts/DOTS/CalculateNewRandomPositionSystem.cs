using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// JobComponentSystem that runs on worker threads.
/// Calculates new random positions based on translation.value.
/// </summary>
public class CalculateNewRandomPositionSystem : JobComponentSystem
{
    // Struct for storing the entity and a new random position
    private struct EntityWithRandomPositions
    {
        public Entity entity;
        public float3 newRandomPosition;
    }

    [BurstCompile]
    struct CalculateNewRandomPositionJob : IJobForEachWithEntity<Translation, AgentComponent, LocalToWorld>
    {
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<EntityWithRandomPositions> randomAgentPositionsArray; // Array with EntityWithRandomPositions Strcut values. They include the specified entity + newPosition

        public void Execute(Entity entity, int index, ref Translation translation, ref AgentComponent agentComponent, ref LocalToWorld localToWorld)
        {
            if (!agentComponent.hasTarget) // Agent dont have a target
            {
                for (int i = 0; i < randomAgentPositionsArray.Length; i++)
                {
                    Entity agent = randomAgentPositionsArray[i].entity; // Get the Entity

                    if (agent == entity) // Check if its the same agent
                    {
                        // Set values like target and agentStatus
                        agentComponent.target = randomAgentPositionsArray[i].newRandomPosition;
                        agentComponent.agentStatus = AgentStatus.Moving;
                        agentComponent.hasTarget = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Runs on main thread, 1 times per frame
    /// </summary>
    /// <param name="inputDeps"></param>
    /// <returns></returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Create Query for array creation (next steps)
        EntityQuery agentQuery = GetEntityQuery(typeof(AgentComponent), ComponentType.ReadOnly<Translation>());

        // Get all agents
        NativeArray<Entity> agentEntityArray = agentQuery.ToEntityArray(Allocator.TempJob);

        // Get all translation components of the agents
        NativeArray<Translation> agentTranslationArray = agentQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        // Create The EntityWithRandomPosition Array for the jobs
        NativeArray<EntityWithRandomPositions> randomPositionsArray = new NativeArray<EntityWithRandomPositions>(agentTranslationArray.Length, Allocator.TempJob);

        // Loop through the translation components
        for (int i = 0; i < agentTranslationArray.Length; i++)
        {
            // Fill the randomPositionsArray with randomPositions, based on the actual translation.value. The entity helps to identify later.
            randomPositionsArray[i] = new EntityWithRandomPositions
            {
                entity = agentEntityArray[i],
                newRandomPosition = new float3(
                UnityEngine.Random.Range(agentTranslationArray[i].Value.x - 10, agentTranslationArray[i].Value.x + 10),
                0.5f,
                UnityEngine.Random.Range(agentTranslationArray[i].Value.z - 10, agentTranslationArray[i].Value.z + 10))
            };
        }

        // Schedule job for passing the newPosition to each entity
        var findNearestPositionJob = new CalculateNewRandomPositionJob
        {
            randomAgentPositionsArray = randomPositionsArray,
        }.Schedule(this, inputDeps);

        // Disposing NativeArrays
        agentEntityArray.Dispose();
        agentTranslationArray.Dispose();

        return findNearestPositionJob;
    }
}
