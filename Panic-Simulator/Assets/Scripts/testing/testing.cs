using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Rendering;
using Unity.Mathematics;

public class testing : MonoBehaviour
{
    #region Variables
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    #endregion // Variables
    private void Start()
    {
        EntityManager entityManager = World.Active.EntityManager;

        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MoveSpeedComponent)
            );

        NativeArray<Entity> entityArray = new NativeArray<Entity>(50000, Allocator.Temp);

        entityManager.CreateEntity(entityArchetype, entityArray);

        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            entityManager.SetComponentData(entity, new Translation
            {
                Value = new float3(UnityEngine.Random.Range(1000f, 1300f), UnityEngine.Random.Range(10f, 15f), UnityEngine.Random.Range(1000f, 1300f))
            });

            entityManager.SetComponentData(entity, new MoveSpeedComponent
            {
                speed = UnityEngine.Random.Range(2f, 4f)
            });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                material = material
            });
        }
        entityArray.Dispose();
    }
}