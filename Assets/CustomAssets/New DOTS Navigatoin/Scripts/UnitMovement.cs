using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class UnitMovement : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        Entities
            .ForEach((ref Translation trans, ref UnitComponentData uc, in NavAgent_Component nc, in DynamicBuffer<NavAgent_Buffer> nb) =>
            {
                if (nc.routed && nb.Length>0)
                {
                    uc.waypointDirection = math.normalize((nb[uc.currentBufferIndex].wayPoints) - trans.Value);
                    trans.Value += (math.normalize(uc.waypointDirection + uc.offset)) * uc.speed * deltaTime;
                    if (!uc.reached && math.distance(trans.Value, nb[uc.currentBufferIndex].wayPoints) <= uc.minDistance && uc.currentBufferIndex < nb.Length - 1)
                    {
                        uc.currentBufferIndex = uc.currentBufferIndex + 1;
                        if (uc.currentBufferIndex == nb.Length - 1)
                        {
                            uc.reached = true;
                        }
                    }
                    else if (uc.reached && math.distance(trans.Value, nb[uc.currentBufferIndex].wayPoints) <= uc.minDistance && uc.currentBufferIndex > 0)
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
