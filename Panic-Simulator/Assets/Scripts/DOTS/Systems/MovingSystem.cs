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
    /// <summary>
    /// Handles the Agent movement.
    /// Runs on every entity with Translation, AgentComponent, MoveSpeedComponent Component.
    /// </summary>
    [BurstCompile]
    public struct MovementBurstJob : IJobForEachWithEntity<Translation, AgentComponent, MoveSpeedComponent>
    {
        // Data from main thread
        public float deltaTime;

        /// <summary>
        /// Handles moving behavior when Agent got the moving Tag in AgentStatus.
        /// Also check the actual y value when Agent is in Idle mode.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity index</param>
        /// <param name="_translation">Current Entity Translation Component</param>
        /// <param name="_agentComponent">Current Entity AgentComponent Component</param>
        /// <param name="_moveSpeedComponent">Current Entity MoveSpeedComponent Component</param>
        public void Execute(Entity entity, int index, ref Translation _translation, ref AgentComponent _agentComponent, [ReadOnly] ref MoveSpeedComponent _moveSpeedComponent)
        {
            // If the agent has a target and the Moving AgentStatus
            if (_agentComponent.hasTarget && _agentComponent.agentStatus == AgentStatus.Moving)
            {
                // Calculations for checking conditions and calculating the new translation.value
                float3 direction = math.normalize(_agentComponent.target - _translation.Value);
                float distance = math.distance(_agentComponent.target, _translation.Value);

                if (distance > .5f) // agent far away
                {
                    _translation.Value += direction * _moveSpeedComponent.moveSpeed * deltaTime;
                }
                else if (distance < .5f) // close
                {
                    _agentComponent.hasTarget = false;
                }
            }

            if (_agentComponent.agentStatus == AgentStatus.Idle && _translation.Value.y > .5f)
            {
                // Agent do not have a target -> Idle
                // y value is greater than .5f -> move agent down to the ground
                _translation.Value.y -= 1f * deltaTime;
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
        // Create MovementBurstJob
        MovementBurstJob movementBurstJob = new MovementBurstJob
        {
            deltaTime = UnityEngine.Time.deltaTime
        };

        // Schedule MovementBurstJob with starting deps
        JobHandle jobHandle = movementBurstJob.Schedule(this, inputDeps);
        return jobHandle;
    }
}
