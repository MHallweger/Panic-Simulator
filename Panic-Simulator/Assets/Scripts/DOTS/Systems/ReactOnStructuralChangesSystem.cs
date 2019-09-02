//using System.Collections;
//using System.Collections.Generic;
//using Unity.Burst;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Mathematics;

///// <summary>
///// System that reacts on changes of the festival area.
///// </summary>
//public class ReactOnStructuralChangesSystem : JobComponentSystem
//{
//    [BurstCompile]
//    struct ReactOnStructuralChangesJob : IJobForEach<BorderComponent, InputComponent>
//    {
//        public void Execute(ref BorderComponent borderComponent, [ReadOnly] ref InputComponent inputComponent)
//        {
//            if (inputComponent.keyOnePressed)
//            {
//                // Key "1" holding down

//            }
//        }
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {

//        var reactOnStructuralChangesJob = new ReactOnStructuralChangesJob
//        {
//        };
//        return reactOnStructuralChangesJob.Schedule(this, inputDeps);
//    }
//}