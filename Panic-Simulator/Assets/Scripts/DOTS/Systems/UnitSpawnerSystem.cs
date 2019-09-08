using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

/// <summary>
/// System that runs only 1 times to spawn the agent entitys.
/// </summary>
[UpdateAfter(typeof(ManagerSystem))]
public class UnitSpawnerSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem; // For creating the commandBuffer

    /// <summary>
    /// Initialize The EndSimulationEntityCommandBufferSystem commandBufferSystem.
    /// </summary>
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [ExcludeComponent(typeof(AgentComponent), typeof(MoveSpeedComponent))]
    //[BurstCompile] TODO implement a new job that handles the addComponents. BurstCompile cannot be used when addComponents are inside this job.
    struct SpawnJob : IJobForEachWithEntity<UnitSpawnerComponent, BorderComponent, InputComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer; // instantiating and deleting of Entitys can only gets done on the main thread, save commands in buffer for main thread
        public uint BaseSeed; // For generating random values with Unity.Mathematics.Random.NextFloat()
        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<float3> randomPositions;
        public void Execute(Entity entity, int index, [ReadOnly] ref UnitSpawnerComponent _spawner, [ReadOnly] ref BorderComponent _borderComponent, [ReadOnly] ref InputComponent _inputComponent)
        {
            if (_inputComponent.keyOnePressedUp)
            {
                var seed = (uint)(BaseSeed + index); // For Unity.Mathematics.Random --Slightly useless here because there is only one entity which calls this Execute()
                var rnd = new Random(seed); // Random Object for accessing rnd.NextFloat()

                for (int i = 0; i < _spawner.AmountToSpawn; i++)
                {
                    // Duplicating the Prefab entity
                    var instance = CommandBuffer.Instantiate(index, _spawner.Prefab);
                    bool setPosition = false;
                    int loopIndex = 0;

                    float3 a = _borderComponent.frontLeft;
                    float3 b = _borderComponent.backLeft;
                    float3 c = _borderComponent.backLeft;

                    float3 d = _borderComponent.frontRight;
                    float3 e = _borderComponent.backRight;
                    float3 f = _borderComponent.backRight;

                    while (!setPosition)
                    {
                        // Left Triangle
                        float as_x_i = randomPositions[i + loopIndex].x - a.x;
                        float as_z_i = randomPositions[i + loopIndex].z - a.z;

                        // Right Triangle
                        float as_x_ii = randomPositions[i + loopIndex].x - d.x;
                        float as_z_ii = randomPositions[i + loopIndex].z - d.z;

                        bool s_ab = (b.x - a.x) * as_z_i - (b.z - a.z) * as_x_i > 0; // Front.Left to Back.Left
                        bool s_de = (e.x - d.x) * as_z_ii - (e.z - d.z) * as_x_ii > 0; // Front.Right to Back.Right

                        if ((f.x - d.x) * as_z_ii - (d.z - d.z) * as_x_ii > 0 == s_de && (c.x - a.x) * as_z_i - (a.z - a.z) * as_x_i > 0 == s_ab)
                        {
                            CommandBuffer.SetComponent(index, instance, new Translation
                            {
                                Value = randomPositions[i + loopIndex]
                            }); // Agent outside. Set Translation value

                            CommandBuffer.AddComponent(index, instance, new AgentComponent
                            {
                                hasTarget = false,
                                target = randomPositions[i + loopIndex],
                                agentStatus = AgentStatus.Idle
                            });

                            setPosition = true;
                        }
                        else if ((f.x - e.x) * (randomPositions[i + loopIndex].z - e.z) - (d.z - e.z) * (randomPositions[i + loopIndex].x - e.x) > 0 != s_de
                            && (c.x - b.x) * (randomPositions[i + loopIndex].z - b.z) - (a.z - b.z) * (randomPositions[i + loopIndex].x - b.x) > 0 != s_ab)
                        {
                            CommandBuffer.SetComponent(index, instance, new Translation
                            {
                                Value = randomPositions[i + loopIndex]
                            }); // Agent outside. Set Translation value

                            CommandBuffer.AddComponent(index, instance, new AgentComponent
                            {
                                hasTarget = false,
                                target = randomPositions[i + loopIndex],
                                agentStatus = AgentStatus.Idle
                            });

                            setPosition = true;
                        }
                        else
                        {
                            setPosition = false; // Inside of Triangle
                            if (i + loopIndex + 1.0f < randomPositions.Length)
                            {
                                loopIndex++;
                            }
                            else
                            {
                                // Index problem here
                                setPosition = true;
                                float3 rndPos = new float3(
                                    rnd.NextFloat(_borderComponent.frontLeft.x, _borderComponent.frontRight.x),
                                    .5f,
                                    rnd.NextFloat(_borderComponent.backLeft.z, _borderComponent.backRight.z));

                                CommandBuffer.SetComponent(index, instance, new Translation
                                {
                                    Value = rndPos
                                });

                                CommandBuffer.AddComponent(index, instance, new AgentComponent
                                {
                                    hasTarget = false,
                                    target = rndPos,
                                    agentStatus = AgentStatus.Idle
                                });
                            }
                        }
                    }

                    // Adding Components to every single Entity
                    CommandBuffer.AddComponent(index, instance, new MoveSpeedComponent
                    {
                        moveSpeed = 4.0f,
                        runningSpeed = 7.0f
                    }); // TODO randomize

                    CommandBuffer.AddComponent(index, instance, new BorderComponent
                    {
                        frontRight = _borderComponent.frontRight,
                        frontLeft = _borderComponent.frontLeft,
                        backRight = _borderComponent.backRight,
                        backLeft = _borderComponent.backLeft
                    });
                    CommandBuffer.AddComponent(index, instance, new InputComponent { });
                }
                // Destory spawner, so the system only runs once
                //CommandBuffer.DestroyEntity(index, entity);
                //CommandBuffer.RemoveComponent(index, entity, typeof(AllowToSpawnTag));

                // Disable this system to save performance. Otherwise this system will recreate n Agents again and again.
                // This system will be enabled when pressing the "1" key down (see ManagerSystem).
                World.Active.GetExistingSystem<UnitSpawnerSystem>().Enabled = false;
            }
        }
    }

    /// <summary>
    /// Runs on main thread, 1 times per frame. Stops when entity (dstManger) is destroyed.
    /// </summary>
    /// <param name="inputDeps"></param>
    /// <returns></returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Create Query for array creation (next steps)
        // Get crowd GameObject as an eneity.
        EntityQuery crowdEntity = GetEntityQuery(ComponentType.ReadOnly<UnitSpawnerComponent>());
        EntityQuery agentEntity = GetEntityQuery(ComponentType.ReadOnly<BorderComponent>());

        // Get the BorderComponent from the crowd entity
        NativeArray<BorderComponent> agentEntityBorderComponentArray = agentEntity.ToComponentDataArray<BorderComponent>(Allocator.TempJob);

        // Get the Spawner Data from the crowd entity
        NativeArray<UnitSpawnerComponent> crowdEntityUnitSpawnerComponentArray = crowdEntity.ToComponentDataArray<UnitSpawnerComponent>(Allocator.TempJob);

        NativeArray<float3> randomPositions = new NativeArray<float3>(
            crowdEntityUnitSpawnerComponentArray[0].AmountToSpawn,
            Allocator.TempJob);

        uint BaseSeed = (uint)UnityEngine.Random.Range(1, 100);

        for (int i = 0; i < randomPositions.Length; i++)
        {
            randomPositions[i] = new float3(
                UnityEngine.Random.Range(agentEntityBorderComponentArray[0].backLeft.x, agentEntityBorderComponentArray[0].backRight.x),
                .5f,
                UnityEngine.Random.Range(agentEntityBorderComponentArray[0].frontLeft.z, agentEntityBorderComponentArray[0].backLeft.z));
        }

        // Schedule spawnJob
        var spawnJob = new SpawnJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), // Create the commandBuffer
            randomPositions = randomPositions,
            BaseSeed = BaseSeed
        }.Schedule(this, inputDeps);

        agentEntityBorderComponentArray.Dispose();
        crowdEntityUnitSpawnerComponentArray.Dispose();
        //randomPositions.Dispose(); // Needs to be disposed twice because the job code only runs when pressing key "1"

        m_EntityCommandBufferSystem.AddJobHandleForProducer(spawnJob); // Execute the commandBuffer commands when spawnJob is finished
        return spawnJob;
    }
}
