using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

/// <summary>
/// System that reacts on changes of agentComponents of the agents. Moves the agent to the actual target position.
/// </summary>
public class MovingSystem : JobComponentSystem
{
    [BurstCompile]
    public struct MovementBurstJob : IJobForEachWithEntity<Translation, AgentComponent, MoveSpeedComponent>
    {
        public float deltaTime;

        public void Execute(Entity entity, int index, ref Translation translation, ref AgentComponent agentComponent, [ReadOnly] ref MoveSpeedComponent moveSpeedComponent)
        {
            if (agentComponent.hasTarget && agentComponent.agentStatus == AgentStatus.Moving)
            {
                // Calculations for checking conditions and calculating the new translation.value
                float3 direction = math.normalize(agentComponent.target - translation.Value);
                float distance = math.distance(agentComponent.target, translation.Value);

                if (distance > .5f) // agent far away
                {
                    translation.Value += direction * moveSpeedComponent.moveSpeed * deltaTime;
                }
                else if (distance < .5f) // closer/close
                {
                    agentComponent.hasTarget = false;
                }
            }

            if (/*!agentComponent.hasTarget && */agentComponent.agentStatus == AgentStatus.Idle && translation.Value.y > .5f)
            {
                // Agent do not have a target [IDLE]
                translation.Value.y -= 1f * deltaTime;
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
        var moveBurstJob = new MovementBurstJob
        {
            deltaTime = UnityEngine.Time.deltaTime
        };
        JobHandle jobHandle = moveBurstJob.Schedule(this, inputDeps);

        return jobHandle;
    }
}
