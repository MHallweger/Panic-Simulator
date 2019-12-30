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
    #region Variables
    // Singleton instance variable
    public static UnitSpawnerProxy instance;

    // MonoBehaviour Variables
    [SerializeField] private GameObject Prefab; // Capsule human Prefab
    [SerializeField] private GameObject frontRight; // Border orientation GameObject
    [SerializeField] private GameObject frontLeft; // Border orientation GameObject
    [SerializeField] private GameObject backRight; // Border orientation GameObject
    [SerializeField] private GameObject backLeft; // Border orientation GameObject
    public int AmountToSpawn; // Amount can be set in the inspector, in later version the user can set the value for himself
    #endregion // Variables

    /// <summary>
    /// Assign instance variable.
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Referenced prefabs have to be declared so that the conversion system knows about them ahead of time.
    /// </summary>
    /// <param name="gameObjects">All current assigned GameObjects, there aren't any at the moment</param>
    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(Prefab);
    }

    /// <summary>
    /// Convert the editor data representation to the entity optimal runtime representation.
    /// </summary>
    /// <param name="entity">The first entity that handles the first steps of this process. -> Crowd GameObject from hierarchy</param>
    /// <param name="dstManager">The current actual World Entity Manager</param>
    /// <param name="conversionSystem">The Unity conversionSystem that handles everything for us</param>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Create Components
        var spawnerData = new UnitSpawnerComponent
        {
            // The referenced prefab will be converted due to DeclareReferencedPrefabs
            // Mapping the game object to an entity reference
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            AmountToSpawn = AmountToSpawn
        };

        var borderData = new BorderComponent
        {
            // Get current Border GameObject positions to save frontRight, frontLeft, backRight, backLeft position
            frontRight = new Vector3(frontRight.transform.position.x, frontRight.transform.position.y, frontRight.transform.position.z),
            frontLeft = new Vector3(frontLeft.transform.position.x, frontLeft.transform.position.y, frontLeft.transform.position.z),
            backRight = new Vector3(backRight.transform.position.x, backRight.transform.position.y, backRight.transform.position.z),
            backLeft = new Vector3(backLeft.transform.position.x, backLeft.transform.position.y, backLeft.transform.position.z)
        };

        var inputData = new InputComponent
        {
        };

        // Add created Components via EntityManager to entity (crowd GameObject)
        dstManager.AddComponentData(entity, spawnerData); // SYNC POINT // Just for the Entity Manager
        dstManager.AddComponentData(entity, borderData); // SYNC POINT // Just for the Entity Manager
        dstManager.AddComponentData(entity, inputData); // SYNC POINT // Just for the Entity Manager
    }
}