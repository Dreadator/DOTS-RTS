using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct FriendlySpawnerSystem : ISystem
{
    private EntityQuery _friendlySpawnerQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Define the query for entities with LocalTransform and FriendlySpawner components
        _friendlySpawnerQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<LocalTransform, FriendlySpawner>()
            .Build(ref state);
        state.RequireForUpdate(_friendlySpawnerQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        // Get the ECB system and create a command buffer
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Schedule the job
        var job = new FriendlySpawnerJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            PrefabToInstantiate = entitiesReferences.soldierPrefabEntity,
            ECB = ecb.AsParallelWriter()
        };

        // Schedule the job and pass the dependency
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

    /*[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        EntityCommandBuffer entityCommandBuffer =
           SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRO<LocalTransform> localTransform, RefRW<FriendlySpawner> friendlySpawner)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRW<FriendlySpawner>>())
        {
            friendlySpawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (friendlySpawner.ValueRO.timer > 0f)
                continue;

            friendlySpawner.ValueRW.timer = friendlySpawner.ValueRO.timerMax;

            Entity friendlyEntity = state.EntityManager.Instantiate(entitiesReferences.soldierPrefabEntity);
            SystemAPI.SetComponent(friendlyEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

            //entityCommandBuffer.AddComponent(friendlyEntity, new RandomWalking
            //{
            //    originPosition = localTransform.ValueRO.Position,
            //    targetPosition = localTransform.ValueRO.Position,
            //    distanceMin = friendlySpawner.ValueRO.randomWalkingDistanceMin,
            //    distanceMax = friendlySpawner.ValueRO.randomWalkingDistanceMax,
            //    random = new Unity.Mathematics.Random((uint)friendlyEntity.Index)
            //});
        }
    }*/
}
