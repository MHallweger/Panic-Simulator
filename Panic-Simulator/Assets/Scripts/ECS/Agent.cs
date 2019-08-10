using Unity.Entities;
using Unity.Mathematics;


public enum AgentStatus
{
    Idle = 0,
    Moving = 1,
    running = 2,
}


public struct Agent : IComponentData
{
    public float3 destination;
    public AgentStatus agentStatus;
    // TODO: Color implementation (attribute) here
}