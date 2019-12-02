using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

/// <summary>
/// System that reacts on Running AgentStatus.
/// </summary>
public class RunningSystem : JobComponentSystem
{
    /// <summary>
    /// Calculates the Running Behavior on Agents with Translation, AgentComponent and MoveSpeedComponent.
    /// </summary>
    [BurstCompile]
    public struct RunningJob : IJobForEachWithEntity<Translation, AgentComponent, MoveSpeedComponent>
    {
        // Data from main thread
        [ReadOnly] public float deltaTime;

        /// <summary>
        /// React on different situations and move the current Agent.
        /// </summary>
        /// <param name="entity">Current entity</param>
        /// <param name="index">Current entity index</param>
        /// <param name="_translation">Current Entity Translation Component</param>
        /// <param name="_agentComponent">Current Entity Agent Component</param>
        /// <param name="_moveSpeedComponent">Current Entity Move Speed Component</param>
        public void Execute(Entity entity, int index, ref Translation _translation, ref AgentComponent _agentComponent, [ReadOnly] ref MoveSpeedComponent _moveSpeedComponent)
        {
            // If an agent got the running Tag and has a Target
            if (_agentComponent.agentStatus == AgentStatus.Running && _agentComponent.hasTarget)
            {
                // Calculate the distance between this agent and the target
                float distance = math.distance(_translation.Value, _agentComponent.target);

                // [ELSE] If this distance is greater than .1f (far away), calculate the direction with the target and the current agent position
                // and add this direction to the current position value
                // [IF] If the distance is less than .1f, change mode because the agent reached its target
                // Take a look on the target, if the target is an exit spot, change to moving and move away from the exit spot (Moving System does its job)
                // The other case is just a normal randomly generated position that has been reached, in both cases, change bools to trigger other Systems/Jobs
                if (distance < .1f)
                {
                    if (_agentComponent.foundFinalExitPoint)
                    {
                        // Agent is near to the target
                        _agentComponent.agentStatus = AgentStatus.Moving;
                        _agentComponent.hasTarget = false;
                        _agentComponent.exitPointReached = true; // The CalculateNewRandomPositionSystem is able now to seperate the agents and allow the correct new random generated positions
                        _agentComponent.marked = false;
                    }
                    else
                    {
                        _agentComponent.hasTarget = false;
                        _agentComponent.marked = false;
                    }
                }
                else
                {
                    // Agent has panic, need to run to the next closest escape spot
                    float3 direction = math.normalize(_agentComponent.target - _translation.Value);
                    _translation.Value += direction * _moveSpeedComponent.runningSpeed * deltaTime;
                }
            }
        }
    }

    /// <summary>
    /// Runs on main thread, 1 times per frame
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>jobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Creating Running Job
        RunningJob runningJob = new RunningJob
        {
            deltaTime = UnityEngine.Time.deltaTime,
        };

        // Scheduling runningJob
        JobHandle jobHandle = runningJob.Schedule(this, inputDeps);

        return jobHandle;
    }
}
