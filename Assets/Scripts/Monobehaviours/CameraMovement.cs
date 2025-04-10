using Unity.Cinemachine;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement Instance { get; private set; }

    [Header("Camera Settings")]
    [SerializeField] float moveSpeed = 10f;
    [Space]
    [SerializeField] float zoomSpeed = 5f;
    [SerializeField] float zoomDampening = 5f; // Adjust for smoother zooming
    [SerializeField] float minZoomOffset = 20f;
    [SerializeField] float maxZoomOffset = 60f;
    [Space]
    [SerializeField] CinemachineFollow cmFollow;

    private Transform mainCameraTransform;
    private Transform targetTransform;
    
    private float targetZoomOffset;
    private float currentZoomVelocity;

    float moveX;
    float moveZ;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        targetTransform = transform;
        mainCameraTransform = Camera.main.transform;
        if (mainCameraTransform == null)
        {
            enabled = false;
            return;
        }
        // Initialize the target zoom distance to the current distance
        targetZoomOffset = cmFollow.FollowOffset.y;
    }

    private void Update()
    {
         moveX = Input.GetAxisRaw("Horizontal");
         moveZ = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed * Time.deltaTime;
        targetTransform.Translate(movement, Space.World);

        // Camera Zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            targetZoomOffset -= scrollInput * zoomSpeed;
            targetZoomOffset = Mathf.Clamp(targetZoomOffset, minZoomOffset, maxZoomOffset);
        }

        // Apply Zoom with Damping to CinemachineFollow's Follow Offset Y
        Vector3 currentOffset = cmFollow.FollowOffset;
        float newOffsetY = Mathf.SmoothDamp(currentOffset.y, targetZoomOffset, ref currentZoomVelocity, zoomDampening * Time.deltaTime);
        cmFollow.FollowOffset = new Vector3(currentOffset.x, newOffsetY, currentOffset.z);
    }
}