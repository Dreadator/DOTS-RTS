using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

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

        //foreach ((RefRO<LocalTransform> localTransform, RefRW<ZombieSpawner> zombieSpawner) 
        //    in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>()) 
        //{
        //    zombieSpawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;

        //    if (zombieSpawner.ValueRO.timer > 0f)
        //        continue;

        //    zombieSpawner.ValueRW.timer = zombieSpawner.ValueRO.timerMax;

        //    Entity zombieEntity = state.EntityManager.Instantiate(entitiesReferences.zombiePrefabEntity);
        //    SystemAPI.SetComponent(zombieEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

        //    entityCommandBuffer.AddComponent(zombieEntity, new RandomWalking
        //    {
        //        originPosition = localTransform.ValueRO.Position,
        //        targetPosition = localTransform.ValueRO.Position,
        //        distanceMin = zombieSpawner.ValueRO.randomWalkingDistanceMin,
        //        distanceMax = zombieSpawner.ValueRO.randomWalkingDistanceMax,
        //        random = new Unity.Mathematics.Random((uint)zombieEntity.Index)
        //    });
        //}
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