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


    //[RequireComponentTag(typeof(DancingTag))]
    //[ExcludeComponent(typeof(IdleTag), typeof(MovingTag), typeof(RunningTag))]
    [BurstCompile]
    public struct JumpingJob : IJobForEachWithEntity<Translation, MoveSpeedComponent, AgentComponent>
    {
        public float deltaTime;

        public void Execute(Entity entity, int index, ref Translation translation, ref MoveSpeedComponent moveSpeedComponent, ref AgentComponent agentComponent)
        {

            if (!agentComponent.hasTarget && agentComponent.agentStatus == AgentStatus.Dancing)
            {
                translation.Value.y += moveSpeedComponent.jumpSpeed * deltaTime;

                if (translation.Value.y > .9f)
                {
                    moveSpeedComponent.jumpSpeed = -math.abs(moveSpeedComponent.jumpSpeed);
                }
                if (translation.Value.y < .5f)
                {
                    moveSpeedComponent.jumpSpeed = +math.abs(moveSpeedComponent.jumpSpeed);
                }
            }
            else if (agentComponent.hasTarget && agentComponent.agentStatus == AgentStatus.Running)
            {
                // Enable Panic-Running-Jumping Animation.
                // Use random jumpSpeed increaser to simulate different panic behavior
                translation.Value.y += moveSpeedComponent.panicJumpSpeed * deltaTime;

                if (translation.Value.y > .9f)
                {
                    moveSpeedComponent.panicJumpSpeed = -math.abs(moveSpeedComponent.panicJumpSpeed);
                }
                if (translation.Value.y < .5f)
                {
                    moveSpeedComponent.panicJumpSpeed = +math.abs(moveSpeedComponent.panicJumpSpeed);
                }
            }

            if ((agentComponent.agentStatus == AgentStatus.Idle || agentComponent.agentStatus == AgentStatus.Moving)
                && translation.Value.y > .5f)
            {
                // Go back to normal position (Y value .5f)
                // Prevent from beeing in y > .5f while moving/running
                translation.Value.y -= 5f * deltaTime;
            }
            #region Old Version (Physics version. Much influence on performance. Physics Scripts on Human Prefab needs to be enabled.)
            //if (!agentComponent.hasTarget && agentComponent.agentStatus == AgentStatus.Dancing && agentComponent.jumped == false) // Agent dont have a target, actual AgentStatus is Dancing
            //{
            //    physicsVelocity.Linear.y = 3f;
            //    agentComponent.jumped = true;
            //} 

            //if (translation.Value.y == 0.5f)
            //{
            //    agentComponent.jumped = false;
            //}
            #endregion // Old Version (Physics version. Much influence on performance. Physics Scripts on Human Prefab needs to be enabled.)
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
            deltaTime = UnityEngine.Time.deltaTime
    }.Schedule(this, inputDeps);

        return jumpingJob;
    }
}
