using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct TestingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*foreach ((RefRW<LocalTransform> localTransform, RefRO<UnitMover> unitMover, RefRW<PhysicsVelocity> physicsVelocity, RefRO<Selected> selected) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<UnitMover>, RefRW<PhysicsVelocity>, RefRO<Selected>>()) 
        {      
        }*/
    }
}
