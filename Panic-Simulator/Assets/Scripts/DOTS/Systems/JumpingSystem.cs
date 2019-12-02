using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

/// <summary>
/// System that reacts on changes of agentComponents of the agents. Creates an Jumping-"dancing" Animation to simulate human behavior on a festival.
/// </summary>
public class JumpingSystem : JobComponentSystem
{
    /// <summary>
    /// Handles Jumping Behavior for each agent.
    /// Runs on every entity with Translation, MoveSpeedComponent and Agent Component Component.
    /// </summary>
    [BurstCompile]
    public struct JumpingJob : IJobForEachWithEntity<Translation, MoveSpeedComponent, AgentComponent>
    {
        // Data from main thread
        public float deltaTime;

        /// <summary>
        /// Calculate a jumping animation on current translation.value.
        /// Sum y up to .9f, if the y value is at this point, move the agent back to the ground.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity index</param>
        /// <param name="_translation">Current Translation Component</param>
        /// <param name="_moveSpeedComponent">Current MoveSpeedComponent Component</param>
        /// <param name="_agentComponent">Current AgentComponent Component</param>
        public void Execute(Entity entity, int index, ref Translation _translation, ref MoveSpeedComponent _moveSpeedComponent, [ReadOnly] ref AgentComponent _agentComponent)
        {
            // If the agent just want to dance, handle this here
            if (!_agentComponent.hasTarget && _agentComponent.agentStatus == AgentStatus.Dancing)
            {
                // Per iteration, add a value to the y value
                _translation.Value.y += _moveSpeedComponent.jumpSpeed * deltaTime;

                // If y is at .9f, set the jumpSpeed to a negative value, to move the agent back to the ground
                if (_translation.Value.y > .9f)
                {
                    _moveSpeedComponent.jumpSpeed = -math.abs(_moveSpeedComponent.jumpSpeed);
                }

                // If y is at .5f, set the jumpSpeed back to positive, to move the agent back up
                if (_translation.Value.y < .5f)
                {
                    _moveSpeedComponent.jumpSpeed = +math.abs(_moveSpeedComponent.jumpSpeed);
                }
            }

            // If the Agent has a target and is in panic Mode, jump while Running
            else if (_agentComponent.hasTarget && _agentComponent.agentStatus == AgentStatus.Running)
            {
                // Per iteration, add a value to the y value
                // Use a different speed variable
                _translation.Value.y += _moveSpeedComponent.panicJumpSpeed * deltaTime;

                // If y is at .9f, set the jumpSpeed to a negative value, to move the agent back to the ground
                if (_translation.Value.y > .9f)
                {
                    _moveSpeedComponent.panicJumpSpeed = -math.abs(_moveSpeedComponent.panicJumpSpeed);
                }

                // If y is at .5f, set the jumpSpeed back to positive, to move the agent back up
                if (_translation.Value.y < .5f)
                {
                    _moveSpeedComponent.panicJumpSpeed = +math.abs(_moveSpeedComponent.panicJumpSpeed);
                }
            }

            // Get Agent back to the ground when it has Idle or Moving AgentStatus
            if ((_agentComponent.agentStatus == AgentStatus.Idle || _agentComponent.agentStatus == AgentStatus.Moving)
                && _translation.Value.y > .5f)
            {
                // Go back to normal position (Y value .5f)
                // Prevent from beeing in y > .5f while moving/running
                _translation.Value.y -= 5f * deltaTime;
            }
            #region Old Version (Physics version. Much influence on performance. Physics Scripts on Human Prefab needs to be enabled. Unity Physics needs to be installed into this project from the Package Manager)
            //if (!agentComponent.hasTarget && agentComponent.agentStatus == AgentStatus.Dancing && agentComponent.jumped == false) // Agent dont have a target, actual AgentStatus is Dancing
            //{
            //    physicsVelocity.Linear.y = 3f; // Add a force to the Agent's physics component
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
    /// Main Thread section, where Jobs are called and connected.
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>jobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Create JumpingJob 
        JumpingJob jumpingJob = new JumpingJob
        {
            deltaTime = UnityEngine.Time.deltaTime
        };

        // Scheduling JumpingJob
        JobHandle jobHandle = jumpingJob.Schedule(this, inputDeps);

        return jobHandle;
    }
}
