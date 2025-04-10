using Unity.Entities;
using UnityEngine;

class FriendlySpawnerAuthoring : MonoBehaviour
{
    public float timerMax;
    public float randomWalkingDistanceMin;
    public float randomWalkingDistanceMax;

    class FriendlySpawnerAuthoringBaker : Baker<FriendlySpawnerAuthoring>
    {
        public override void Bake(FriendlySpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FriendlySpawner
            {
                timerMax = authoring.timerMax,
                randomWalkingDistanceMin = authoring.randomWalkingDistanceMin,
                randomWalkingDistanceMax = authoring.randomWalkingDistanceMax,
            });
        }

    }
}

public struct FriendlySpawner : IComponentData 
{
    public float timer;
    public float timerMax;
    public float randomWalkingDistanceMin;
    public float randomWalkingDistanceMax;
}

