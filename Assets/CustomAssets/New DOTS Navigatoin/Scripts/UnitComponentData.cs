using Unity.Entities;
using Unity.Mathematics;

public struct UnitComponentData : IComponentData
{
    public float speed;
    public int currentBufferIndex;
    public float3 waypointDirection;
    public float minDistance;
    public bool reached;
    public float3 offset;
}
