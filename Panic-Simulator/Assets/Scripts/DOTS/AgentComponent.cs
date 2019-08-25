using Unity.Entities;
using Unity.Mathematics;

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
    public bool hasTarget; // Which agent has a target?
    public float3 target; // The actual target
    public AgentStatus agentStatus; // The actual status of the agent
}
