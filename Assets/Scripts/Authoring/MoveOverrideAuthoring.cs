using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class MoveOverrideAuthoring : MonoBehaviour
{
    class MoveOverrideAuthoringBaker : Baker<MoveOverrideAuthoring>
    {
        public override void Bake(MoveOverrideAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveOverride {});
            SetComponentEnabled<MoveOverride>(entity, false);
        }
    }
}

public struct MoveOverride : IComponentData, IEnableableComponent
{
    public float3 targetPosition;
}


