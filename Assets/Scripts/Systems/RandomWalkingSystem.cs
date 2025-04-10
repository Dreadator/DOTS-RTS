using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct RandomWalkingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        RandomWalkingJob randomWalkingJob = new ();
        randomWalkingJob.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct RandomWalkingJob : IJobEntity 
    {
        public void Execute(ref RandomWalking randomWalking, ref UnitMover unitMover, in LocalTransform localTransform)
        {
            if (math.distancesq(localTransform.Position, randomWalking.targetPosition) < UnitMoverSystem.REACHED_TARGET_POSITION_SQ)
            {
                Random random = randomWalking.random;

                float3 randomDirection = new float3(random.NextFloat(-1f, 1f), 0, random.NextFloat(-1f, 1f));
                randomDirection = math.normalize(randomDirection);

                randomWalking.targetPosition = randomWalking.originPosition +
                    randomDirection * random.NextFloat(randomWalking.distanceMin, randomWalking.distanceMax);

                randomWalking.random = random;

            }
            else
                unitMover.targetPosition = randomWalking.targetPosition;         
        }
    }
}
