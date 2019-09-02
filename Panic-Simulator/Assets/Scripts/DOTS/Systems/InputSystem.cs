using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// System that updates the current status of left and right click. The agents are now able to access the specific left/right click bool to see if one of both was clicked. 
/// The System also takes a look on different keys.
/// </summary>
public class InputSystem : JobComponentSystem
{
    [BurstCompile]
    struct PlayerInputJob : IJobForEach<InputComponent>
    {
        public bool leftClick;
        public bool rightClick;
        public bool keyOnePressed;

        public void Execute(ref InputComponent inputComponent)
        {
            inputComponent.leftClick = leftClick;
            inputComponent.rightClick = rightClick;
            inputComponent.keyOnePressed = keyOnePressed;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {

        var inputJob = new PlayerInputJob
        {
            leftClick = UnityEngine.Input.GetMouseButtonDown(0),
            rightClick = UnityEngine.Input.GetMouseButtonDown(1),
            keyOnePressed = UnityEngine.Input.GetButton(UnityEngine.KeyCode.Alpha1.ToString())
        };
        return inputJob.Schedule(this, inputDeps);
    }
}