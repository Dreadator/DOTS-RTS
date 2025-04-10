using Unity.Entities;
using UnityEngine;

class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject bulletPrefabGO;
    public GameObject soldierPrefabGO;
    public GameObject zombiePrefabGO;
    public GameObject shootLightPrefabGO;


    class EntitiesReferencesAuthoringBaker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                bulletPrefabEntity = GetEntity(authoring.bulletPrefabGO, TransformUsageFlags.Dynamic),
                soldierPrefabEntity = GetEntity(authoring.soldierPrefabGO, TransformUsageFlags.Dynamic),
                zombiePrefabEntity = GetEntity(authoring.zombiePrefabGO, TransformUsageFlags.Dynamic),
                shootLightPrefabEntity = GetEntity(authoring.shootLightPrefabGO, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct EntitiesReferences : IComponentData 
{
    public Entity bulletPrefabEntity;
    public Entity soldierPrefabEntity;
    public Entity zombiePrefabEntity;
    public Entity shootLightPrefabEntity;
}