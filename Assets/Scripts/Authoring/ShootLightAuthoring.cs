using Unity.Entities;
using UnityEngine;

class ShootLightAuthoring : MonoBehaviour
{
    public float timer;
}

class ShootLightAuthoringBaker : Baker<ShootLightAuthoring>
{
    public override void Bake(ShootLightAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Renderable);
        AddComponent(entity, new ShootLight
        {
            timer = authoring.timer,
        });
    }
}

public struct ShootLight : IComponentData 
{
    public float timer;
}
