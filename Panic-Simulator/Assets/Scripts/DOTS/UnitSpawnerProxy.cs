using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Authoring
/// Starting Area of this project.
/// </summary>
[RequiresEntityConversion]
public class UnitSpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    // MonoBehaviour Variables
    [SerializeField] private GameObject Prefab;
    [SerializeField] private GameObject frontRight;
    [SerializeField] private GameObject frontLeft;
    [SerializeField] private GameObject backRight;
    [SerializeField] private GameObject backLeft;
    public int AmountToSpawn;
    public static UnitSpawnerProxy instance;

    private void Awake()
    {
        instance = this;
    }

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
            AmountToSpawn = AmountToSpawn
        };

        var borderData = new BorderComponent
        {
            frontRight = new Vector3(frontRight.transform.position.x, frontRight.transform.position.y, frontRight.transform.position.z),
            frontLeft = new Vector3(frontLeft.transform.position.x, frontLeft.transform.position.y, frontLeft.transform.position.z),
            backRight = new Vector3(backRight.transform.position.x, backRight.transform.position.y, backRight.transform.position.z),
            backLeft = new Vector3(backLeft.transform.position.x, backLeft.transform.position.y, backLeft.transform.position.z)
        };

        var inputData = new InputComponent
        {
        };

        dstManager.AddComponentData(entity, spawnerData); // SYNC POINT // Just for the Entity Manager
        dstManager.AddComponentData(entity, borderData); // SYNC POINT // Just for the Entity Manager
        dstManager.AddComponentData(entity, inputData); // SYNC POINT // Just for the Entity Manager
    }
}