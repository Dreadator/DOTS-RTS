using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct SetupUnitMoverDefaultPositionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer =
           SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRO<SetupUnitMoverDefaultPosition> setupUnitMoverDefaultPosition, RefRO<LocalTransform> localTransform, RefRW<UnitMover> unitMover, Entity entity) 
            in SystemAPI.Query<RefRO<SetupUnitMoverDefaultPosition>, RefRO<LocalTransform>, RefRW<UnitMover>>().WithEntityAccess())
        {
            unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            entityCommandBuffer.RemoveComponent<SetupUnitMoverDefaultPosition>(entity);
        }
    }
}
