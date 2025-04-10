using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<RaycastHit> raycastHitsList = new NativeList<RaycastHit>(Allocator.Temp);

        foreach ((RefRW<LocalTransform> localTransform, RefRW<MeleeAttack> meleeAttack, RefRO<Target> target, RefRW<UnitMover> unitMover) 
            in SystemAPI.Query< RefRW<LocalTransform>, RefRW<MeleeAttack>, RefRO<Target>, RefRW<UnitMover>>().WithDisabled<MoveOverride>()) 
        {
            if (target.ValueRO.targetEntity == Entity.Null)
                continue;

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            float meleeAttackDistanceSq = 2f;

            bool isCloseEnoughToAttack = math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.Position) > meleeAttackDistanceSq;

            bool isTouchingTarget = false;

            if (!isCloseEnoughToAttack) 
            {
                float3 dirToTarget = targetLocalTransform.Position - localTransform.ValueRO.Position;
                dirToTarget = math.normalize(dirToTarget);

                float rayOffset = 0.4f;

                RaycastInput raycastInput = new RaycastInput 
                {
                    Start = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position + dirToTarget * (meleeAttack.ValueRO.colliderSize + rayOffset),
                    Filter = CollisionFilter.Default,
                };
                raycastHitsList.Clear();
                
                if(collisionWorld.CastRay(raycastInput, ref raycastHitsList)) 
                {
                    foreach(RaycastHit raycastHit in raycastHitsList) 
                    {
                        if(raycastHit.Entity == target.ValueRO.targetEntity) 
                        {
                            // found target, and close enough to attack
                            isTouchingTarget |= true;
                            break;
                        }
                    }
                }
            }

            if (!isCloseEnoughToAttack) 
            {
                // too far
                unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
            }
            else 
            {
                // close enough
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;

                float3 dirToTarget = targetLocalTransform.Position - localTransform.ValueRO.Position;
                dirToTarget = math.normalize(dirToTarget);

                localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation,
                                            quaternion.LookRotation(dirToTarget, math.up()),
                                            SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);

                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if(meleeAttack.ValueRO.timer > 0f) 
                {
                    continue;
                }
                meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;

                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                targetHealth.ValueRW.OnHealthChanged = true;
            }
        }
    }
}
