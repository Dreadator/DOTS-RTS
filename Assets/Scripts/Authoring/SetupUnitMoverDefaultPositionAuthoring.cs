using Unity.Entities;
using UnityEngine;

class SetupUnitMoverDefaultPositionAuthoring : MonoBehaviour
{
    class SetupUnitMoverDefaultPositionAuthoringBaker : Baker<SetupUnitMoverDefaultPositionAuthoring>
    {
        public override void Bake(SetupUnitMoverDefaultPositionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SetupUnitMoverDefaultPosition 
            {

            });
        }
    }
}

public struct SetupUnitMoverDefaultPosition : IComponentData {}