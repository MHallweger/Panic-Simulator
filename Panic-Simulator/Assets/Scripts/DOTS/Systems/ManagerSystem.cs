using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

/// <summary>
/// Helper System for the other Systems.
/// </summary>
[UpdateBefore(typeof(UnitSpawnerSystem))]
public class ManagerSystem : JobComponentSystem
{
    // For checking if an action was used
    // Can be used in other Systems
    public static bool actionUsed = false;

    /// <summary>
    /// Job that runs on every single agent to calculate human behavior while beeing not in panic mode.
    /// </summary>
    [BurstCompile]
    struct ManagerJob : IJobForEachWithEntity<AgentComponent, Translation>
    {
        // Data from main thread
        [NativeDisableParallelForRestriction] // Enables writing to any index of RandomGenerator
        [DeallocateOnJobCompletion]
        public NativeArray<Random> RandomGenerator; // For generating random values inside this job

        [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex]
        [ReadOnly]
        private int threadIndex; // For generating individual random values inside this job

        public bool actionUsed; // To check if an action has been used. So this System stops working on. (It's still online but it cannot pass the if case)

        /// <summary>
        /// Generate random values and assign them to different random behavior for the agents.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity index</param>
        /// <param name="_agentComponent">Current Entity AgentComponent</param>
        /// <param name="_translation">Current Entity Translation Component</param>
        public void Execute(Entity entity, int index, ref AgentComponent _agentComponent, ref Translation _translation)
        {
            if (!actionUsed)
            {
                // Only Start the random schedule process when the agent is on the festival area and it did not pass an exit yet
                var rnd = RandomGenerator[threadIndex - 1]; // Use the current threadIndex-1 to access a random Random value out of the RandomGenerator Native Array
                var dice = rnd.NextFloat(1000); // Use this random Random value to calculate a real random value for this job: value 0||1||2||...||1000

                // 30% Idle, 50% Jumping, 20% Moving
                if (dice >= 0f && dice <= 3f)
                {
                    // Stay where you are
                    // Idle mode
                    _agentComponent.target = _translation.Value;
                    _agentComponent.agentStatus = AgentStatus.Idle;
                    _agentComponent.hasTarget = false;
                }
                else if (dice >= 3f && dice <= 8f)
                {
                    // Trigger Jumping System to start operating on this agent
                    _agentComponent.target = _translation.Value;
                    _agentComponent.agentStatus = AgentStatus.Dancing;
                    _agentComponent.hasTarget = false;
                }
                else if (dice >= 8 && dice <= 10)
                {
                    // Trigger MovingSystem to start operating on this agent
                    _agentComponent.target = _translation.Value;
                    _agentComponent.agentStatus = AgentStatus.Moving;
                    _agentComponent.hasTarget = false;
                }
                RandomGenerator[threadIndex - 1] = rnd; // This is necessary to update the state of the element inside the array.
            }
        }
    }

    /// <summary>
    /// Open/Close different Systems to save performance.
    /// No Burst here because of the World.Active access.
    /// </summary>
    struct ManagerInputJob : IJobForEachWithEntity<InputComponent>
    {
        /// <summary>
        /// Enable/Disable different Systems.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity index</param>
        /// <param name="_inputComponent">Current Entity Input Component</param>
        public void Execute(Entity entity, int index, ref InputComponent _inputComponent)
        {
            if (_inputComponent.keyOnePressedDown)
            {
                // Enable/Disable the RemoveAgentsSystem and the UnitSpawnerSystem when agents are spawned with key 1
                World.Active.GetExistingSystem<RemoveAgentsSystem>().Enabled = false;
                World.Active.GetExistingSystem<UnitSpawnerSystem>().Enabled = true;
            }
            if (_inputComponent.keyFivePressedUp || _inputComponent.keySixPressedUp || _inputComponent.keyThreePressedUp || _inputComponent.keyFourPressedUp)
            {
                //Add/Remove/Change Barriers
                //Enable System that updates the spawn objects from the border component
                World.Active.GetExistingSystem<UpdateBordersSystem>().Enabled = true;
            }
            if (_inputComponent.keyTwoPressedUp)
            {
                // Enable the RemoveAgentsSystem to remove all agents
                World.Active.GetExistingSystem<RemoveAgentsSystem>().Enabled = true;
            }
            if (_inputComponent.keySevenPressedUp)
            {
                // Enable the RemoveExitsSystem to remove all exits
                World.Active.GetExistingSystem<RemoveExitsSystem>().Enabled = true;
            }
        }
    }

    /// <summary>
    /// Job that updates the user created entity amount in DOTS world.
    /// </summary>
    [BurstCompile]
    struct UpdateEntityAmount : IJobForEachWithEntity<UnitSpawnerComponent>
    {
        // Data from main thread
        public int newAmountToSpawn;

        /// <summary>
        /// Update the current Entity amount.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity index</param>
        /// <param name="_unitSpawnerComponent">Current Entity unitSpawnerComponent</param>
        public void Execute(Entity entity, int index, ref UnitSpawnerComponent _unitSpawnerComponent)
        {
            _unitSpawnerComponent.AmountToSpawn = newAmountToSpawn;
        }
    }

    // Variables for not creating new ones each time OnUpdate restarts
    #region Variables
    Random Rnd = new Random(1);
    NativeArray<Random> RandomGenerator;
    #endregion // Variables

    /// <summary>
    /// Main Thread section, where Jobs are called and connected.
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>jobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Initialize Native Array with the processorCount length and the TempJob Allocator tag
        RandomGenerator = new NativeArray<Random>(System.Environment.ProcessorCount, Allocator.TempJob);

        // Set actionUsed to true when an action was used
        if (Actions.instance.actionEnabled)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                actionUsed = true;
            }
        }

        // Fill the RandomGenerator with random Random objects
        for (int i = 0; i < RandomGenerator.Length; i++)
        {
            RandomGenerator[i] = new Random((uint)Rnd.NextInt());
        }

        // Create ManagerInputJob
        ManagerInputJob managerInputJob = new ManagerInputJob
        {
        };

        // Schedule ManagerInputJob with starting deps, save in jobHandle
        JobHandle jobHandle = managerInputJob.Schedule(this, inputDeps);

        // Create ManagerJob
        ManagerJob managerJob = new ManagerJob
        {
            RandomGenerator = RandomGenerator,
            actionUsed = actionUsed
        };

        // Schedule ManagerJob with jobHandle, save in jobHandle
        jobHandle = managerJob.Schedule(this, jobHandle);

        // Create UpdateEntityAmount Job
        UpdateEntityAmount updateEntityAmount = new UpdateEntityAmount
        {
            newAmountToSpawn = UnitSpawnerProxy.instance.AmountToSpawn
        };

        // Schedule UpdateEntityAmount Job with current jobHandle, save in jobHandle
        jobHandle = updateEntityAmount.Schedule(this, jobHandle);

        return jobHandle;
    }
}