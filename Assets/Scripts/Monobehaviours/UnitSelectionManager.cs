using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using System;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    
    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;

    [SerializeField] float multipleSelectionSizeMin = 40f;

    private Vector2 selectionMouseStartPosition;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0)) 
        {
            selectionMouseStartPosition = Input.mousePosition;
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 selectionEndMousePosition = Input.mousePosition;

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            // Deselect Units 
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            NativeArray<Entity> entitiesArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);

            for (int i = 0; i < entitiesArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entitiesArray[i], false);
                Selected selected = selectedArray[i];
                selected.OnDeselected = true;
                entityManager.SetComponentData(entitiesArray[i], selected);
            }

            Rect selectionAreaRect = GetSelctionAreaRect();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;

            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            // Select Units 
            if (isMultipleSelection)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

                entitiesArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

                for (int i = 0; i < localTransformArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = localTransformArray[i];
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);

                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        entityManager.SetComponentEnabled<Selected>(entitiesArray[i], true);
                        Selected selected = entityManager.GetComponentData<Selected>(entitiesArray[i]);
                        selected.OnSelected = true;
                        entityManager.SetComponentData(entitiesArray[i], selected);
                    }
                }
            }
            else 
            {
                
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                
                PhysicsWorldSingleton physicsWorldSingleton =  entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                
                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER,
                        GroupIndex = 0
                    }
                };
                
                if(collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit)) 
                {
                    if (entityManager.HasComponent<Unit>(raycastHit.Entity) 
                        && entityManager.HasComponent<Selected>(raycastHit.Entity)) 
                    {
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.OnSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity, selected);
                    }
                }
            }     
            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(1)) 
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().WithPresent<MoveOverride>().Build(entityManager);

            NativeArray<Entity> entitiesArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<MoveOverride> moveOverrideArray = entityQuery.ToComponentDataArray<MoveOverride>(Allocator.Temp);
            NativeArray<float3> movePositionArray = GenerateMovePositionArray(mouseWorldPosition, entitiesArray.Length);

            for (int i = 0; i < moveOverrideArray.Length; i++) 
            {
                MoveOverride moveOverride = moveOverrideArray[i];
                moveOverride.targetPosition = movePositionArray[i];
                moveOverrideArray[i] = moveOverride;
                entityManager.SetComponentEnabled<MoveOverride>(entitiesArray[i], true);
            }
            entityQuery.CopyFromComponentDataArray(moveOverrideArray);
        }    
    }

    public Rect GetSelctionAreaRect() 
    {
        Vector2 selectionMouseEndPosition = Input.mousePosition;
        
        Vector2 lowerLeftCorner = new Vector2(
            Mathf.Min(selectionMouseStartPosition.x, selectionMouseEndPosition.x),
            Mathf.Min(selectionMouseStartPosition.y, selectionMouseEndPosition.y)
            );

        Vector2 upperRightCorner = new Vector2(
            Mathf.Max(selectionMouseStartPosition.x, selectionMouseEndPosition.x),
            Mathf.Max(selectionMouseStartPosition.y, selectionMouseEndPosition.y)
            );
        
        return new Rect(
            lowerLeftCorner.x,
            lowerLeftCorner.y,
            upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y
            );
    }

    private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount) 
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);

        if(positionCount == 0)
            return positionArray;
        
        positionArray[0] = targetPosition;
        if (positionCount == 1) 
            return positionArray;

        float ringSize = 1.2f;
        int ring = 0;
        int positionIndex = 1;

        while (positionIndex < positionCount) 
        {
            int ringPositionCount = 3 + ring * 2;

            for (int i = 0; i < ringPositionCount; i++) 
            {
                float angle = i * (math.PI2 / ringPositionCount);
                float3 ringVector = math.rotate(quaternion.RotateY(angle) ,new float3(ringSize * (ring + 1), 0, 0));
                float3 ringPosition = targetPosition + ringVector;

                positionArray[positionIndex] = ringPosition;
                positionIndex++;

                if (positionIndex >= positionCount)
                {
                    break;
                }
                ring++;
            }
        }
        return positionArray;
    }
}
