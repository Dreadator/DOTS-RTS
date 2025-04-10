using Unity.Entities;
using UnityEngine;

class HealthAuthoring : MonoBehaviour
{
    public int healthAmount;
    public int healthAmountMax;

    class HealthAuthoringBaker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health 
            {
                healthAmount = authoring.healthAmount,
                healthAmountMax = authoring.healthAmountMax,
                OnHealthChanged = true,
            });
        }
    }
}

public struct Health : IComponentData 
{
    public int healthAmount;
    public int healthAmountMax;

    public bool OnHealthChanged;
}

