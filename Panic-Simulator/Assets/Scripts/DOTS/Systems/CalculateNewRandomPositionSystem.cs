using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// JobComponentSystem that runs on worker threads.
/// Calculates new random positions based on translation.value.
/// </summary>
public class CalculateNewRandomPositionSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem; // For creating the commandBuffer

    /// <summary>
    /// Initialize The EndSimulationEntityCommandBufferSystem commandBufferSystem.
    /// </summary>
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    public static bool IsInsideFestivalArea(ref BorderComponent _borderComponent, float3 testPosition)
    {
        float3 a = _borderComponent.frontLeft;
        float3 b = _borderComponent.backLeft;
        float3 c = _borderComponent.backLeft;

        float3 d = _borderComponent.frontRight;
        float3 e = _borderComponent.backRight;
        float3 f = _borderComponent.backRight;

        // Left Triangle
        float as_x_i = testPosition.x - a.x;
        float as_z_i = testPosition.z - a.z;

        // Right Triangle
        float as_x_ii = testPosition.x - d.x;
        float as_z_ii = testPosition.z - d.z;

        bool s_ab = (b.x - a.x) * as_z_i - (b.z - a.z) * as_x_i > 0; // Front.Left to Back.Left 
        bool s_de = (e.x - d.x) * as_z_ii - (e.z - d.z) * as_x_ii > 0; // Front.Right to Back.Right

        if ((f.x - d.x) * as_z_ii - (d.z - d.z) * as_x_ii > 0 == s_de && (c.x - a.x) * as_z_i - (a.z - a.z) * as_x_i > 0 == s_ab)
        {
            if (testPosition.z >= _borderComponent.frontLeft.z
            && testPosition.x <= _borderComponent.backLeft.x
            && testPosition.x >= _borderComponent.backRight.x)
            {
                // If newRandomPosition is inside the festival area
                return true;
            }
        }
        else if ((f.x - e.x) * (testPosition.z - e.z) - (d.z - e.z) * (testPosition.x - e.x) > 0 != s_de
            && (c.x - b.x) * (testPosition.z - b.z) - (a.z - b.z) * (testPosition.x - b.x) > 0 != s_ab)
        {
            if (testPosition.z >= _borderComponent.frontLeft.z
            && testPosition.x <= _borderComponent.backLeft.x
            && testPosition.x >= _borderComponent.backRight.x)
            {
                // If newRandomPosition is inside the festival area, move to this position
                return true;
            }
        }
        return false;
    }

    [BurstCompile]
    struct CalculateNewRandomPositionBurstJob : IJobForEachWithEntity<Translation, AgentComponent, BorderComponent>
    {
        [NativeDisableParallelForRestriction]
        [DeallocateOnJobCompletion]
        public NativeArray<Random> RandomGenerator;

        [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex]
        private int threadIndex;

        public void Execute(Entity entity, int index, ref Translation _translation, ref AgentComponent _agentComponent, ref BorderComponent _borderComponent)
        {
            if (!_agentComponent.hasTarget && _agentComponent.agentStatus == AgentStatus.Moving)
            {
                var rnd = RandomGenerator[threadIndex - 1];
                float3 calculatedRandomPosition = float3.zero;

                if (_agentComponent.exitPointReached)
                {
                    if (_translation.Value.x >= 180f)
                    {
                        // Agent is on a right exit spot
                        // Calculate a random Position that points to the right side
                        calculatedRandomPosition = new float3(
                            rnd.NextFloat(_translation.Value.x + rnd.NextFloat(10f, 50f), _translation.Value.x + rnd.NextFloat(10f, 50f)),
                            .5f,
                            rnd.NextFloat(_translation.Value.z - rnd.NextFloat(10f, 50f), _translation.Value.z + rnd.NextFloat(10f, 50f)));
                    }
                    else
                    {
                        // Agent is on a left exit spot
                        // Calculate a random Position that points to the left side
                        calculatedRandomPosition = new float3(
                            rnd.NextFloat(_translation.Value.x - rnd.NextFloat(10f, 50f), _translation.Value.x - rnd.NextFloat(10f, 50f)),
                            .5f,
                            rnd.NextFloat(_translation.Value.z - rnd.NextFloat(10f, 50f), _translation.Value.z + rnd.NextFloat(10f, 50f)));
                    }
                }
                else
                {
                    calculatedRandomPosition = new float3(
                    rnd.NextFloat((_translation.Value.x - 3f), (_translation.Value.x + 3f)),
                    .5f,
                    rnd.NextFloat((_translation.Value.z - 3f), (_translation.Value.z + 3f)));
                }

                RandomGenerator[threadIndex - 1] = rnd; //This is necessary to update the state of the element inside the array.

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

                if ((f.x - d.x) * as_z_ii - (d.z - d.z) * as_x_ii > 0 == s_de && (c.x - a.x) * as_z_i - (a.z - a.z) * as_x_i > 0 == s_ab)
                {
                    if (_agentComponent.exitPointReached)
                    {
                        _agentComponent.target = calculatedRandomPosition;
                        _agentComponent.hasTarget = true;
                    }
                    else
                    {
                        // Outside festival area
                        // Exit point not reached
                        // Else if newRandomPosition is outside the festival area, stay with AgentStatus.Idle
                        _agentComponent.target = _translation.Value;
                        _agentComponent.hasTarget = false;
                    }

                    if (calculatedRandomPosition.z >= _borderComponent.frontLeft.z
                    && calculatedRandomPosition.x <= _borderComponent.backLeft.x
                    && calculatedRandomPosition.x >= _borderComponent.backRight.x
                    && !_agentComponent.exitPointReached)
                    {
                        // If newRandomPosition is inside the festival area, move to this position TODO kommentieren mehr
                        // Set values like target and agentStatus
                        _agentComponent.target = calculatedRandomPosition;
                        _agentComponent.hasTarget = true;
                    }
                }
                else if ((f.x - e.x) * (calculatedRandomPosition.z - e.z) - (d.z - e.z) * (calculatedRandomPosition.x - e.x) > 0 != s_de
                    && (c.x - b.x) * (calculatedRandomPosition.z - b.z) - (a.z - b.z) * (calculatedRandomPosition.x - b.x) > 0 != s_ab)
                {
                    if (_agentComponent.exitPointReached)
                    {
                        _agentComponent.target = calculatedRandomPosition;
                        _agentComponent.hasTarget = true;
                    }
                    else
                    {
                        // Outside festival area
                        // Exit point not reached
                        // Else if newRandomPosition is outside the festival area, stay with AgentStatus.Idle
                        _agentComponent.target = _translation.Value;
                        _agentComponent.hasTarget = false;
                    }

                    if (calculatedRandomPosition.z >= _borderComponent.frontLeft.z
                    && calculatedRandomPosition.x <= _borderComponent.backLeft.x
                    && calculatedRandomPosition.x >= _borderComponent.backRight.x
                    && !_agentComponent.exitPointReached)
                    {
                        // If newRandomPosition is inside the festival area, move to this position
                        // Set values like target and agentStatus
                        _agentComponent.target = calculatedRandomPosition;
                        _agentComponent.hasTarget = true;
                    }
                }
                else
                {
                    //if (_agentComponent.exitPointReached)
                    //{
                    //    _agentComponent.target = calculatedRandomPosition;
                    //    _agentComponent.hasTarget = true;
                    //}
                    //else
                    //{
                    //    // Outside festival area
                    //    // Exit point not reached
                    //    // Else if newRandomPosition is outside the festival area, stay with AgentStatus.Idle
                    //    _agentComponent.target = _translation.Value;
                    //    _agentComponent.hasTarget = false;
                    //}
                }
            }
        }
    }

    Random Rnd = new Random(1);
    NativeArray<Random> RandomGenerator;

    /// <summary>
    /// Runs on main thread, 1 times per frame
    /// </summary>
    /// <param name="inputDeps"></param>
    /// <returns></returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        RandomGenerator = new NativeArray<Random>(System.Environment.ProcessorCount, Allocator.TempJob);

        for (int i = 0; i < RandomGenerator.Length; i++)
        {
            RandomGenerator[i] = new Random((uint)Rnd.NextInt());
        }
        // Schedule job for passing the newPosition to each entity
        var calculateNewRandomPositionBurstjob = new CalculateNewRandomPositionBurstJob
        {
            RandomGenerator = RandomGenerator
        };

        JobHandle jobHandle = calculateNewRandomPositionBurstjob.Schedule(this, inputDeps);

        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle); // Execute the commandBuffer commands when spawnJob is finished

        return jobHandle;
    }
}
