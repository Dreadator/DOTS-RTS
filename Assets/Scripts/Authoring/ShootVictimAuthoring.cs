using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

class ShootVictimAuthoring : MonoBehaviour
{
    public Transform hitPositionTransform;

    class ShootVictimAuthoringBaker : Baker<ShootVictimAuthoring>
    {

        public override void Bake(ShootVictimAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootVictim 
            {
                hitLocalPosition = authoring.hitPositionTransform.localPosition,
            });
        }
    }
}

public struct ShootVictim : IComponentData 
{
    public float3 hitLocalPosition;
}


