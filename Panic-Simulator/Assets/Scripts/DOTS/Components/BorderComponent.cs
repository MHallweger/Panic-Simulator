using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// This Component represents the Border of the actual festival area.
/// </summary>
public struct BorderComponent : IComponentData
{
    // 4 spawn positions
    public float3 frontRight;
    public float3 frontLeft;
    public float3 backRight;
    public float3 backLeft;
}
