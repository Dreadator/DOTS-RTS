using Unity.Entities;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

class ZombieSpawnerAuthoring : MonoBehaviour
{
    public float timerMax;
    public float randomWalkingDistanceMin;
    public float randomWalkingDistanceMax;

    class ZombieAuthoringBaker : Baker<ZombieSpawnerAuthoring>
    {
        public override void Bake(ZombieSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ZombieSpawner 
            {
                timerMax = authoring.timerMax,
                randomWalkingDistanceMin = authoring.randomWalkingDistanceMin,
                randomWalkingDistanceMax = authoring.randomWalkingDistanceMax,
            });
        }
    }
}

public struct ZombieSpawner : IComponentData 
{
    public float timer;
    public float timerMax;
    public float randomWalkingDistanceMin;
    public float randomWalkingDistanceMax;
}


