using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;

namespace Unity.Transforms
{
    public class MoveForwardSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(MoveSpeed))]
        struct MoveForwardWithSpeed : IJobForEach<Translation, MoveSpeed>
        {
            public void Execute(ref Translation c0, ref MoveSpeed moveSpeed)
            {
                c0.Value = new float3(c0.Value.x + moveSpeed.value, c0.Value.y + moveSpeed.value, c0.Value.z + moveSpeed.value);
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var moveForwardWithSpeedJob = new MoveForwardWithSpeed();

            return moveForwardWithSpeedJob.Schedule(this, inputDeps);
        }
    }
}

