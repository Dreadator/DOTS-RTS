using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

partial struct ShootAttackSystem : ISystem
{
    //[BurstCompile]
    //public void OnUpdate(ref SystemState state)
    //{
    //    EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

    //    var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
    //    var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

    //    var job = new ShootAttackJob
    //    {
    //        DeltaTime = SystemAPI.Time.DeltaTime,
    //        PrefabToInstantiate = entitiesReferences.bulletPrefabEntity,
    //        ECB = ecb.AsParallelWriter(),
    //    };
    //    job.ScheduleParallel();
    //}


    //[BurstCompile]
    //public partial struct ShootAttackJob : IJobEntity
    //{
    //    public float DeltaTime;
    //    public Entity PrefabToInstantiate;
    //    public EntityCommandBuffer.ParallelWriter ECB;

    //    public void Execute([EntityIndexInQuery] int entityIndexInQuery, ref LocalTransform localTransform,
    //        ref ShootAttack shootAttack, ref UnitMover unitMover, in Target target)
    //    {
    //        if (target.targetEntity == Entity.Null)
    //            return;

    //        float3 targetPosition = target.targetLocalTransform.Position;

    //        if (math.distance(localTransform.Position, targetPosition) > shootAttack.attackDistance)
    //        {
    //            // Too far, move closer
    //            unitMover.targetPosition = targetPosition;
    //            return;
    //        }
    //        else
    //            unitMover.targetPosition = localTransform.Position;


    //        float3 aimDirection = targetPosition - localTransform.Position;
    //        aimDirection = math.normalize(aimDirection);

    //        localTransform.Rotation = math.slerp(localTransform.Rotation,
    //                                         quaternion.LookRotation(aimDirection, math.up()),
    //                                         DeltaTime * unitMover.rotationSpeed);

    //        shootAttack.timer -= DeltaTime;

    //        if (shootAttack.timer > 0f)
    //            return;

    //        shootAttack.timer = shootAttack.timerMax;

    //        Entity bulletEntity = ECB.Instantiate(entityIndexInQuery, PrefabToInstantiate);
    //        float3 bulletSpawnWorldPosition = localTransform.TransformPoint(shootAttack.bulletSpawnLocalPosition);

    //        ECB.SetComponent(entityIndexInQuery, bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));

    //        ECB.SetComponent(entityIndexInQuery, bulletEntity, new Bullet {
    //            speed = 60f,
    //            damageAmount = shootAttack.damageAmount,
    //        });

    //        ECB.SetComponent(entityIndexInQuery, bulletEntity, new Target {
    //            targetEntity = target.targetEntity,
    //        });

    //        shootAttack.OnShoot.isTriggered = true;
    //        shootAttack.OnShoot.shootFromPosition = bulletSpawnWorldPosition;
    //    }
    //}


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach ((RefRW<LocalTransform> localTransform, RefRW<ShootAttack> shootAttack, RefRO<Target> target, RefRW<UnitMover> unitMover)
            in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ShootAttack>, RefRO<Target>, RefRW<UnitMover>>().WithDisabled<MoveOverride>())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
                continue;

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) > shootAttack.ValueRO.attackDistance)
            {
                // Too far, move closer
                unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
                continue;
            }
            else
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            

            float3 aimDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
            aimDirection = math.normalize(aimDirection);

            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation,
                                             quaternion.LookRotation(aimDirection, math.up()),
                                             SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);


            shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (shootAttack.ValueRO.timer > 0f)
                continue;

            shootAttack.ValueRW.timer = shootAttack.ValueRO.timerMax;

            Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.bulletPrefabEntity);
            float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));

            RefRW<Bullet> bulletBullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bulletBullet.ValueRW.damageAmount = shootAttack.ValueRO.damageAmount;

            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.targetEntity = target.ValueRO.targetEntity;

            shootAttack.ValueRW.OnShoot.isTriggered = true;
            shootAttack.ValueRW.OnShoot.shootFromPosition = bulletSpawnWorldPosition;
        }
    }
}


