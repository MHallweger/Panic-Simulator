using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;

public class movesystem : JobComponentSystem
{

    //BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    /*protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }*/

    struct MoveJob : IJobForEachWithEntity<Translation, MoveSpeedComponent>
    {
        //public EntityCommandBuffer CommandBuffer;
        public float dt;

        public void Execute(Entity entity, int index, ref Translation translation, ref MoveSpeedComponent moveSpeedComponent)
        {
            translation.Value.y += moveSpeedComponent.speed * dt;

            if (translation.Value.y > 5f)
            {
                moveSpeedComponent.speed = math.abs(moveSpeedComponent.speed);
            }
            if (translation.Value.y < -5f)
            {
                moveSpeedComponent.speed = +math.abs(moveSpeedComponent.speed);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {

        var job = new MoveJob
        {
            //CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
            dt = UnityEngine.Time.deltaTime,
        }.ScheduleSingle(this, inputDeps);

       
        //m_EntityCommandBufferSystem.AddJobHandleForProducer(job);
        return job;
    }
}