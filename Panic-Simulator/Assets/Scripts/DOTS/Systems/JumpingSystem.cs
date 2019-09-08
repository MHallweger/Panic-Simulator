using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Physics;

/// <summary>
/// System that reacts on changes of agentComponents of the agents. Creates an Jumping-"dancing" Animation to simulate human behavior on a festival.
/// </summary>
public class JumpingSystem : JobComponentSystem
{
    [BurstCompile]
    public struct JumpingJob : IJobForEachWithEntity<Translation, AgentComponent, PhysicsVelocity>
    {
        public void Execute(Entity entity, int index, ref Translation translation, ref AgentComponent agentComponent, ref PhysicsVelocity physicsVelocity)
        {
            if (!agentComponent.hasTarget && agentComponent.agentStatus == AgentStatus.Dancing && agentComponent.jumped == false) // Agent dont have a target, actual AgentStatus is Dancing
            {
                physicsVelocity.Linear.y = 3f;
                agentComponent.jumped = true;
            }

            if (translation.Value.y == 0.5f)
            {
                agentComponent.jumped = false;
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
        // Schedule moveJob
        var jumpingJob = new JumpingJob
        {
        }.Schedule(this, inputDeps);

        return jumpingJob;
    }
}
