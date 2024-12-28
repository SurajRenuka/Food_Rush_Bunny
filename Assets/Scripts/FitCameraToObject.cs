using UnityEngine;

public class FitCameraToObject : MonoBehaviour
{
    public Camera mainCamera; // The Camera to adjust
    public GameObject targetObject; // The target object to fit within the camera's view

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main; // Use the main camera if none is assigned

        if (targetObject != null)
        {
            FitToTarget();
        }
        else
        {
            Debug.LogError("No target object assigned!");
        }
    }

    void FitToTarget()
    {
        // Get the renderer of the target object
        Renderer targetRenderer = targetObject.GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogError("The target object does not have a Renderer component!");
            return;
        }

        // Calculate the width of the target object in world units
        float targetWidth = targetRenderer.bounds.size.x;

        // Set the orthographic size to match the target's width
        float screenAspect = mainCamera.aspect; // Aspect ratio (width/height)
        float cameraSize = targetWidth / (2f * screenAspect);
        mainCamera.orthographicSize = cameraSize;

        // Adjust the camera's position to center on the target object
        Vector3 targetPosition = targetRenderer.bounds.center;
        mainCamera.transform.position = new Vector3(targetPosition.x, transform.position.y, mainCamera.transform.position.z);
    }
}