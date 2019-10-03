using Unity.Entities;

/// <summary>
/// Each agent get their own moveSpeed Component to simulate the different speed attributes of humans.
/// </summary>
public struct MoveSpeedComponent : IComponentData
{
    public float moveSpeed; // Speed for normal moving state
    public float runningSpeed; // Speed for panic situation
    public float jumpSpeed; // Spped for jumping
    public float panicJumpSpeed; // Speed for panic Jumping
}
