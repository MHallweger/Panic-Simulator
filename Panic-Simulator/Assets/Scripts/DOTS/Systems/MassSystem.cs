using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

/// <summary>
/// System that handles the overloaded bools for every single exit entity.
/// </summary>
public class MassSystem : JobComponentSystem
{
    /// <summary>
    /// Job that handles the overloaded bools for every single exit entity.
    /// </summary>
    [BurstCompile]
    public struct CalculateEntitysAroundExitJob : IJobForEachWithEntity<ExitComponent, Translation>
    {
        // Data from main thread
        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> nativeMultiHashMap;

        /// <summary>
        /// The actual calculation of the Entity amound around an exit entity.
        /// If the calculated amound is greater than x, set the overloaded bool to true. 
        /// Other Systems will react to this bool.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity index</param>
        /// <param name="_exitComponent">Current Entity ExitComponent</param>
        /// <param name="_translation">Current Entity Translation Component</param>
        public void Execute(Entity entity, int index, [WriteOnly] ref ExitComponent _exitComponent, [ReadOnly] ref Translation _translation)
        {
            int amount = 0;

            // Sum the amound of each quadrant around the exit entity
            amount = QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(_translation.Value)); // This quadrant
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(_translation.Value) + 1); // Right quadrant
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(_translation.Value) - 1); // Left quadrant
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(_translation.Value) + QuadrantSystem.quadrantYMultiplier); // Above quadrant
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(_translation.Value) - QuadrantSystem.quadrantYMultiplier); // Below quadrant

            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(_translation.Value) + 1 + QuadrantSystem.quadrantYMultiplier); // Corner Top Right
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(_translation.Value) - 1 + QuadrantSystem.quadrantYMultiplier); // Corner Top Left
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(_translation.Value) + 1 - QuadrantSystem.quadrantYMultiplier); // Corner Bottom Right
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(_translation.Value) - 1 - QuadrantSystem.quadrantYMultiplier); // Corner Bottom Left
            _exitComponent.amount = amount;

            if (amount > 10)
            {
                // Amount is greater than x, set overloaded to true
                _exitComponent.overloaded = true;
            }
            else
            {
                // Amount is less than x, set overloaded to false
                _exitComponent.overloaded = false;
            }
        }
    }

    /// <summary>
    /// Main Thread section, where Jobs are called and connected.
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>jobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Create CalculateEntitysAroundExitJob
        CalculateEntitysAroundExitJob calculateEntitysAroundExitJob = new CalculateEntitysAroundExitJob
        {
            nativeMultiHashMap = QuadrantSystem.quadrantMultiHashMap
        };

        // Schedule CalculateEntitysAroundExitJob with starting deps
        JobHandle jobHandle = calculateEntitysAroundExitJob.Schedule(this, inputDeps);

        jobHandle.Complete(); // because other jobs need access to the nativeMultiHashMap
        return jobHandle;
    }
}
