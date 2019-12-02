using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

/// <summary>
/// System that will be activated when border data changes. It will save the current border position of each corner.
/// </summary>
public class UpdateBordersSystem : JobComponentSystem
{
    /// <summary>
    /// Job that will be executed on every entity with BorderComponent.
    /// From main thread this Job gets the actual Border Data from MonoBehavior, this data will be transfered to ECS world.
    /// Save the MonoBehavior Data in BorderComponent of each entity with this Component.
    /// </summary>
    [BurstCompile]
    struct UpdateBordersJob : IJobForEachWithEntity<BorderComponent>
    {
        // Data from main thread
        [ReadOnly] public float3 frontLeft;
        [ReadOnly] public float3 frontRight;
        [ReadOnly] public float3 backLeft;
        [ReadOnly] public float3 backRight;

        /// <summary>
        /// Assign Border data to Border Components.
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="index">Current Entity ID</param>
        /// <param name="_borderComponent">BorderComponent of entity x</param>
        public void Execute(Entity entity, int index, [WriteOnly] ref BorderComponent _borderComponent)
        {
            // Assign each value to the borderComponent of each entity with this Component
            _borderComponent.frontLeft = frontLeft;
            _borderComponent.frontRight = frontRight;
            _borderComponent.backLeft = backLeft;
            _borderComponent.backRight = backRight;
        }
    }

    /// <summary>
    /// Job that runs after UpdateBordersJob to disable this Job. This way, [BurstCompile] can be used at UpdateBordersJob.
    /// The reason for this is that Burst cannot handle World access.
    /// </summary>
    struct DisableUpdateBordersJob : IJobForEach<BorderComponent>
    {
        public void Execute([ReadOnly] ref BorderComponent _borderComponent)
        {
            // Disable this whole System
            World.Active.GetExistingSystem<UpdateBordersSystem>().Enabled = false;
        }
    }

    /// <summary>
    /// Main Thread section, where Jobs are called and connected.
    /// This OnUpdate only runs one time.
    /// Because of the DisableUpdateBordersJob, the whole System will be disabled.
    /// </summary>
    /// <param name="inputDeps">starting deps</param>
    /// <returns>JobHandle</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new JobHandle();

        // Only start UpdateBordersJob when InputField of the Input window is not focused
        if (!InputWindow.instance.inputField.isFocused)
        {
            // Create a UpdateBordersJob and find each Corner GameObject in MonoBehavior
            UpdateBordersJob updateBordersJob = new UpdateBordersJob
            {
                frontLeft = UnityEngine.GameObject.Find("SpawnPoint_Front_Left").transform.position,
                frontRight = UnityEngine.GameObject.Find("SpawnPoint_Front_Right").transform.position,
                backLeft = UnityEngine.GameObject.Find("SpawnPoint_Back_Left_1").transform.position,
                backRight = UnityEngine.GameObject.Find("SpawnPoint_Back_Right_1").transform.position
            };

            // Schedule this Job with starting deps
            jobHandle = updateBordersJob.Schedule(this, inputDeps);

            // After that, create a DisableUpdateBordersJob to disable this whole System when BorderComponents are updated
            DisableUpdateBordersJob disableUpdateBordersJob = new DisableUpdateBordersJob
            {
            };

            // Schedule this Job with current jobHandle
            jobHandle = disableUpdateBordersJob.Schedule(this, jobHandle);
        }
        return jobHandle;
    }
}