using Unity.Entities;

/// <summary>
/// This Component represents the Border of the actual festival area.
/// </summary>
public struct BorderComponent : IComponentData
{
    // Looking to stage:
    // Front-Right.x: 134.838f ; Front-Left.x: 215.446f ; Front-Left/Right.z: 367.907f ; Back-Left/Right.z: 506.695
    public float frontRight_x;
    public float frontLeft_x;
    public float frontLeftRight_z;
    public float backLeftRight_z;
}
