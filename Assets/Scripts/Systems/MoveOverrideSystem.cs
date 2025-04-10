using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MoveOverrideSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        MoveOverrideJob moveOverrideJob = new MoveOverrideJob
        {
            ecbParallel = ecb.AsParallelWriter(),
        };
        state.Dependency = moveOverrideJob.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct MoveOverrideJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecbParallel;

        public void Execute(Entity entity, in LocalTransform localTransform, in MoveOverride moveOverride, ref UnitMover unitMover, [ChunkIndexInQuery] int chunkIndex)
        {
            if (math.distancesq(localTransform.Position, moveOverride.targetPosition) > UnitMoverSystem.REACHED_TARGET_POSITION_SQ)
            {
                // move closer
                unitMover.targetPosition = moveOverride.targetPosition;
            }
            else
            {
                // reached move override target position
                 ecbParallel.SetComponentEnabled<MoveOverride>(chunkIndex, entity, false);
            }
        }
    }
}
