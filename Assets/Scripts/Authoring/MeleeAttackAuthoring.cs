using Unity.Entities;
using UnityEngine;

class MeleeAttackAuthoring : MonoBehaviour
{
    public float timerMax;
    public float attackRange;
    public int damageAmount;
    public float colliderSize;

    class MeleeAttackAuthoringBaker : Baker<MeleeAttackAuthoring>
    {
        public override void Bake(MeleeAttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeleeAttack 
            {
                timerMax = authoring.timerMax,
                attackRange = authoring.attackRange,
                damageAmount = authoring.damageAmount,
                colliderSize = authoring.colliderSize,
            });
        }
    }
}

public struct MeleeAttack : IComponentData 
{
    public float timer;
    public float timerMax;
    public float attackRange;
    public int damageAmount;
    public float colliderSize;
}


