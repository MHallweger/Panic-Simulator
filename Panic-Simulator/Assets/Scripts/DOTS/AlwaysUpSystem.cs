using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// System that calculates an vertical rotation for the agents.
/// </summary>
public class AlwaysUpSystem : JobComponentSystem
{
    [BurstCompile]
    struct UpJob : IJobForEachWithEntity<Rotation>
    {
        public void Execute(Entity entity, int index, ref Rotation rotation)
        {
            // Vertical rotation calculation
            if (rotation.Value.value.x != 0f
                && rotation.Value.value.y != 0f
                && rotation.Value.value.y != 0f)
            {
                rotation.Value.value = float4.zero;
                rotation.Value.value.w = 1f;
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
        // Schedule upJob
        var upJob = new UpJob
        {
        }.Schedule(this, inputDeps);

        return upJob;
    }
}
