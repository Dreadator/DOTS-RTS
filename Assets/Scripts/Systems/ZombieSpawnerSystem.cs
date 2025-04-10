using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct ZombieSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        EntityCommandBuffer entityCommandBuffer =
           SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var job = new ZombieSpawnerJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            RandomSeed = (uint)DateTime.Now.Ticks,
            PrefabToInstantiate = entitiesReferences.zombiePrefabEntity,
            ECB = ecb.AsParallelWriter(),
        };
        job.ScheduleParallel();
    }


    [BurstCompile]
    public partial struct ZombieSpawnerJob : IJobEntity
    {
        public float DeltaTime;
        public uint RandomSeed; 
        public Entity PrefabToInstantiate;
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute([EntityIndexInQuery] int entityIndexInQuery, in LocalTransform localTransform, ref ZombieSpawner zombieSpawner)
        {
            zombieSpawner.timer -= DeltaTime;

            if (zombieSpawner.timer > 0f)
                return;

            zombieSpawner.timer = zombieSpawner.timerMax;

            Entity zombieEntity = ECB.Instantiate(entityIndexInQuery, PrefabToInstantiate);

            ECB.SetComponent(entityIndexInQuery, zombieEntity, LocalTransform.FromPosition(localTransform.Position));

            var random = new Unity.Mathematics.Random(RandomSeed + (uint)entityIndexInQuery);

            ECB.AddComponent(entityIndexInQuery, zombieEntity, new RandomWalking
            {
                originPosition = localTransform.Position,
                targetPosition = localTransform.Position,
                distanceMin = zombieSpawner.randomWalkingDistanceMin,
                distanceMax = zombieSpawner.randomWalkingDistanceMax,
                random = random,
            });
        }
    }
}