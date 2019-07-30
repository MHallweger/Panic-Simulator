using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using System.Collections;

public class PanicECS : MonoBehaviour
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
    // Singleton Instance Variable
    public static PanicECS instance;

    // Radial Menu UI
    public string title; // Label text
    public Action[] options;

    // Camera
    [SerializeField] private float zoomSpeed; // Speed Variable for zoom up/down feature
    [SerializeField] private float cameraSpeed; // Speed Variable for camera movement
    private float lookSpeedH = 2f; // Variable for Camera rotation feature
    private float lookSpeedV = 2f; // Variable for Camera rotation feature
    private float yaw = 0f; // Variable for Camera rotation feature
    private float pitch = 0f; // Variable for Camera rotation feature

    // ECS
    private EntityManager entityManager;
    private Entity humanEntityPrefab;
    [SerializeField] private int humanAmount;
    [SerializeField] private GameObject humanGameObject;
    #endregion // Variables

    private void Start()
    {
        if (title == "" || title == null)
        {
            title = gameObject.name;
        }

        BeginECSProcess();
    }

    private void Awake()
    {
        instance = this;

        HandleECS();
    }

    void Update()
    {
        HandleRadialMenuUI();
        HandleCamera();
    }

    private void BeginECSProcess()
    {
        NativeArray<Entity> humans = new NativeArray<Entity>(humanAmount, Allocator.Temp);
        entityManager.Instantiate(humanEntityPrefab, humans);

        for (int i = 0; i < humans.Length; i++)
        {
            entityManager.SetComponentData(humans[i], new Translation { Value = new float3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10))});
        }

        humans.Dispose();
    }

    private void HandleECS()
    {
        entityManager = World.Active.EntityManager;
        humanEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(humanGameObject, World.Active);
    }

    private void HandleRadialMenuUI()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Tell the canvas to spawn a menu
            RadialMenuSpawner.instance.SpawnMenu(this); // The buttons (slots) needs .this
        }
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
            pitch -= lookSpeedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        //Zoom up and down with mouse wheel [DONE]
        transform.Translate(0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, 0, Space.Self);
    }
}
