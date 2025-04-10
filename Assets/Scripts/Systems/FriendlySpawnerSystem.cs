using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

partial struct FriendlySpawnerSystem : ISystem
{
    private EntityQuery _friendlySpawnerQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _friendlySpawnerQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<LocalTransform, FriendlySpawner>()
            .Build(ref state);
        state.RequireForUpdate(_friendlySpawnerQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var job = new FriendlySpawnerJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            PrefabToInstantiate = entitiesReferences.soldierPrefabEntity,
            ECB = ecb.AsParallelWriter()
        };
        state.Dependency = job.ScheduleParallel(_friendlySpawnerQuery, state.Dependency);
    }

    [BurstCompile]
    public partial struct FriendlySpawnerJob : IJobEntity
    {
        public float DeltaTime;
        public Entity PrefabToInstantiate;
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute([EntityIndexInQuery] int entityIndexInQuery, in LocalTransform localTransform, ref FriendlySpawner friendlySpawner)
        {
            friendlySpawner.timer -= DeltaTime;

            if (friendlySpawner.timer > 0f)
                return;

            friendlySpawner.timer = friendlySpawner.timerMax;

            Entity friendlyEntity = ECB.Instantiate(entityIndexInQuery, PrefabToInstantiate);

            ECB.SetComponent(entityIndexInQuery, friendlyEntity, LocalTransform.FromPosition(localTransform.Position));
        }
    }
}
