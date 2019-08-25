using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Rendering;
using Unity.Mathematics;

public class Bootstrap : MonoBehaviour
{
    #region Variables
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private int amountToSpawn;
    #endregion // Variables
    private void Start()
    {
        EntityManager entityManager = World.Active.EntityManager;

        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
            );

        NativeArray<Entity> entityArray = new NativeArray<Entity>(amountToSpawn, Allocator.Temp);

        entityManager.CreateEntity(entityArchetype, entityArray);

        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            entityManager.SetComponentData(entity, new Translation
            {
                Value = new float3(UnityEngine.Random.Range(134.838f, 215.446f), 0.5f, UnityEngine.Random.Range(367.907f, 506.695f))
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