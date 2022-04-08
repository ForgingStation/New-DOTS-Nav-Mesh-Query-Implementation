using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

public struct NavAgent_Component : IComponentData
{
    public float3 fromLocation;
    public float3 toLocation;
    public NavMeshLocation nml_FromLocation;
    public NavMeshLocation nml_ToLocation;
    public bool routed;
}
