using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var moveOverrideLookup = SystemAPI.GetComponentLookup<MoveOverride>(isReadOnly: true);

        var job = new ShootAttackJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            BulletPrefab = entitiesReferences.bulletPrefabEntity,
            ECB = ecb.AsParallelWriter(),
            MoveOverrideLookup = moveOverrideLookup,
        };
        job.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct ShootAttackJob : IJobEntity
    {
        public float DeltaTime;
        public Entity BulletPrefab;
        public EntityCommandBuffer.ParallelWriter ECB;

        [ReadOnly] public ComponentLookup<MoveOverride> MoveOverrideLookup;
        public void Execute([EntityIndexInQuery] int entityIndexInQuery,
            ref LocalTransform localTransform,
            ref ShootAttack shootAttack,
            ref UnitMover unitMover,
            in Target target,
            Entity entity)
        {
            if (MoveOverrideLookup.IsComponentEnabled(entity)) return;

            if (target.targetEntity == Entity.Null)
                return;

            float3 targetPosition = target.targetLocalTransform.Position;

            if (math.distance(localTransform.Position, targetPosition) > shootAttack.attackDistance)
            {
                // Too far, move closer
                unitMover.targetPosition = targetPosition;
                return;
            }
            else
                unitMover.targetPosition = localTransform.Position;


            float3 aimDirection = targetPosition - localTransform.Position;
            aimDirection = math.normalize(aimDirection);

            localTransform.Rotation = math.slerp(localTransform.Rotation,
                                             quaternion.LookRotation(aimDirection, math.up()),
                                             DeltaTime * unitMover.rotationSpeed);

            shootAttack.timer -= DeltaTime;

            if (shootAttack.timer > 0f)
                return;

            shootAttack.timer = shootAttack.timerMax;

            Entity bulletEntity = ECB.Instantiate(entityIndexInQuery, BulletPrefab);
            float3 bulletSpawnWorldPosition = localTransform.TransformPoint(shootAttack.bulletSpawnLocalPosition);

            ECB.SetComponent(entityIndexInQuery, bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));
            ECB.SetComponent(entityIndexInQuery, bulletEntity, new Bullet { speed = 60f, damageAmount = shootAttack.damageAmount, });
            ECB.SetComponent(entityIndexInQuery, bulletEntity, new Target { targetEntity = target.targetEntity });

            shootAttack.OnShoot.isTriggered = true;
            shootAttack.OnShoot.shootFromPosition = bulletSpawnWorldPosition;
        }
    }
}


