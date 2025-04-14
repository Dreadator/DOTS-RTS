using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ShootLightSpawnerSytem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        ShootLightSpawnJob shootLightSpawnJob = new ShootLightSpawnJob
        {
            shootLightPrefab = entitiesReferences.shootLightPrefabEntity,
            ECB = ecb.AsParallelWriter(),
        };
        shootLightSpawnJob.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct ShootLightSpawnJob : IJobEntity
    {
        public Entity shootLightPrefab;
        public EntityCommandBuffer.ParallelWriter ECB;
        public void Execute([EntityIndexInQuery] int entityIndexInQuery,in ShootAttack shootAttack) 
        {
            if (shootAttack.OnShoot.isTriggered)
            {
                Entity shootLightEntity = ECB.Instantiate(entityIndexInQuery,shootLightPrefab);
                ECB.SetComponent(entityIndexInQuery ,shootLightEntity, LocalTransform.FromPosition(shootAttack.OnShoot.shootFromPosition));
            }
        }
    }
}
