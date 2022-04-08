using Unity.Entities;
using Unity.Mathematics;

public struct Boid_ComponentData : IComponentData
{
    public float cohesionBias;
    public float separationBias;
    public float alignmentBias;
    public float targetBias;
    public float perceptionRadius;
    public float speed;
    public float3 currentPosition;
    public float3 velocity;
    public float3 acceleration;
    public float3 target;
    public int cellSize;
}