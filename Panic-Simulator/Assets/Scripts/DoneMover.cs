using UnityEngine;
using System.Collections;
using Unity.Entities;

public class DoneMover : MonoBehaviour, IConvertGameObjectToEntity
{
    public float speed;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //dstManager.AddComponent(entity, typeof(MoveSpeed));
        MoveSpeed moveSpeed = new MoveSpeed { value = speed };
        dstManager.AddComponentData(entity, moveSpeed);
    }
}
