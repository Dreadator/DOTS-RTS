using Unity.Entities;
using UnityEngine;

class UnitAuthoring : MonoBehaviour
{
    public Faction Faction;

    class UnitAuthoringBaker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Unit 
            {
                Faction = authoring.Faction 
            });
        }
    }
}

public struct Unit : IComponentData 
{
    public Faction Faction;
}


