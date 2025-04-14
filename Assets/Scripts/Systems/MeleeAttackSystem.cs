using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct MeleeAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var healthLookup = SystemAPI.GetComponentLookup<Health>(isReadOnly: true);
        var moveOverrideLookup = SystemAPI.GetComponentLookup<MoveOverride>(isReadOnly: true);

        MeleeAttackJob meleeAttackJob = new MeleeAttackJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            PhysicsWorldSingleton = physicsWorldSingleton,
            HealthLookup = healthLookup,
            MoveOverrideLookup = moveOverrideLookup,
            ECB = ecb.AsParallelWriter(),
        };
        meleeAttackJob.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct MeleeAttackJob : IJobEntity
    {
        public float DeltaTime;
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorldSingleton;

        [ReadOnly] public ComponentLookup<Health> HealthLookup;
        [ReadOnly] public ComponentLookup<MoveOverride> MoveOverrideLookup;

        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute([EntityIndexInQuery] int entityIndexInQuery,
            ref LocalTransform localTransform,
            ref MeleeAttack meleeAttack,
            in Target target,
            ref UnitMover unitMover, Entity entity)
        {
            if (MoveOverrideLookup.IsComponentEnabled(entity)) return;
            
            if (target.targetEntity == Entity.Null) return;

            CollisionWorld collisionWorld = PhysicsWorldSingleton.CollisionWorld;
            NativeList<RaycastHit> raycastHitsList = new(Allocator.TempJob);

            LocalTransform targetLocalTransform = target.targetLocalTransform;
            float meleeAttackDistanceSq = 2f;

            bool isCloseEnoughToAttack = math.distancesq(localTransform.Position, targetLocalTransform.Position) > meleeAttackDistanceSq;

            bool isTouchingTarget = false;

            if (!isCloseEnoughToAttack)
            {
                float3 dirToTarget = targetLocalTransform.Position - localTransform.Position;
                dirToTarget = math.normalize(dirToTarget);

                float rayOffset = 0.4f;

                RaycastInput raycastInput = new RaycastInput
                {
                    Start = localTransform.Position,
                    End = localTransform.Position + dirToTarget * (meleeAttack.colliderSize + rayOffset),
                    Filter = CollisionFilter.Default,
                };
                raycastHitsList.Clear();

                if (collisionWorld.CastRay(raycastInput, ref raycastHitsList))
                {
                    foreach (RaycastHit raycastHit in raycastHitsList)
                    {
                        if (raycastHit.Entity == target.targetEntity)
                        {
                            // found target, and close enough to attack
                            isTouchingTarget = true;
                            break;
                        }
                    }
                }
            }

            if (!isCloseEnoughToAttack && !isTouchingTarget)
            {
                unitMover.targetPosition = targetLocalTransform.Position;
            }
            else
            {
                unitMover.targetPosition = localTransform.Position;

                float3 dirToTarget = targetLocalTransform.Position - localTransform.Position;
                dirToTarget = math.normalize(dirToTarget);

                localTransform.Rotation = math.slerp(localTransform.Rotation,
                                            quaternion.LookRotation(dirToTarget, math.up()),
                                            DeltaTime * unitMover.rotationSpeed);

                meleeAttack.timer -= DeltaTime;
                if (meleeAttack.timer > 0f)
                {
                    raycastHitsList.Dispose();
                    return;
                }
                meleeAttack.timer = meleeAttack.timerMax;

                Health targetHealth = HealthLookup[target.targetEntity];
                targetHealth.healthAmount -= meleeAttack.damageAmount;
                targetHealth.OnHealthChanged = true;
                ECB.SetComponent(entityIndexInQuery, target.targetEntity, targetHealth);
            }
            raycastHitsList.Dispose();
        }
    }
}
