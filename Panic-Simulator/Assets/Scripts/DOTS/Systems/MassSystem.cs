using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

public class MassSystem : JobComponentSystem
{
    [BurstCompile]
    public struct CalculateEntitysAroundExitJob : IJobForEachWithEntity<ExitComponent, Translation>
    {
        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> nativeMultiHashMap;

        public void Execute(Entity entity, int index, ref ExitComponent exitComponent, ref Translation translation)
        {
            // Dieser code läuft auf allen exits
            // Schaue, wieviele entitys sich auf translation.value befinden.
            // Wenn diese Anzahl größer 10 z.B ist dann setze einen Tag in exitComponent (muss noch erstellt werden)
            int amount = 0;
            amount = QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(translation.Value)); // This quadrant
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(translation.Value) + 1); // Right quadrant
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(translation.Value) - 1); // Left quadrant
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(translation.Value) + QuadrantSystem.quadrantYMultiplier); // Above quadrant
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(translation.Value) - QuadrantSystem.quadrantYMultiplier); // Below quadrant

            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(translation.Value) + 1 + QuadrantSystem.quadrantYMultiplier); // Corner Top Right
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(translation.Value) - 1 + QuadrantSystem.quadrantYMultiplier); // Corner Top Left
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(translation.Value) + 1 - QuadrantSystem.quadrantYMultiplier); // Corner Bottom Right
            amount += QuadrantSystem.GetEntityCountInHashMap(nativeMultiHashMap, QuadrantSystem.GetPositionHashMapKey(translation.Value) - 1 - QuadrantSystem.quadrantYMultiplier); // Corner Bottom Left
            exitComponent.amount = new float3(amount, 0, 0);
            if (amount > 10)
            {
                exitComponent.overloaded = true;
            }
            else if (amount > 20)
            {
                exitComponent.extremelyOverloaded = true;
            }
            else 
            {
                exitComponent.overloaded = false;
            }
        }
    }
    
    //public struct MassJob : IJobForEachWithEntity<AgentComponent>

    /// <summary>
    /// Runs on main thread, 1 times per frame
    /// </summary>
    /// <param name="inputDeps"></param>
    /// <returns></returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        CalculateEntitysAroundExitJob massJob = new CalculateEntitysAroundExitJob
        {
            nativeMultiHashMap = QuadrantSystem.quadrantMultiHashMap
        };
        JobHandle jobHandle = massJob.Schedule(this, inputDeps);

        jobHandle.Complete(); // because other jobs need access to the nativeMultiHashMap
        return jobHandle;
    }
}
