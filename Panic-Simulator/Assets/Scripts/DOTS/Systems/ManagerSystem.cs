using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

// <summary>
// System that updates the current status of left and right click.The agents are now able to access the specific left/right click bool to see if one of both was clicked.
// The System also takes a look on different keys.
// </summary>
[UpdateBefore(typeof(UnitSpawnerSystem))]
public class ManagerSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        //World.Active.GetExistingSystem<RemoveExitsSystem>().Enabled = false;
    }

    //[BurstCompile] Burst does not support World.Active...
    struct ManagerJob : IJobForEachWithEntity<AgentComponent, Translation>
    {
        [NativeDisableParallelForRestriction]
        [DeallocateOnJobCompletion]
        public NativeArray<Random> RandomGenerator;

        [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex]
        [ReadOnly]
        private int threadIndex;

        // TODO: BUG: Siehe Trello, außerdem könnte man hier drauf auch verzichten und die Abfrage direkt im main thread machen, unnötig die variable hier rauf zu schicken.
        public bool actionUsed; // To check if an action has been used. So this System stops working on. (Its still online but it cannot pass the if case

        public void Execute(Entity entity, int index, ref AgentComponent agentComponent, ref Translation translation)
        {
            if (!actionUsed)
            {
                // Only Start the random schedule process when the agent is on the festival and do not pass an exit yet
                var rnd = RandomGenerator[threadIndex - 1];
                var dice = rnd.NextFloat(1000); // value 0||1||2||...||1000

                // 30% Idle, 50% Jumping, 20% Moving
                if (dice >= 0f && dice <= 3f)
                {
                    // Stay where you are
                    // Idle mode
                    agentComponent.target = translation.Value;
                    agentComponent.agentStatus = AgentStatus.Idle;
                    agentComponent.hasTarget = false;
                }
                else if (dice >= 3f && dice <= 8f)
                {
                    // Trigger Jumping System to start operating on this agent
                    agentComponent.target = translation.Value; // zur Sicherheit auf der gleichen Stelle bleiben
                    agentComponent.agentStatus = AgentStatus.Dancing;
                    agentComponent.hasTarget = false;
                }
                else if (dice >= 8 && dice <= 10)
                {
                    // Trigger MovingSystem to start operating on this agent
                    agentComponent.target = translation.Value;
                    agentComponent.agentStatus = AgentStatus.Moving;
                    agentComponent.hasTarget = false;
                }
                RandomGenerator[threadIndex - 1] = rnd; //This is necessary to update the state of the element inside the array.
            }
        }
    }

    struct ManagerInputJob : IJobForEachWithEntity<InputComponent>
    {
        public void Execute(Entity entity, int index, ref InputComponent inputComponent)
        {
            //If key 1 is pressed down. Add a allowToSpawn component to the spawn system, to allow spawning agents when pressing key 1 up.
            if (inputComponent.keyOnePressedDown)
            {
                World.Active.GetExistingSystem<RemoveAgentsSystem>().Enabled = false;
                World.Active.GetExistingSystem<UnitSpawnerSystem>().Enabled = true;
            }
            if (inputComponent.keyFivePressedUp || inputComponent.keySixPressedUp || inputComponent.keyThreePressedUp || inputComponent.keyFourPressedUp)
            {
                //Add / Remove Barriers
                //Enable System that updates the spawn objects from the border component
                World.Active.GetExistingSystem<UpdateBordersSystem>().Enabled = true;
            }
            if (inputComponent.keyTwoPressedUp)
            {
                World.Active.GetExistingSystem<RemoveAgentsSystem>().Enabled = true;
            }
            // Check if key 7 (remove exits) was pressed down
            if (inputComponent.keySevenPressedUp)
            {
                World.Active.GetExistingSystem<RemoveExitsSystem>().Enabled = true;
            }
        }
    }

    Random Rnd = new Random(1);
    NativeArray<Random> RandomGenerator;
    bool actionUsed = false;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        RandomGenerator = new NativeArray<Random>(System.Environment.ProcessorCount, Allocator.TempJob);

        if (Actions.instance.actionEnabled)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                actionUsed = true;
            }
        }

        for (int i = 0; i < RandomGenerator.Length; i++)
        {
            RandomGenerator[i] = new Random((uint)Rnd.NextInt());
        }

        var managerInputJob = new ManagerInputJob
        {
        };

        JobHandle jobHandle = managerInputJob.Schedule(this, inputDeps);

        var managerJob = new ManagerJob
        {
            RandomGenerator = RandomGenerator,
            actionUsed = actionUsed
        };

        jobHandle = managerJob.Schedule(this, jobHandle);
        return jobHandle;
    }
}