using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    private EntityQuery _bulletMoverQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _bulletMoverQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<LocalTransform, Bullet, Target>()
            .Build(ref state);

        state.RequireForUpdate(_bulletMoverQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var shootVictimLookup = SystemAPI.GetComponentLookup<ShootVictim>(isReadOnly: true);
        var healthLookup = SystemAPI.GetComponentLookup<Health>(isReadOnly: true);
        var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(isReadOnly: true);

        var job = new BulletMoverJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            ECB = ecb.AsParallelWriter(),
            LocalToWorldLookup = localToWorldLookup,
            ShootVictimLookup = shootVictimLookup,
            HealthLookup = healthLookup
        };
        state.Dependency = job.ScheduleParallel(_bulletMoverQuery, state.Dependency);
    }

    [BurstCompile]
    public partial struct BulletMoverJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        [ReadOnly] public ComponentLookup<ShootVictim> ShootVictimLookup;
        [ReadOnly] public ComponentLookup<Health> HealthLookup;

        public void Execute([EntityIndexInQuery] int entityIndexInQuery, Entity entity, ref LocalTransform localTransform, in Bullet bullet, in Target target)
        {
            if (target.targetEntity == Entity.Null)
            {
                ECB.DestroyEntity(entityIndexInQuery, entity);
                return;
            }

            LocalToWorld targetLocalToWorld = LocalToWorldLookup[target.targetEntity];

            ShootVictim shootVictim = ShootVictimLookup[target.targetEntity];
            float3 hitLocalPosition = shootVictim.hitLocalPosition;

            float3 targetPosition = targetLocalToWorld.Position + shootVictim.hitLocalPosition;

            float distanceBeforesq = math.distancesq(localTransform.Position, targetPosition);

            float3 moveDirection = targetPosition - localTransform.Position;
            moveDirection = math.normalize(moveDirection);

            localTransform.Position += moveDirection * bullet.speed * DeltaTime;

            float distanceAftersq = math.distancesq(localTransform.Position, targetPosition);

            if (distanceAftersq > distanceBeforesq)
            {
                // Overshot the target
                localTransform.Position = targetPosition;
            }

            float destroyDistanceSq = 0.2f;
            if (math.distancesq(localTransform.Position, targetPosition) < destroyDistanceSq)
            {
                if (!HealthLookup.HasComponent(target.targetEntity))
                {
                    ECB.DestroyEntity(entityIndexInQuery, entity);
                    return;
                }

                Health health = HealthLookup[target.targetEntity];

                health.healthAmount -= bullet.damageAmount;
                health.OnHealthChanged = true;

                ECB.SetComponent(entityIndexInQuery, target.targetEntity, health);

                ECB.DestroyEntity(entityIndexInQuery, entity);
            }
        }
    }
}
