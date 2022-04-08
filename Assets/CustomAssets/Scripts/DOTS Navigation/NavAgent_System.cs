using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Experimental.AI;

public partial class NavAgent_System : SystemBase
{
    private NavMeshWorld navMeshWorld;
    private bool navMeshQueryAssigned;
    private NavMeshQuery query;
    private BeginSimulationEntityCommandBufferSystem es_ECB_Parallel;

    protected override void OnCreate()
    {
        es_ECB_Parallel = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        navMeshQueryAssigned = false;
        navMeshWorld = NavMeshWorld.GetDefaultWorld();
    }

    protected override void OnUpdate()
    {

        if (!navMeshQueryAssigned)
        {
            query = new NavMeshQuery(navMeshWorld, Allocator.Persistent, NavAgent_GlobalSettings.instance.maxPathNodePoolSize);
            navMeshQueryAssigned = true;
        }

        float3 extents = NavAgent_GlobalSettings.instance.extents;
        int maxIterations = NavAgent_GlobalSettings.instance.maxIterations;
        int maxPathSize = NavAgent_GlobalSettings.instance.maxPathSize;
        NavMeshQuery currentQuery = query;
        var ecbParallel = es_ECB_Parallel.CreateCommandBuffer().AsParallelWriter();
        Entities
            .WithNativeDisableParallelForRestriction(currentQuery)
            .WithAll<NavAgent_ToBeRoutedTag>()
            .WithBurst()
            .ForEach((Entity e, int entityInQueryIndex, ref NavAgent_Component nc, ref DynamicBuffer<NavAgent_Buffer> nb) =>
            {
                PathQueryStatus status = PathQueryStatus.Failure;
                nc.nml_FromLocation = currentQuery.MapLocation(nc.fromLocation, extents, 0);
                nc.nml_ToLocation = currentQuery.MapLocation(nc.toLocation, extents, 0);
                if (currentQuery.IsValid(nc.nml_FromLocation) && currentQuery.IsValid(nc.nml_ToLocation))
                {
                    status = currentQuery.BeginFindPath(nc.nml_FromLocation, nc.nml_ToLocation, -1);
                }
                if (status == PathQueryStatus.InProgress)
                {
                    status = currentQuery.UpdateFindPath(maxIterations, out int iterationPerformed);
                }
                if (status == PathQueryStatus.Success)
                {
                    status = currentQuery.EndFindPath(out int polygonSize);
                    NativeArray<NavMeshLocation> res = new NativeArray<NavMeshLocation>(polygonSize, Allocator.Temp);
                    NativeArray<StraightPathFlags> straightPathFlag = new NativeArray<StraightPathFlags>(maxPathSize, Allocator.Temp);
                    NativeArray<float> vertexSide = new NativeArray<float>(maxPathSize, Allocator.Temp);
                    NativeArray<PolygonId> polys = new NativeArray<PolygonId>(polygonSize, Allocator.Temp);
                    int straightPathCount = 0;
                    currentQuery.GetPathResult(polys);
                    status = PathUtils.FindStraightPath(
                        currentQuery,
                        nc.fromLocation,
                        nc.toLocation,
                        polys,
                        polygonSize,
                        ref res,
                        ref straightPathFlag,
                        ref vertexSide,
                        ref straightPathCount,
                        maxPathSize
                        );
                    if (status == PathQueryStatus.Success)
                    {
                        for (int i = 0; i < straightPathCount; i++)
                        {
                            nb.Add(new NavAgent_Buffer { wayPoints = res[i].position });
                        }
                        nc.routed = true;
                        ecbParallel.RemoveComponent<NavAgent_ToBeRoutedTag>(entityInQueryIndex, e);
                    }
                    res.Dispose();
                    straightPathFlag.Dispose();
                    polys.Dispose();
                    vertexSide.Dispose();
                }
            }).ScheduleParallel();

        navMeshWorld.AddDependency(Dependency);
        es_ECB_Parallel.AddJobHandleForProducer(Dependency);
    }
}