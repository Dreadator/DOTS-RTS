using Unity.Burst;
using Unity.Entities;

partial struct ShootLightDestroySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        ShootLightDestroyJob sldJob = new ShootLightDestroyJob 
        {
            ecbParallel = entityCommandBuffer.AsParallelWriter(),
            deltaTime = SystemAPI.Time.DeltaTime,
        };
        sldJob.ScheduleParallel();
    }


    [BurstCompile]
    public partial struct ShootLightDestroyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecbParallel;
        public float deltaTime;

        public void Execute([ChunkIndexInQuery] int chunkIndex, ref ShootLight shootLight, Entity entity)
        {
            shootLight.timer -= deltaTime;

            if (shootLight.timer <= 0f)
            {
                ecbParallel.DestroyEntity(chunkIndex, entity);
            }
        }
    }
}
