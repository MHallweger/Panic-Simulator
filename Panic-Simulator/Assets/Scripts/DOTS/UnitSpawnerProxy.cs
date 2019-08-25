using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// Authoring
/// <summary>
/// Starting Area of this project.
/// </summary>
[RequiresEntityConversion]
public class UnitSpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    // MonoBehaviour Variables
    [SerializeField] private GameObject Prefab;
    [SerializeField] private int CountX;
    [SerializeField] private int CountY;
    [SerializeField] private float frontRight_x;
    [SerializeField] private float frontLeft_x;
    [SerializeField] private float frontLeftRight_z;
    [SerializeField] private float backLeftRight_z;

    // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(Prefab);
    }

    // Convert the editor data representation to the entity optimal runtime representation
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new UnitSpawnerComponent
        {
            // The referenced prefab will be converted due to DeclareReferencedPrefabs
            // Mapping the game object to an entity reference
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            CountX = CountX,
            CountY = CountY
        };

        var borderData = new BorderComponent
        {
            frontRight_x = frontRight_x, //134.838f,
            frontLeft_x = frontLeft_x, //215.446f,
            frontLeftRight_z = frontLeftRight_z, //367.907f,
            backLeftRight_z = backLeftRight_z //506.695f
        };

        dstManager.AddComponentData(entity, spawnerData); // SYNC POINT //
        dstManager.AddComponentData(entity, borderData); // SYNC POINT //
    }
}