using Unity.Entities;
using Unity.Mathematics;

public enum AgentStatus
{
    Idle = 0, // standing still
    Moving = 1, // normal moving
    Dancing = 2, // jumping/dancing "animation"
    PrePanic = 3, // Orientation mode
    Running = 4 // fast moving if panic spot appears
}

/// <summary>
/// Each Agent gets a AgentComponent.
/// </summary>
public struct AgentComponent : IComponentData
{
    public bool hasTarget; // Which agent has a target?
    public bool jumped; // For checking if the agent jumped up from the ground
    public float3 target; // The actual target
    public AgentStatus agentStatus; // The actual status of the agent
    public bool exitPointReached; // For checking if the agent has reached the user generated exit spot
    //public float randomPositionsAfterActionPassed; // Int that describes the amount of random Positions that have been passed from an agent since the action appeared
    public bool foundTemporaryNewRandomPosition;
    public bool foundFinalExitPoint;
    public bool marked;
    public bool testing;
    public float fleeProbability;
    public float discoverProbability;
}
