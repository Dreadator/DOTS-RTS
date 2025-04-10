using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

class TargetAuthoring : MonoBehaviour
{
    public GameObject targetGO;

    class TargetAuthotinBaker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Target
            {
                targetEntity = GetEntity(authoring.targetGO, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct Target : IComponentData 
{
    public Entity targetEntity;
    public LocalTransform targetLocalTransform;
} 


