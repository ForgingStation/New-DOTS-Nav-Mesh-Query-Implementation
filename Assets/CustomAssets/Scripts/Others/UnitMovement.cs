using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public partial class UnitMovement : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        Entities
            .ForEach((ref Translation trans, 
            ref UnitComponentData uc,
            ref Boid_ComponentData bc,
            ref Rotation rot,
            in NavAgent_Component nc, 
            in DynamicBuffer<NavAgent_Buffer> nb) =>
            {
                if (nc.routed && nb.Length>0)
                {
                    uc.correctedWaypoint = nb[uc.currentBufferIndex].wayPoints + new float3(0, uc.waypoint_Z_Offset, 0);
                    //uc.waypointDirection = math.normalize(uc.correctedWaypoint - trans.Value);
                    bc.target = uc.correctedWaypoint;
                    //trans.Value += uc.waypointDirection * uc.speed * deltaTime;
                    //rot.Value = math.slerp(rot.Value, quaternion.LookRotation(math.normalize(bc.velocity), math.up()), deltaTime * 10);
                    trans.Value = math.lerp(trans.Value, (trans.Value + new float3(bc.velocity.x, 0, bc.velocity.z)), deltaTime);

                    if (!uc.reached && math.distance(trans.Value, uc.correctedWaypoint) <= uc.minDistance && uc.currentBufferIndex < nb.Length - 1)
                    {
                        uc.currentBufferIndex = uc.currentBufferIndex + 1;
                        if (uc.currentBufferIndex == nb.Length - 1)
                        {
                            uc.reached = true;
                        }
                    }
                    else if (uc.reached && math.distance(trans.Value, uc.correctedWaypoint) <= uc.minDistance && uc.currentBufferIndex > 0)
                    {
                        uc.currentBufferIndex = uc.currentBufferIndex - 1;
                        if (uc.currentBufferIndex == 0)
                        {
                            uc.reached = false;
                        }
                    }
                }
            }).ScheduleParallel();
    }
}
