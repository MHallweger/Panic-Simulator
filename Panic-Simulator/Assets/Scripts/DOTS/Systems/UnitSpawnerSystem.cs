using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// System that runs only 1 time to spawn the agent entities.
/// </summary>
[UpdateAfter(typeof(ManagerSystem))]
public class UnitSpawnerSystem : JobComponentSystem
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
    /// System that spawns x agents on random positions and adds different Components to them.
    /// </summary>
    [ExcludeComponent(typeof(AgentComponent), typeof(MoveSpeedComponent))]
    struct SpawnJob : IJobForEachWithEntity<UnitSpawnerComponent, BorderComponent, InputComponent>
    {
        // Data from main thread
        public EntityCommandBuffer.Concurrent CommandBuffer; // instantiating and deleting of Entities can only gets done on the main thread, save commands in buffer for main thread later
        [ReadOnly] public uint BaseSeed; // For generating random values with Unity.Mathematics.Random.NextFloat()

        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<float3> randomPositions; // Native Array filled with random float3 positions

        [NativeDisableParallelForRestriction] // Enables writing to any index of RandomGenerator
        [DeallocateOnJobCompletion]
        public NativeArray<Random> RandomGenerator; // For generating random values inside this job

        [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex]
        [ReadOnly] private int threadIndex; // For generating individual random values inside this job

        /// <summary>
        /// Instantiate the Agent entities.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity index</param>
        /// <param name="_spawner">Current Entity Spawner Component</param>
        /// <param name="_borderComponent">Current Entity Border Component</param>
        /// <param name="_inputComponent">Current Entity Input Component</param>
        public void Execute(Entity entity, int index, [ReadOnly] ref UnitSpawnerComponent _spawner, [ReadOnly] ref BorderComponent _borderComponent, [ReadOnly] ref InputComponent _inputComponent)
        {
            var randomGenerator = RandomGenerator[threadIndex - 1];

            RandomGenerator[threadIndex - 1] = randomGenerator; // This is necessary to update the state of the element inside the array.

            var seed = (uint)(BaseSeed + index); // For Unity.Mathematics.Random --Slightly useless here because there is only one entity which calls this Execute()
            var rnd = new Random(seed); // Random Object for accessing rnd.NextFloat()

            if (_inputComponent.keyOnePressedUp) // Get access to the keyOnePressedUp bool in the Input System
            {
                // Spawn x Agents
                for (int i = 0; i < _spawner.AmountToSpawn; i++)
                {
                    // Duplicating the Prefab entity
                    var instance = CommandBuffer.Instantiate(index, _spawner.Prefab);
                    bool setPosition = false;
                    int loopIndex = 0; // For accessing other index array values in the while loop

                    float3 a = _borderComponent.frontLeft;
                    float3 b = _borderComponent.backLeft;
                    float3 c = _borderComponent.backLeft;

                    float3 d = _borderComponent.frontRight;
                    float3 e = _borderComponent.backRight;
                    float3 f = _borderComponent.backRight;

                    // Loops until finding a valid random array position value, this position must be inside of the festival area
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
                            // Position inside of the festival area, set position and leave this while loop in the next iteration
                            CommandBuffer.SetComponent(index, instance, new Translation
                            {
                                Value = randomPositions[i + loopIndex]
                            });

                            setPosition = true;
                        }
                        else if ((f.x - e.x) * (randomPositions[i + loopIndex].z - e.z) - (d.z - e.z) * (randomPositions[i + loopIndex].x - e.x) > 0 != s_de
                            && (c.x - b.x) * (randomPositions[i + loopIndex].z - b.z) - (a.z - b.z) * (randomPositions[i + loopIndex].x - b.x) > 0 != s_ab)
                        {
                            // Position inside of the festival area, set position and leave this while loop in the next iteration
                            CommandBuffer.SetComponent(index, instance, new Translation
                            {
                                Value = randomPositions[i + loopIndex]
                            });

                            setPosition = true;
                        }
                        else
                        {
                            // Position not inside of the festival area, increase LoopIndex if possible for accessing i++'th value in the next iteration
                            setPosition = false; // Inside of Triangle
                            if (i + loopIndex + 1.0f < randomPositions.Length)
                            {
                                loopIndex++;
                            }
                            else
                            {
                                // Index problem here
                                // Just create a random position and set it as Translation.value
                                setPosition = true;
                                float3 rndPos = new float3(
                                    rnd.NextFloat(_borderComponent.frontLeft.x, _borderComponent.frontRight.x),
                                    .5f,
                                    rnd.NextFloat(_borderComponent.backLeft.z, _borderComponent.backRight.z));

                                CommandBuffer.SetComponent(index, instance, new Translation
                                {
                                    Value = rndPos
                                });
                            }
                        }
                    }

                    // Adding Components to every single Entity
                    CommandBuffer.AddComponent(index, instance, new MoveSpeedComponent
                    {
                        moveSpeed = rnd.NextFloat(4.0f, 5.0f),
                        runningSpeed = rnd.NextFloat(3.0f, 4.0f), // before (7.0,9.0)
                        jumpSpeed = rnd.NextFloat(2.0f, 4.0f),
                        panicJumpSpeed = rnd.NextFloat(4.0f, 5.0f)
                    });

                    CommandBuffer.AddComponent(index, instance, new AgentComponent
                    {
                        hasTarget = false,
                        agentStatus = AgentStatus.Idle,
                        exitPointReached = false,
                        fleeProbability = 15.55f,
                    });

                    CommandBuffer.AddComponent(index, instance, new BorderComponent
                    {
                        frontRight = _borderComponent.frontRight,
                        frontLeft = _borderComponent.frontLeft,
                        backRight = _borderComponent.backRight,
                        backLeft = _borderComponent.backLeft
                    });
                    CommandBuffer.AddComponent(index, instance, new InputComponent { });

                    CommandBuffer.AddComponent(index, instance, new QuadrantEntity
                    {
                        typeEnum = QuadrantEntity.TypeEnum.Agent
                    });
                }

                // Disable this system to save performance. Otherwise this system will recreate n Agents again and again.
                // This system will be enabled when pressing the "1" key down (see ManagerSystem).
                ManagerSystem.actionUsed = false;
                World.Active.GetExistingSystem<UnitSpawnerSystem>().Enabled = false;
            }
        }
    }

    // Variables for not creating new ones each time OnUpdate restarts
    #region Variables
    Random Rnd = new Random(1);
    NativeArray<Random> RandomGenerator;
    #endregion // Variables

    /// <summary>
    /// Runs on main thread, 1 times per frame. Stops when entity (dstManger) is destroyed.
    /// </summary>
    /// <param name="inputDeps"></param>
    /// <returns>jobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Random Generator with a static ProcessorCount length
        // Used to generate individual random values inside a job
        RandomGenerator = new NativeArray<Random>(System.Environment.ProcessorCount, Allocator.TempJob);

        // Filled with random values
        // The access is via Thread Index later
        for (int i = 0; i < RandomGenerator.Length; i++)
        {
            RandomGenerator[i] = new Random((uint)Rnd.NextInt());
        }

        // Create Query for array creation (next steps)
        // Get crowd GameObject as an eneity.
        EntityQuery crowdEntity = GetEntityQuery(ComponentType.ReadOnly<UnitSpawnerComponent>());
        EntityQuery agentEntity = GetEntityQuery(ComponentType.ReadOnly<BorderComponent>());

        // Get the BorderComponent from the crowd entity
        NativeArray<BorderComponent> agentEntityBorderComponentArray = agentEntity.ToComponentDataArray<BorderComponent>(Allocator.TempJob);

        // Get the Spawner Data from the crowd entity
        NativeArray<UnitSpawnerComponent> crowdEntityUnitSpawnerComponentArray = crowdEntity.ToComponentDataArray<UnitSpawnerComponent>(Allocator.TempJob);

        // Create the randomPositions Native Array
        NativeArray<float3> randomPositions = new NativeArray<float3>(
            crowdEntityUnitSpawnerComponentArray[0].AmountToSpawn,
            Allocator.TempJob);

        // Value to create another seed value to create a random object to create a random value inside a job
        uint BaseSeed = (uint)UnityEngine.Random.Range(1, 100);

        // Fill the randomPositions array with random MonoBehavior float3 values
        for (int i = 0; i < randomPositions.Length; i++)
        {
            randomPositions[i] = new float3(
                UnityEngine.Random.Range(agentEntityBorderComponentArray[0].backLeft.x, agentEntityBorderComponentArray[0].backRight.x),
                .5f,
                UnityEngine.Random.Range(agentEntityBorderComponentArray[0].frontLeft.z, agentEntityBorderComponentArray[0].backLeft.z));
        }

        // Create SpawnJob
        SpawnJob spawnJob = new SpawnJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), // Create the commandBuffer
            randomPositions = randomPositions,
            BaseSeed = BaseSeed,
            RandomGenerator = RandomGenerator
        };

        // Schedule spawnJob with starting deps, save process inside jobHandle
        JobHandle jobHandle = spawnJob.Schedule(this, inputDeps);

        // Dispose Arrays when job finished
        agentEntityBorderComponentArray.Dispose();
        crowdEntityUnitSpawnerComponentArray.Dispose();

        // Execute the commandBuffer commands when spawnJob is finished
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
