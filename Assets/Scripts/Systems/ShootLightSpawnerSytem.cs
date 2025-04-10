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

        foreach (RefRO<ShootAttack> shootAttack in SystemAPI.Query<RefRO<ShootAttack>>()) 
        {
            if (shootAttack.ValueRO.OnShoot.isTriggered)
            {
                Entity shootLightEntity = state.EntityManager.Instantiate(entitiesReferences.shootLightPrefabEntity);
                SystemAPI.SetComponent(shootLightEntity, LocalTransform.FromPosition(shootAttack.ValueRO.OnShoot.shootFromPosition));
            }
        }
    }
}
