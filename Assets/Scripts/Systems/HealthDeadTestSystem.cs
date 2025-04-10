using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthDeadTestSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        
        EntityCommandBuffer entityCommandBuffer = 
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRO<Health> health, Entity entity)
          in SystemAPI.Query<RefRO<Health>>().WithEntityAccess())
        {
            if (state.EntityManager.Exists(entity))
            {
                if (health.ValueRO.healthAmount <= 0)
                {
                    entityCommandBuffer.DestroyEntity(entity);
                }
            }
        }
    }

}
