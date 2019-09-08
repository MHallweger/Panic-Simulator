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

        public bool keyOnePressedUp;
        public bool keyOnePressedDown;
        public bool keyTwoPressedUp;
        public bool keyTwoPressedDown;
        public bool keyThreePressedUp; // Bool for lifting finger from key 3 when rotating barriers outside
        public bool keyFourPressedUp; // Bool for lifting finger from key 4 when rotating barriers inside
        public bool keyFivePressedUp;
        public bool keySixPressedUp;

        public void Execute(ref InputComponent _inputComponent)
        {
            _inputComponent.leftClick = leftClick;
            _inputComponent.rightClick = rightClick;
            _inputComponent.keyOnePressedDown = keyOnePressedDown;
            _inputComponent.keyOnePressedUp = keyOnePressedUp;
            _inputComponent.keyTwoPressedDown = keyTwoPressedDown;
            _inputComponent.keyTwoPressedUp = keyTwoPressedUp;
            _inputComponent.keyThreePressedUp = keyThreePressedUp;
            _inputComponent.keyFourPressedUp = keyFourPressedUp;
            _inputComponent.keyFivePressedUp = keyFivePressedUp;
            _inputComponent.keySixPressedUp = keySixPressedUp;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var inputJob = new PlayerInputJob
        {
            leftClick = UnityEngine.Input.GetMouseButtonDown(0),
            rightClick = UnityEngine.Input.GetMouseButtonDown(1),
            keyOnePressedDown = UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha1), // add tag to crowd entity
            keyOnePressedUp = UnityEngine.Input.GetKeyUp(UnityEngine.KeyCode.Alpha1), // bool for allow system to spawn entitys
            keyTwoPressedDown = UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha2),
            keyTwoPressedUp = UnityEngine.Input.GetKeyUp(UnityEngine.KeyCode.Alpha2),
            keyThreePressedUp = UnityEngine.Input.GetKeyUp(UnityEngine.KeyCode.Alpha3),
            keyFourPressedUp = UnityEngine.Input.GetKeyUp(UnityEngine.KeyCode.Alpha4),
            keyFivePressedUp = UnityEngine.Input.GetKeyUp(UnityEngine.KeyCode.Alpha5),
            keySixPressedUp = UnityEngine.Input.GetKeyUp(UnityEngine.KeyCode.Alpha6)
            
        };
        return inputJob.Schedule(this, inputDeps);
    }
}