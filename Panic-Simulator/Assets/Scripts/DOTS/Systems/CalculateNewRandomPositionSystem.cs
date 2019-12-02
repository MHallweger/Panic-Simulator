using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class CalculateNewRandomPositionSystem : JobComponentSystem
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
    /// Method that checks if a position is inside the festival area.
    /// </summary>
    /// <param name="_borderComponent">Used BorderComponent</param>
    /// <param name="_testPosition">Checked test position</param>
    /// <returns>The information if the given _testPosition is inside the festival area</returns>
    [BurstCompile]
    public static bool IsInsideFestivalArea(ref BorderComponent _borderComponent, float3 _testPosition)
    {
        // All different festival area corners, saved in different variables, used for later calculations
        float3 a = _borderComponent.frontLeft;
        float3 b = _borderComponent.backLeft;
        float3 c = _borderComponent.backLeft;

        float3 d = _borderComponent.frontRight;
        float3 e = _borderComponent.backRight;
        float3 f = _borderComponent.backRight;

        // Left Triangle
        float as_x_i = _testPosition.x - a.x;
        float as_z_i = _testPosition.z - a.z;

        // Right Triangle
        float as_x_ii = _testPosition.x - d.x;
        float as_z_ii = _testPosition.z - d.z;

        bool s_ab = (b.x - a.x) * as_z_i - (b.z - a.z) * as_x_i > 0; // Front.Left to Back.Left 
        bool s_de = (e.x - d.x) * as_z_ii - (e.z - d.z) * as_x_ii > 0; // Front.Right to Back.Right

        // Mathematical caluclation if given position is not inside the 2 triangles of the whole square [1]
        if ((f.x - d.x) * as_z_ii - (d.z - d.z) * as_x_ii > 0 == s_de && (c.x - a.x) * as_z_i - (a.z - a.z) * as_x_i > 0 == s_ab)
        {
            // Additional check if z value is greater than front.z and x value is between back left.x and back right.x
            if (_testPosition.z >= _borderComponent.frontLeft.z
            && _testPosition.x <= _borderComponent.backLeft.x
            && _testPosition.x >= _borderComponent.backRight.x)
            {
                // _testPosition is inside festival area
                return true;
            }
        }
        // Mathematical caluclation if given position is not inside the 2 triangles of the whole square [2]
        else if ((f.x - e.x) * (_testPosition.z - e.z) - (d.z - e.z) * (_testPosition.x - e.x) > 0 != s_de
            && (c.x - b.x) * (_testPosition.z - b.z) - (a.z - b.z) * (_testPosition.x - b.x) > 0 != s_ab)
        {
            // Additional check if z value is greater than front.z and x value is between back left.x and back right.x
            if (_testPosition.z >= _borderComponent.frontLeft.z
            && _testPosition.x <= _borderComponent.backLeft.x
            && _testPosition.x >= _borderComponent.backRight.x)
            {
                // _testPosition is inside festival area
                return true;
            }
        }
        // no match -> _testPosition is not inside the festival area
        return false;
    }

    /// <summary>
    /// System that runs on every Agent Entity, to check if this agent is in Moving AgentStatus with no current target.
    /// If this is the case, calculate a new one.
    /// </summary>
    [BurstCompile]
    struct CalculateNewRandomPositionBurstJob : IJobForEachWithEntity<Translation, AgentComponent, BorderComponent>
    {
        // Data from main thread
        [NativeDisableParallelForRestriction] // Enables writing to any index of RandomGenerator
        [DeallocateOnJobCompletion]
        public NativeArray<Random> RandomGenerator; // For generating random values inside this job

        [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex]
        private int threadIndex; // For generating individual random values inside this job

        /// <summary>
        /// Case 1: Agent reached goal at exit and is in Moving AgentStatus now. -> Calculate a new random position outside the festival area.
        /// Case 2: Agent reached it's random goal inside the festival area and need a new one now. -> Calculate a new random position inside the festival area. Check if this random position is valid.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity index</param>
        /// <param name="_translation">Current Entity Translation Component</param>
        /// <param name="_agentComponent">Current Entity AgentComponent</param>
        /// <param name="_borderComponent">Current Entity Border Component</param>
        public void Execute(Entity entity, int index, ref Translation _translation, ref AgentComponent _agentComponent, ref BorderComponent _borderComponent)
        {
            // If an Agent is in Moving AgentStauts and don't have a target yet
            if (!_agentComponent.hasTarget && _agentComponent.agentStatus == AgentStatus.Moving)
            {
                // initialization
                var rnd = RandomGenerator[threadIndex - 1];
                float3 calculatedRandomPosition = float3.zero;

                // Case 1: Agent reached exit, check which side and calculate a random position outside the festival area
                if (_agentComponent.exitPointReached)
                {
                    if (_translation.Value.x >= 180f)
                    {
                        // Agent is on a left exit spot
                        // Calculate a random Position that points to the right side
                        calculatedRandomPosition = new float3(
                            rnd.NextFloat(_translation.Value.x + rnd.NextFloat(3f, 6f), _translation.Value.x + rnd.NextFloat(3f, 6f)),
                            .5f,
                            rnd.NextFloat(_translation.Value.z - rnd.NextFloat(3f, 4f), _translation.Value.z + rnd.NextFloat(3f, 4f)));
                    }
                    else
                    {
                        // Agent is on a right exit spot
                        // Calculate a random Position that points to the left side
                        calculatedRandomPosition = new float3(
                            rnd.NextFloat(_translation.Value.x - rnd.NextFloat(3f, 6f), _translation.Value.x - rnd.NextFloat(3f, 6f)),
                            .5f,
                            rnd.NextFloat(_translation.Value.z - rnd.NextFloat(3f, 4f), _translation.Value.z + rnd.NextFloat(3f, 4f)));
                    }
                }
                else
                {
                    // Case 2: Agent reached it's old random position inside the festival area, calculate a new one
                    calculatedRandomPosition = new float3(
                    rnd.NextFloat((_translation.Value.x - 3f), (_translation.Value.x + 3f)),
                    .5f,
                    rnd.NextFloat((_translation.Value.z - 3f), (_translation.Value.z + 3f)));
                }

                RandomGenerator[threadIndex - 1] = rnd; // This is necessary to update the state of the element inside the array.

                // ##### Check if calculated random festival position is valid ##### //

                // All different festival area corners, saved in different variables, used for later calculations
                float3 a = _borderComponent.frontLeft;
                float3 b = _borderComponent.backLeft;
                float3 c = _borderComponent.backLeft;

                float3 d = _borderComponent.frontRight;
                float3 e = _borderComponent.backRight;
                float3 f = _borderComponent.backRight;

                // Left Triangle
                float as_x_i = calculatedRandomPosition.x - a.x;
                float as_z_i = calculatedRandomPosition.z - a.z;

                // Right Triangle
                float as_x_ii = calculatedRandomPosition.x - d.x;
                float as_z_ii = calculatedRandomPosition.z - d.z;

                bool s_ab = (b.x - a.x) * as_z_i - (b.z - a.z) * as_x_i > 0; // Front.Left to Back.Left 
                bool s_de = (e.x - d.x) * as_z_ii - (e.z - d.z) * as_x_ii > 0; // Front.Right to Back.Right

                // Mathematical caluclation if given position is not inside the 2 triangles of the whole square [1]
                if ((f.x - d.x) * as_z_ii - (d.z - d.z) * as_x_ii > 0 == s_de && (c.x - a.x) * as_z_i - (a.z - a.z) * as_x_i > 0 == s_ab)
                {
                    // Outside festival area
                    if (_agentComponent.exitPointReached)
                    {
                        // No validation needed
                        _agentComponent.target = calculatedRandomPosition;
                        _agentComponent.hasTarget = true;
                    }
                    else
                    {
                        // Outside festival area
                        // Exit point not reached
                        _agentComponent.target = _translation.Value;
                        _agentComponent.hasTarget = false;
                    }

                    // Additional check if z value is greater than front.z and x value is between back left.x and back right.x
                    if (calculatedRandomPosition.z >= _borderComponent.frontLeft.z
                    && calculatedRandomPosition.x <= _borderComponent.backLeft.x
                    && calculatedRandomPosition.x >= _borderComponent.backRight.x
                    && !_agentComponent.exitPointReached)
                    {
                        _agentComponent.target = calculatedRandomPosition;
                        _agentComponent.hasTarget = true;
                    }
                }

                // Mathematical caluclation if given position is not inside the 2 triangles of the whole square [2]
                else if ((f.x - e.x) * (calculatedRandomPosition.z - e.z) - (d.z - e.z) * (calculatedRandomPosition.x - e.x) > 0 != s_de
                    && (c.x - b.x) * (calculatedRandomPosition.z - b.z) - (a.z - b.z) * (calculatedRandomPosition.x - b.x) > 0 != s_ab)
                {
                    if (_agentComponent.exitPointReached)
                    {
                        // No validation needed
                        _agentComponent.target = calculatedRandomPosition;
                        _agentComponent.hasTarget = true;
                    }
                    else
                    {
                        // Outside festival area
                        // Exit point not reached
                        _agentComponent.target = _translation.Value;
                        _agentComponent.hasTarget = false;
                    }

                    // Additional check if z value is greater than front.z and x value is between back left.x and back right.x
                    if (calculatedRandomPosition.z >= _borderComponent.frontLeft.z
                    && calculatedRandomPosition.x <= _borderComponent.backLeft.x
                    && calculatedRandomPosition.x >= _borderComponent.backRight.x
                    && !_agentComponent.exitPointReached)
                    {
                        _agentComponent.target = calculatedRandomPosition;
                        _agentComponent.hasTarget = true;
                    }
                }
                else
                {
                    // Inside Festival
                    if (_agentComponent.exitPointReached)
                    {
                        // No validation needed
                        _agentComponent.target = calculatedRandomPosition;
                        _agentComponent.hasTarget = true;
                    }
                    else
                    {
                        // Outside festival area
                        // Exit point not reached
                        _agentComponent.target = _translation.Value;
                        _agentComponent.hasTarget = false;
                    }
                }
            }
        }
    }

    // Variables for not creating new ones each time OnUpdate restarts
    #region Variables
    Random Rnd = new Random(1);
    NativeArray<Random> RandomGenerator;
    #endregion // Variables

    /// <summary>
    /// Main Thread section, where Jobs are called and connected.
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>jobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Initialize Native Array with the processorCount length and the TempJob Allocator tag
        RandomGenerator = new NativeArray<Random>(System.Environment.ProcessorCount, Allocator.TempJob);

        // Fill the RandomGenerator with random Random objects
        for (int i = 0; i < RandomGenerator.Length; i++)
        {
            RandomGenerator[i] = new Random((uint)Rnd.NextInt());
        }

        // Create CalculateNewRandomPositionBurstJob
        CalculateNewRandomPositionBurstJob calculateNewRandomPositionBurstjob = new CalculateNewRandomPositionBurstJob
        {
            RandomGenerator = RandomGenerator
        };

        // Schedule CalculateNewRandomPositionBurstJob with starting deps
        JobHandle jobHandle = calculateNewRandomPositionBurstjob.Schedule(this, inputDeps);

        // Execute the commandBuffer commands when spawnJob is finished
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
