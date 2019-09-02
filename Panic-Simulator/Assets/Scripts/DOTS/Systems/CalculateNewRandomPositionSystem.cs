using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

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
    struct CalculateNewRandomPositionJob : IJobForEachWithEntity<Translation, AgentComponent, BorderComponent>
    {
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<EntityWithRandomPositions> randomAgentPositionsArray; // Array with EntityWithRandomPositions Strcut values. They include the specified entity + newPosition

        public float dice;
        public void Execute(Entity entity, int index, ref Translation translation, ref AgentComponent agentComponent, [ReadOnly] ref BorderComponent borderComponent)
        {
            if (!agentComponent.hasTarget) // Agent dont have a target
            {
                for (int i = 0; i < randomAgentPositionsArray.Length; i++)
                {
                    Entity agent = randomAgentPositionsArray[i].entity; // Get the Entity

                    if (agent == entity) // Check if its the same agent
                    {
                        if (dice % 2 == 0 && dice >= 45f && dice <= 65)
                        {
                            // Looking to stage:
                            // Front-Right.x: 134.838f ; Front-Left.x: 215.446f ; Front-Left/Right.z: 367.907f ; Back-Left/Right.z: 506.695
                            if (!(randomAgentPositionsArray[i].newRandomPosition.x < borderComponent.frontRight_x
                                || randomAgentPositionsArray[i].newRandomPosition.x > borderComponent.frontLeft_x
                                || randomAgentPositionsArray[i].newRandomPosition.z < borderComponent.frontLeftRight_z
                                || randomAgentPositionsArray[i].newRandomPosition.z > borderComponent.backLeftRight_z))
                            {
                                // If newRandomPosition is inside the festival area, move to this position
                                // Set values like target and agentStatus
                                agentComponent.target = randomAgentPositionsArray[i].newRandomPosition;
                                agentComponent.agentStatus = AgentStatus.Moving;
                                agentComponent.hasTarget = true;
                            }
                            else 
                            {
                                // Else if newRandomPosition is outside the festival area, stay with AgentStatus.Idle
                                agentComponent.target = translation.Value;
                                agentComponent.agentStatus = AgentStatus.Dancing;
                                agentComponent.hasTarget = false;
                            }
                        }
                        else if (dice % 2 == 0 && dice >= 430 && dice <= 450)
                        {
                            if (!agentComponent.jumped) // prevent from beeing idle when jumping, in der luft stehen bleiben
                            {
                                agentComponent.target = translation.Value;
                                agentComponent.agentStatus = AgentStatus.Idle;
                                agentComponent.hasTarget = false;
                            }
                        }
                        else if (dice % 2 == 0 && dice >= 630 && dice <= 650)
                        {
                            agentComponent.target = translation.Value;
                            agentComponent.agentStatus = AgentStatus.Dancing;
                            agentComponent.hasTarget = false;
                        }
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

        // Get all agents (entitys)
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

        //var test = EntityManager.GetSharedComponentData<RenderMesh>(agentEntityArray[0]);

        //UnityEngine.Debug.Log(test.material);
        // Schedule job for passing the newPosition to each entity
        var calculateNewRandomPositionjob = new CalculateNewRandomPositionJob
        {
            randomAgentPositionsArray = randomPositionsArray,
            dice = UnityEngine.Random.Range(1, 1000)
        }.Schedule(this, inputDeps);

        // Disposing NativeArrays
        agentEntityArray.Dispose();
        agentTranslationArray.Dispose();

        return calculateNewRandomPositionjob;
    }
}
