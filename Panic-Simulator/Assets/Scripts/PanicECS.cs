using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanicECS : MonoBehaviour
{
    #region Variables
    public static PanicECS instance;


    // Camera
    private Vector3 actualPositon;
    [SerializeField] private float panBorderThickness = 10f; // Top Screen Border limit
    [SerializeField] private float panSpeed = 20f; // Speed for moving Camera across the world
    [SerializeField] private Vector2 panLimit; // For Clamp Purpose (Camera x,z)
    [SerializeField] private float scrollSpeed = 20f; // Speed for scroll wheel
    [SerializeField] private float minY = 20f; // Min Y height for the scroll wheel
    [SerializeField] private float maxY = 120f; // Max Y height for the scroll wheel
    // Camera Smoothness
    private Vector3 desiredPosition;
    [SerializeField] private float smoothSpeed = 10f;
    #endregion // Variables

    private void Start()
    {

    }

    private void Update()
    {
        //HandleCamera();
    }

    private void LateUpdate()
    {
        HandleCamera();
        SmoothCamera(); // No conflicts with the camera movement
    }

    private void HandleCamera()
    {
        actualPositon = Camera.main.transform.position;

        if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            actualPositon.z += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= panBorderThickness)
        {
            actualPositon.z -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            actualPositon.x += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= panBorderThickness)
        {
            actualPositon.x -= panSpeed * Time.deltaTime;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        actualPositon.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        actualPositon.x = Mathf.Clamp(actualPositon.x, -panLimit.x, panLimit.x);
        actualPositon.z = Mathf.Clamp(actualPositon.z, -panLimit.y, panLimit.y);
        actualPositon.y = Mathf.Clamp(actualPositon.y, minY, maxY);

        //Camera.main.transform.position = actualPositon;

    }

    private void SmoothCamera()
    {
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, actualPositon, smoothSpeed * Time.deltaTime); ;
    }
}
