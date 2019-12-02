using Unity.Entities;

/// <summary>
/// The converted Scene GameObject (ConvertToEntity attached) gets this component to spawn the entitys.
/// </summary>
public struct UnitSpawnerComponent : IComponentData
{
    public int AmountToSpawn; // Amount to spawn
    public Entity Prefab; // Gameobject/Prefab -> converted into Entity -> accessed and copyed
}