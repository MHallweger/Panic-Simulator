using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Script that handles different UI windows and user Inputs.
/// </summary>
public class UIHandler : MonoBehaviour
{
    #region Radial Menu UI
    /// <summary>
    /// The Action class for every single Radial Menu Slot
    /// </summary>
    [System.Serializable]
    public class Action
    {
        public Color color; // Background color of a single Radial Menu item
        public Sprite sprite; // Background sprite of a single Radial Menu item
        public string title; // Title of a single Radial Menu item
    }
    #endregion // Radial Menu UI

    #region Variables
    // Singleton Instance Variable
    public static UIHandler instance; // For accessing this instance

    // Radial Menu UI
    [SerializeField] private TextMeshProUGUI infoText; // The displayed Info text in the statistic window
    public string title; // Label text
    public Action[] options; // All actions/options which can be selected in the Radial Menu. Needed for the Radial Menu
    private float deltaTime = 0.0f;

    // Camera
    [SerializeField] private float zoomSpeed = 13.0f; // Speed Variable for zoom up/down feature
    [SerializeField] private float cameraSpeed = 5.0f; // Speed Variable for camera movement
    private float lookSpeedH = 2f; // Variable for Camera rotation feature
    private float lookSpeedV = 2f; // Variable for Camera rotation feature
    private float yaw = 0f; // Variable for Camera rotation feature
    private float pitch = 0f; // Variable for Camera rotation feature

    // FPS/ms/Entity/exits amount, UI stuff
    [HideInInspector] public int entityAmount; // UI entity Amount
    [HideInInspector] public int exitsAmount; // UI exits Amount
    private float ms = 0.0f; // UI ms amount
    private float fps = 0.0f; // UI fps amount
    public string mode = "-"; // Selected mode for the statistic window

    // Information/Statistic Windows
    [SerializeField] private GameObject informationWindowPanel; // Helper panel which includes explanations to different keys
    [SerializeField] private GameObject statisticWindowPanel; // Statistic Panel which includes some system informations

    // Effects
    [HideInInspector] public bool effectsEnabled = false; // Bool to identify if effects are enabled
    [SerializeField] private GameObject smoke; // The smoke GameObject
    [SerializeField] private GameObject displays; // The GameObject that holds all displays
    [SerializeField] private GameObject trussLights; // The GameObject that holds all lights on the stage

    // Barriers/Sound Systems
    public List<GameObject> userCreatedSoundSystems = new List<GameObject>(); // An Array, containing all user created Sound Systems
    [SerializeField] private GameObject frontSoundSystemLight; // The first Sound System
    [SerializeField] private GameObject barrierLeftWithPivot; // First barrier left
    [SerializeField] private GameObject barrierRightWithPivot; // First barrier right
    [SerializeField] private GameObject[] additionalSoundSystems; // Contains 4 additional Sound Systems

    // Animations
    [HideInInspector] public bool enableArrows = false; // Bool that allows the animation script on the Information Arrow GameObhects to animate
    #endregion // Variables

    /// <summary>
    /// Assign instance variable.
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Assign default title if null. Declare List for user created Sound Systems.
    /// </summary>
    private void Start()
    {
        if (title == "" || title == null)
        {
            title = gameObject.name;
        }

        userCreatedSoundSystems = new List<GameObject>();
    }

    /// <summary>
    /// Handle UI and User Input.
    /// </summary>
    void Update()
    {
        HandleRadialMenuUI();
        HandleCamera();
        ShowFPS();
        WindowCheck();
        CheckKeys();
    }

    /// <summary>
    /// A method that takes care of different Keycodes that can be pressed from the user.
    /// </summary>
    private void CheckKeys()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Disable active mode and update the UI
        {
            UIHandler.instance.mode = "-";
            Actions.instance.createExits = false;
            Actions.instance.fallingTruss = false;
            Actions.instance.smallGroundExplosion = false;
            Actions.instance.mediumGroundExplosion = false;
            Actions.instance.bigGroundExplosion = false;
            Actions.instance.dropSoundSystem = false;
        }
        else if (Input.GetKeyDown(KeyCode.O)) // Enable/Disable LOD Function
        {
            EnableOrDisableLODFunction();
        }
        else if (Input.GetKeyDown(KeyCode.N)) // Enable/Disable NightMode
        {
            EnableOrDisableNightMode();
        }
        else if (Input.GetKeyDown(KeyCode.C)) // Enable/Disable Orbit Camera
        {
            EnableOrDisableOrbitCamera();
        }
        else if (Input.GetKeyDown(KeyCode.L)) // Enable/Disable Effects (Lights, Displays, Smoke)
        {
            EnableOrDisableEffects();
        }
    }

    public void InCreaseExitsAmount()
    {
        exitsAmount += 1;
    }

    /// <summary>
    /// Starting point for the Radial Menu.
    /// </summary>
    private void HandleRadialMenuUI()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Tell the canvas to spawn a menu
            RadialMenuSpawner.instance.SpawnMenu(this); // The buttons (slots) needs .this
        }
    }

    /// <summary>
    /// Handle ms and fps calculation. Assign this information to infoText.
    /// </summary>
    private void ShowFPS()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        ms = deltaTime * 1000.0f;
        fps = 1.0f / deltaTime;
        infoText.text = "ms: " + ms + "    FPS: " + Mathf.Ceil(fps).ToString() + "\n" + "Entitys: " + entityAmount + "    Exits: " + exitsAmount + "\n" + "Mode: " + mode;
    }

    /// <summary>
    /// Controls Camera with WASD.
    /// Look around with right mouse click (holded).
    /// Zoom up and down with mouse wheel.
    /// </summary>
    private void HandleCamera()
    {
        // Control Camera with WASD
        if (Input.GetKey(KeyCode.W) /*|| Input.mousePosition.y >= Screen.height - panBorderThickness*/)
        {
            transform.Translate(Vector3.forward * cameraSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A) /*|| Input.mousePosition.x <= panBorderThickness*/)
        {
            transform.Translate(Vector3.left * cameraSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S) /*|| Input.mousePosition.y <= panBorderThickness*/)
        {
            transform.Translate(Vector3.back * cameraSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D) /*|| Input.mousePosition.x >= Screen.width - panBorderThickness*/)
        {
            transform.Translate(Vector3.right * cameraSpeed * Time.deltaTime);
        }


        // Look around with Right Mouse click
        // Use Unity own Axis
        if (Input.GetMouseButton(1))
        {
            yaw += lookSpeedH * Input.GetAxis("Mouse X");
            pitch += lookSpeedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // Zoom up and down with mouse wheel
        transform.Translate(0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, 0, Space.Self);
    }

    /// <summary>
    /// Method that takes care of N and I keys to enable/disable the information or statistic window.
    /// Disable when window is active in Hierarchy. Otherwise Enable.
    /// </summary>
    private void WindowCheck()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (informationWindowPanel.activeInHierarchy)
            {
                // Information Window is enabled
                informationWindowPanel.SetActive(false);
            }
            else
            {
                // Information Window is disabled
                informationWindowPanel.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (statisticWindowPanel.activeInHierarchy)
            {
                // Information Window is enabled
                statisticWindowPanel.SetActive(false);
            }
            else
            {
                // Information Window is disabled
                statisticWindowPanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Takes all barriers on the left and on the right and disable/enable all LOD components.
    /// </summary>
    private void EnableOrDisableLODFunction()
    {
        LODGroup[] lodsLeft = barrierLeftWithPivot.GetComponentsInChildren<LODGroup>(); // Get all LODs from left side
        LODGroup[] lodsRight = barrierRightWithPivot.GetComponentsInChildren<LODGroup>(); // Get all LODs from right side

        // Loop throught them and disable/enable them
        foreach (LODGroup lodGroup in lodsLeft)
        {
            if (lodGroup.enabled)
            {
                lodGroup.enabled = false;
            }
            else
            {
                lodGroup.enabled = true;
            }
        }

        foreach (LODGroup lodGroup in lodsRight)
        {
            if (lodGroup.enabled)
            {
                lodGroup.enabled = false;
            }
            else
            {
                lodGroup.enabled = true;
            }
        }
    }

    /// <summary>
    /// Method that Enable/Disable different effects.
    /// </summary>
    private void EnableOrDisableEffects()
    {
        // Truss
        if (trussLights.activeInHierarchy)
        {
            trussLights.SetActive(false);
            effectsEnabled = false;
        }
        else
        {
            trussLights.SetActive(true);
            effectsEnabled = true;
        }

        // Smoke
        if (smoke.activeInHierarchy)
        {
            smoke.SetActive(false);
        }
        else
        {
            smoke.SetActive(true);
        }

        // Displays
        if (displays.activeInHierarchy)
        {
            displays.SetActive(false);
        }
        else
        {
            displays.SetActive(true);
        }

        // First Sound System
        if (frontSoundSystemLight.activeInHierarchy)
        {
            frontSoundSystemLight.SetActive(false);
        }
        else
        {
            frontSoundSystemLight.SetActive(true);
        }

        // Lights of user created Sound Systems
        foreach (GameObject soundSystem in userCreatedSoundSystems)
        {
            GameObject lightObject = soundSystem.transform.Find("Lights").gameObject;
            Debug.Log(lightObject.name);
            if (lightObject.activeInHierarchy)
            {
                lightObject.SetActive(false);
            }
            else
            {
                lightObject.SetActive(true);
            }
        }

        // Lights of additional Sound Systems
        foreach (GameObject soundSystem in additionalSoundSystems)
        {
            GameObject lightObject = soundSystem.transform.Find("Lights").gameObject;
            if (lightObject.activeInHierarchy)
            {
                lightObject.SetActive(false);
            }
            else
            {
                lightObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Method that Enable/Disable Night mode
    /// </summary>
    private void EnableOrDisableNightMode()
    {
        // TODO: implementing Disable or enable night mode function
        // Dark atmosphere etc.
    }

    /// <summary>
    /// Method that Enable/Disable Orbit Camera
    /// </summary>
    private void EnableOrDisableOrbitCamera()
    {
        // TODO: implement Disable or enable Orbit Camera function.
        // Camera flying to different (random?) positions.
        // Showing different things
    }
}
