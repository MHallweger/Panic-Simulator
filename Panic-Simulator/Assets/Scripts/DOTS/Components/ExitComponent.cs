using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// </summary>
public struct ExitComponent : IComponentData
{
    public bool overloaded;
    public float amount;
}
