using Unity.Entities;

/// <summary>
/// Component for each exit entity.
/// </summary>
public struct ExitComponent : IComponentData
{
    public bool overloaded; // Bool for checking if an exit is overloaded
    public float amount; // How many entitys are in the exit quadrant
}
