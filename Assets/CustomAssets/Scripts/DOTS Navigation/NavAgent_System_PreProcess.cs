using Unity.Entities;

public partial class NavAgent_System_PreProcess : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem bs_ECB;

    protected override void OnCreate()
    {
        bs_ECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        var ecb = bs_ECB.CreateCommandBuffer();
        int maxEntitiesRoutedPerFrame = NavAgent_GlobalSettings.instance.maxEntitiesRoutedPerFrame;
        int entitiesRoutedThisFrame = 0;
        Entities
            .WithNone<NavAgent_ToBeRoutedTag>()
            .WithBurst()
            .ForEach((Entity e, ref NavAgent_Component nc) =>
            {
                if (entitiesRoutedThisFrame < maxEntitiesRoutedPerFrame && !nc.routed)
                {
                    ecb.AddComponent<NavAgent_ToBeRoutedTag>(e);
                    entitiesRoutedThisFrame++;
                }
            }).Run();
        bs_ECB.AddJobHandleForProducer(Dependency);
    }
}
