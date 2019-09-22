using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

/// <summary>
/// System that updates the current status of left and right click. The agents are now able to access the specific left/right click bool to see if one of both was clicked. 
/// The System also takes a look on different keys.
/// </summary>
public class InputSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem; // For creating the commandBuffer

    /// <summary>
    /// Initialize The EndSimulationEntityCommandBufferSystem commandBufferSystem.
    /// </summary>
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        // Create Dummy Entity:
        Entity dummyEntity = EntityManager.CreateEntity();
        EntityManager.AddComponent(dummyEntity, typeof(DummyComponent));
        EntityManager.SetName(dummyEntity, "DummyEntity");
    }

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

    struct CreateExitEntitys : IJobForEachWithEntity<DummyComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer; // instantiating and deleting of Entitys can only gets done on the main thread, save commands in buffer for main thread
        public float3 positionForExitEntity;

        public void Execute(Entity entity, int index, [ReadOnly] ref DummyComponent _dummyComponent)
        {
            Entity exitEntity = CommandBuffer.CreateEntity(index);
            CommandBuffer.AddComponent(index, exitEntity, new Translation { Value = positionForExitEntity });
            CommandBuffer.AddComponent(index, exitEntity, new ExitComponent { });
        }
    }

    float3 exitPosition;

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

        JobHandle jobHandle = inputJob.Schedule(this, inputDeps);

        if (Actions.instance.createExits) // Just for Exits placement
        {
            // Create Exits mode selected
            // Now every mouseClick (0) places an exit spot.
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                // Small Explosions in Radial menu was selected
                exitPosition = UnityEngine.Input.mousePosition;
                UnityEngine.Ray ray = UnityEngine.Camera.main.ScreenPointToRay(exitPosition);
                if (UnityEngine.Physics.Raycast(ray, out UnityEngine.RaycastHit hit))
                {
                    if (hit.collider != null)
                    {
                        exitPosition = new float3(hit.point.x, 0.5f, hit.point.z);

                        var createExitEntitysJob = new CreateExitEntitys
                        {
                            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), // Create the commandBuffer
                            positionForExitEntity = exitPosition
                        };

                        // Schedule this job when an exit spot is created with Mono behavior and the position is spotted here
                        jobHandle = createExitEntitysJob.Schedule(this, jobHandle);
                    }
                }
            }
        }
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle); // Execute the commandBuffer commands when spawnJob is finished
        return jobHandle;
    }
}