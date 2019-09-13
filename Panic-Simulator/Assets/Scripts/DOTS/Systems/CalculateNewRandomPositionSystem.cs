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
        public void Execute(Entity entity, int index, ref Translation _translation, ref AgentComponent _agentComponent, [ReadOnly] ref BorderComponent _borderComponent)
        {
            if (!_agentComponent.hasTarget) // Agent dont have a target
            {
                for (int i = 0; i < randomAgentPositionsArray.Length; i++)
                {
                    Entity agent = randomAgentPositionsArray[i].entity; // Get the Entity

                    if (agent == entity) // Check if its the same agent
                    {
                        if (dice % 2 == 0 && dice >= 45f && dice <= 65)
                        {
                            float3 a = _borderComponent.frontLeft;
                            float3 b = _borderComponent.backLeft;
                            float3 c = _borderComponent.backLeft;

                            float3 d = _borderComponent.frontRight;
                            float3 e = _borderComponent.backRight;
                            float3 f = _borderComponent.backRight;

                            // Left Triangle
                            float as_x_i = randomAgentPositionsArray[i].newRandomPosition.x - a.x;
                            float as_z_i = randomAgentPositionsArray[i].newRandomPosition.z - a.z;

                            // Right Triangle
                            float as_x_ii = randomAgentPositionsArray[i].newRandomPosition.x - d.x;
                            float as_z_ii = randomAgentPositionsArray[i].newRandomPosition.z - d.z;

                            bool s_ab = (b.x - a.x) * as_z_i - (b.z - a.z) * as_x_i > 0; // Front.Left to Back.Left
                            bool s_de = (e.x - d.x) * as_z_ii - (e.z - d.z) * as_x_ii > 0; // Front.Right to Back.Right

                            if ((f.x - d.x) * as_z_ii - (d.z - d.z) * as_x_ii > 0 == s_de && (c.x - a.x) * as_z_i - (a.z - a.z) * as_x_i > 0 == s_ab
                                && randomAgentPositionsArray[i].newRandomPosition.z >= _borderComponent.frontLeft.z
                                && randomAgentPositionsArray[i].newRandomPosition.x <= _borderComponent.backLeft.x
                                && randomAgentPositionsArray[i].newRandomPosition.x >= _borderComponent.backRight.x)
                            {
                                // If newRandomPosition is inside the festival area, move to this position
                                // Set values like target and agentStatus
                                _agentComponent.target = randomAgentPositionsArray[i].newRandomPosition;
                                _agentComponent.agentStatus = AgentStatus.Moving;
                                _agentComponent.hasTarget = true;
                            }
                            else if ((f.x - e.x) * (randomAgentPositionsArray[i].newRandomPosition.z - e.z) - (d.z - e.z) * (randomAgentPositionsArray[i].newRandomPosition.x - e.x) > 0 != s_de
                                && (c.x - b.x) * (randomAgentPositionsArray[i].newRandomPosition.z - b.z) - (a.z - b.z) * (randomAgentPositionsArray[i].newRandomPosition.x - b.x) > 0 != s_ab
                                && randomAgentPositionsArray[i].newRandomPosition.z >= _borderComponent.frontLeft.z
                                && randomAgentPositionsArray[i].newRandomPosition.x <= _borderComponent.backLeft.x
                                && randomAgentPositionsArray[i].newRandomPosition.x >= _borderComponent.backRight.x)
                            {
                                // If newRandomPosition is inside the festival area, move to this position
                                // Set values like target and agentStatus
                                _agentComponent.target = randomAgentPositionsArray[i].newRandomPosition;
                                _agentComponent.agentStatus = AgentStatus.Moving;
                                _agentComponent.hasTarget = true;
                            }
                            else
                            {
                                // Else if newRandomPosition is outside the festival area, stay with AgentStatus.Idle
                                _agentComponent.target = _translation.Value;
                                _agentComponent.agentStatus = AgentStatus.Dancing;
                                _agentComponent.hasTarget = false;
                            }
                        }
                        else if (dice % 2 == 0 && dice >= 430 && dice <= 450)
                        {
                            if (!_agentComponent.jumped) // prevent from beeing idle when jumping, in der luft stehen bleiben
                            {
                                _agentComponent.target = _translation.Value;
                                _agentComponent.agentStatus = AgentStatus.Idle;
                                _agentComponent.hasTarget = false;
                            }
                        }
                        else if (dice % 2 == 0 && dice >= 630 && dice <= 650)
                        {
                            _agentComponent.target = _translation.Value;
                            _agentComponent.agentStatus = AgentStatus.Dancing;
                            _agentComponent.hasTarget = false;
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
                UnityEngine.Random.Range(agentTranslationArray[i].Value.x - 10, agentTranslationArray[i].Value.x + 10), // TODO: radius als/in Component darstellen.
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
