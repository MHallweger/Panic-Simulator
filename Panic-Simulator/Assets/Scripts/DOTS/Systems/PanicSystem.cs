﻿//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Collections;
//using Unity.Transforms;
//using Unity.Burst;
//using Unity.Rendering;
///// <summary>
///// System that reacts on panic spots, generated by mouse click.
///// </summary>
//public class PanicSystem : JobComponentSystem
//{
//    //[BurstCompile]
//    public struct PanicJob : IJobForEachWithEntity<Translation, AgentComponent, InputComponent>
//    {
//        public float3 mousePosition; // The exact mouse x and z mouse position. y position is set to 0.5f

//        public void Execute(Entity entity, int index, ref Translation translation, ref AgentComponent agentComponent, [ReadOnly] ref InputComponent inputComponent)
//        {
//            if (inputComponent.leftClick)
//            {
//                float3 panicSpot = new float3(mousePosition.x, .5f, mousePosition.z);
//                float distance = math.distance(translation.Value, panicSpot);

//                if (distance <= 10f) // ExplosionPowerDistance // TODO: integrate it into component?
//                {
//                    // Agent is very close to panic spot, Start running
//                    agentComponent.agentStatus = AgentStatus.Running;
//                    agentComponent.target = new float3(133.8f, .5f, 389.54f);
//                    agentComponent.hasTarget = true;
//                }
//            }
//        }
//    }
//    /// <summary>
//    /// Runs on main thread, 1 times per frame
//    /// </summary>
//    /// <param name="inputDeps"></param>
//    /// <returns></returns>
//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        var mousePosition = UnityEngine.Input.mousePosition;
//        UnityEngine.Ray ray = UnityEngine.Camera.main.ScreenPointToRay(mousePosition);
//        if (UnityEngine.Physics.Raycast(ray, out UnityEngine.RaycastHit hit))
//        {
//            if (hit.collider != null)
//            {
//                mousePosition = new float3(hit.point.x, .5f, hit.point.z);
//            }
//        }

//        // Schedule panicJob
//        var panicJob = new PanicJob
//        {
//            mousePosition = mousePosition
//        }.Schedule(this, inputDeps);

//        return panicJob;
//    }
//}
