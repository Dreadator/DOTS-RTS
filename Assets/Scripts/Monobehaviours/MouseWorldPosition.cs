using UnityEngine;

public class MouseWorldPosition : MonoBehaviour
{
    public static MouseWorldPosition Instance { get; private set; }

    private Camera mainCam;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        mainCam = Camera.main;
    }

    public Vector3 GetPosition()
    {
        Ray mouseCameraRay = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(mouseCameraRay, out float distance))
            return mouseCameraRay.GetPoint(distance);
        else
        { return Vector3.zero; }
    }
}
