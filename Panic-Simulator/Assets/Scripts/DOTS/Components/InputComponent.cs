using Unity.Entities;

/// <summary>
/// Each agent get access on Input information.
/// </summary>
public struct InputComponent : IComponentData
{
    public bool leftClick; // Bool for clicking left mouse button down
    public bool rightClick; // Bool for clicking right mouse button down
    public bool keyOnePressed; // Bool for key "1" (while holding down)
}
