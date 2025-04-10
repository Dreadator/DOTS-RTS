using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct FindTargetSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(isReadOnly: true);
        var unitLookup = SystemAPI.GetComponentLookup<Unit>(isReadOnly: true);
        var targetLocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(isReadOnly: true);

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        CollisionFilter collisionFilter = new CollisionFilter
        {
            BelongsTo = ~0u,
            CollidesWith = 1u << GameAssets.UNITS_LAYER,
            GroupIndex = 0
        };

        FindTargetJob findTargetJob = new FindTargetJob
        {
            PhysicsWorldSingleton = physicsWorldSingleton,
            CollisionFilter = collisionFilter,
            DeltaTime = SystemAPI.Time.DeltaTime,
            LocalToWorldLookup = localToWorldLookup,
            TargetLocalTransformLookup = targetLocalTransformLookup,
            UnitLookup = unitLookup,
        };
        findTargetJob.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct FindTargetJob : IJobEntity
    {
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorldSingleton;
        [ReadOnly] public CollisionFilter CollisionFilter;
        [ReadOnly] public float DeltaTime;

        [ReadOnly] public ComponentLookup<Unit> UnitLookup;
        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> TargetLocalTransformLookup;

        public void Execute(in LocalTransform localTransform, ref FindTarget findTarget, ref Target target)
        {
            CollisionWorld collisionWorld = PhysicsWorldSingleton.CollisionWorld;
            NativeList<DistanceHit> distanceHitList = new(Allocator.TempJob);

            findTarget.timer -= DeltaTime;
            if (findTarget.timer > 0f)
                return;

            findTarget.timer = findTarget.timerMax;

            if (collisionWorld.OverlapSphere(localTransform.Position,
                   findTarget.range, ref distanceHitList, CollisionFilter))
            {
                Entity closestEntity = Entity.Null;
                float closestDistance = float.MaxValue;

                for (int i = 0; i < distanceHitList.Length; i++)
                {
                    DistanceHit hit = distanceHitList[i];

                    if (!UnitLookup.HasComponent(hit.Entity)) continue;

                    Unit targetUnit = UnitLookup[hit.Entity];
                    if (targetUnit.Faction != findTarget.targetFaction) continue;

                    float3 targetPosition = LocalToWorldLookup[hit.Entity].Position;
                    float distance = math.distance(localTransform.Position, targetPosition);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEntity = hit.Entity;
                    }
                }

                if (closestEntity != Entity.Null)
                {
                    LocalTransform targetLocalTransform = TargetLocalTransformLookup[closestEntity];

                    target.targetEntity = closestEntity;
                    target.targetLocalTransform = targetLocalTransform;
                }
            }
            distanceHitList.Dispose();
        }
    }
}

