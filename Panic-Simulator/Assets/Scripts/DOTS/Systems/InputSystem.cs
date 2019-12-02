using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

/// <summary>
/// System that handles user input.
/// System that handles exit entity creation.
/// </summary>
public class InputSystem : JobComponentSystem
{
    // For creating the commandBuffer
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem; 

    /// <summary>
    /// Initialize The EndSimulationEntityCommandBufferSystem commandBufferSystem.
    /// Create Dummy Entity.
    /// </summary>
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        // Create Dummy Entity:
        Entity dummyEntity = EntityManager.CreateEntity();
        EntityManager.AddComponent(dummyEntity, typeof(DummyComponent));
        EntityManager.SetName(dummyEntity, "DummyEntity");
    }

    /// <summary>
    /// Job that handles different User Input.
    /// </summary>
    [BurstCompile]
    struct PlayerInputJob : IJobForEach<InputComponent>
    {
        // Data from main thread
        // Mono Behavior Input
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
        public bool keySevenPressedDown;
        public bool keySevenPressedUp;

        public bool entityInputisFocused;

        /// <summary>
        /// Assign Mono Behavior main thread Inputs to each entity with InputComponent.
        /// </summary>
        /// <param name="_inputComponent">Current Entity InputComponent</param>
        public void Execute([WriteOnly] ref InputComponent _inputComponent)
        {
            // Assign Inputs
            _inputComponent.leftClick = leftClick;
            _inputComponent.rightClick = rightClick;
            _inputComponent.keyThreePressedUp = keyThreePressedUp;
            _inputComponent.keyFourPressedUp = keyFourPressedUp;
            _inputComponent.keyFivePressedUp = keyFivePressedUp;
            _inputComponent.keySixPressedUp = keySixPressedUp;
            _inputComponent.entityInputisFocused = entityInputisFocused;

            if (!entityInputisFocused)
            {
                // Seperated keys: Only enable them when the Entity Input UI Field is not focused. Otherwise the bool will be set tu true, when you leave the UI Input field the bool will be set to true
                // and the specific action starts (e.g removing all exits).
                _inputComponent.keyOnePressedDown = keyOnePressedDown;
                _inputComponent.keyOnePressedUp = keyOnePressedUp;

                _inputComponent.keyTwoPressedDown = keyTwoPressedDown;
                _inputComponent.keyTwoPressedUp = keyTwoPressedUp;

                _inputComponent.keySevenPressedDown = keySevenPressedDown;
                _inputComponent.keySevenPressedUp = keySevenPressedUp;
            }
        }
    }

    /// <summary>
    /// Job that handles the exit entity creation.
    /// No Burst here because of the CommandBuffer and the World.Active access.
    /// </summary>
    struct CreateExitEntities : IJobForEachWithEntity<DummyComponent>
    {
        // Data from main thread
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float3 positionForExitEntity;

        /// <summary>
        /// Creating actual exit entities.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity index</param>
        /// <param name="_dummyComponent">Current _DummyComponent</param>
        public void Execute(Entity entity, int index, [ReadOnly] ref DummyComponent _dummyComponent)
        {
            // Create exit entity and add Components
            Entity exitEntity = CommandBuffer.CreateEntity(index);
            CommandBuffer.AddComponent(index, exitEntity, new Translation { Value = positionForExitEntity });
            CommandBuffer.AddComponent(index, exitEntity, new ExitComponent {});
            CommandBuffer.AddComponent(index, exitEntity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.Exit });

            // Disable Remove Exits System, otherwise the exit entity will instantly removed
            World.Active.GetExistingSystem<RemoveExitsSystem>().Enabled = false;
        }
    }

    // Variables for not creating new ones each time OnUpdate restarts
    #region // Variables
    float3 exitPosition;
    #endregion // Variables

    /// <summary>
    /// Main Thread section, where Jobs are called and connected.
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>jobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Creating PlayerInputJob
        PlayerInputJob inputJob = new PlayerInputJob
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
            keySixPressedUp = UnityEngine.Input.GetKeyUp(UnityEngine.KeyCode.Alpha6),
            keySevenPressedDown = UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha7),
            keySevenPressedUp = UnityEngine.Input.GetKeyUp(UnityEngine.KeyCode.Alpha7),
            entityInputisFocused = InputWindow.instance.inputField.isFocused
        };

        // Scheduling PlayerInputJob with starting deps
        JobHandle jobHandle = inputJob.Schedule(this, inputDeps);

        // Just for Exits placement
        if (Actions.instance.createExits)
        {
            // Create Exits mode selected
            // Now every mouseClick (0) places an exit spot.
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                // Save mouse position and check if hitted collider is a barrier.
                // If it is a barrie, save the exit position with the hitted object and create the CreateExitEntitiesJob
                // Disable the barrier GameObject visible
                exitPosition = UnityEngine.Input.mousePosition;
                UnityEngine.Ray ray = UnityEngine.Camera.main.ScreenPointToRay(exitPosition);
                if (UnityEngine.Physics.Raycast(ray, out UnityEngine.RaycastHit hit))
                {
                    if (hit.collider != null && hit.collider.gameObject.name != "ColliderGround" && hit.collider.gameObject.tag != "Truss")
                    {
                        exitPosition = new float3(hit.collider.gameObject.transform.position.x, 0.5f, hit.collider.gameObject.transform.position.z);

                        // Create CreateExitEntities
                        CreateExitEntities createExitEntitiesJob = new CreateExitEntities
                        {
                            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), // Create the commandBuffer
                            positionForExitEntity = exitPosition
                        };

                        // Disable barrier GameObject
                        //hit.collider.gameObject.transform.parent.gameObject.SetActive(false);
                        // Not the best implementation but there is no better solution inside ECS
                        // This code only runs shortly in the frame frame where the left mouse button is pressed down
                        // Disable the MeshRenderer of the first Child of the parent of the pin GameObject to disable the GameObjects visible 
                        // SetActive does not work for this situation
                        hit.collider.gameObject.transform.parent.gameObject.transform.GetChild(0).GetComponent<UnityEngine.MeshRenderer>().enabled = false;

                        // Schedule this job when an exit spot is created with Mono behavior and the position is spotted here
                        jobHandle = createExitEntitiesJob.Schedule(this, jobHandle);
                    }
                }
            }
        }

        // Execute the commandBuffer commands when spawnJob is finished
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}