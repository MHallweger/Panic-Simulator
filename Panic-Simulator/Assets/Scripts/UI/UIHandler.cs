using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using TMPro;

public class UIHandler : MonoBehaviour
{
    #region Radial Menu UI
    /// <summary>
    /// The Action class for every single Radial Menu Slot
    /// </summary>
    [System.Serializable]
    public class Action
    {
        public Color color;
        public Sprite sprite;
        public string title;
    }
    #endregion // Radial Menu UI
    #region Variables
    // Radial Menu UI
    public string title; // Label text
    public Action[] options;
    [SerializeField] private TextMeshProUGUI fpsText;
    private float deltaTime = 0.0f;

    // Camera
    [SerializeField] private float zoomSpeed = 13.0f; // Speed Variable for zoom up/down feature
    [SerializeField] private float cameraSpeed = 5.0f; // Speed Variable for camera movement
    private float lookSpeedH = 2f; // Variable for Camera rotation feature
    private float lookSpeedV = 2f; // Variable for Camera rotation feature
    private float yaw = 0f; // Variable for Camera rotation feature
    private float pitch = 0f; // Variable for Camera rotation feature

    // FPS
    private float ms = 0.0f;
    private float fps = 0.0f;
    #endregion // Variables

    private void Start()
    {
        if (title == "" || title == null)
        {
            title = gameObject.name;
        }
    }

    void Update()
    {
        HandleRadialMenuUI();
        HandleCamera();

        ShowFPS();
    }

    private void HandleRadialMenuUI()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Tell the canvas to spawn a menu
            RadialMenuSpawner.instance.SpawnMenu(this); // The buttons (slots) needs .this
        }
    }

    private void ShowFPS()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        ms = deltaTime * 1000.0f;
        fps = 1.0f / deltaTime;
        fpsText.text = "ms: " + ms + "    FPS: " + Mathf.Ceil(fps).ToString();
    }

    /// <summary>
    /// Controls Camera with WASD
    /// Look around with right mouse click (holded)
    /// Zoom up and down with mouse wheel
    /// TODO: SMOOTH FEATURE, CHECKBOX FOR GAME VIEW BORDER MOUSE CONTROL
    /// </summary>
    private void HandleCamera()
    {
        // Control Camera with WASD
        if (Input.GetKey(KeyCode.W) /*|| Input.mousePosition.y >= Screen.height - panBorderThickness*/)
        {
            transform.Translate(Vector3.forward * cameraSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S) /*|| Input.mousePosition.y <= panBorderThickness*/)
        {
            transform.Translate(Vector3.back * cameraSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D) /*|| Input.mousePosition.x >= Screen.width - panBorderThickness*/)
        {
            transform.Translate(Vector3.right * cameraSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A) /*|| Input.mousePosition.x <= panBorderThickness*/)
        {
            transform.Translate(Vector3.left * cameraSpeed * Time.deltaTime);
        }

        //Look around with Right Mouse [DONE][BUG: FLIP AT START]
        if (Input.GetMouseButton(1))
        {
            yaw += lookSpeedH * Input.GetAxis("Mouse X");
            pitch += lookSpeedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        //Zoom up and down with mouse wheel [DONE]
        transform.Translate(0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, 0, Space.Self);
    }
}
