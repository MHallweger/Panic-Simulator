using Unity.Entities;
/// <summary>
/// The converted Scene GameObject (ConvertToEntity attached) gets this component to spawn the entitys.
/// </summary>
public struct UnitSpawnerComponent : IComponentData
{
    // Two variables for instantiating copys of the prefab entity 
    public int CountX;
    public int CountY;

    public Entity Prefab; // Gameobject/Prefab -> converted into Entity -> accessed and copyed
}