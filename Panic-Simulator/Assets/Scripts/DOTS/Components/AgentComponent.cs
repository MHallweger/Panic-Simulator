using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Describes the current AgentStatus.
/// </summary>
public enum AgentStatus
{
    Idle = 0, // standing still
    Moving = 1, // normal moving
    Dancing = 2, // jumping/dancing "animation"
    Running = 3 // fast moving if panic spot appears
}

/// <summary>
/// Each Agent gets a AgentComponent.
/// </summary>
public struct AgentComponent : IComponentData
{
    public bool hasTarget; // Agent has a target
    public float3 target; // The actual target
    public AgentStatus agentStatus; // The current status of the agent
    public bool exitPointReached; // Agent has reached the user generated exit spot
    public bool foundTemporaryNewRandomPosition; // Agent found a first random generated position
    public bool foundFinalExitPoint; // Agent found the final exit position
    public bool marked; // Agent was on a exit spot (or on the way to it) but turned around to find a new exit spot
    public float fleeProbability; // The Probability to turn around when running to an exit spot
}
