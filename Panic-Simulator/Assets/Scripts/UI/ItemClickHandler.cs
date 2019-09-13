using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the keys from the "inventory" bar on the bottom.
/// </summary>
public class ItemClickHandler : MonoBehaviour
{
    #region Variables
    [SerializeField] private KeyCode key;
    [SerializeField] private Button button;
    [SerializeField] private GameObject barrierLeftWithPivot; // hier
    [SerializeField] private GameObject barrierRightWithPivot; // hier
    [SerializeField] private float rotationSpeed;

    [SerializeField] private GameObject[] additionalBarriersLeft; // Contains 4 additional Barriers on the left side
    [SerializeField] private GameObject[] additionalBarriersRight; // Contains 4 additional Barriers on the right side

    [SerializeField] private GameObject spawnPlaceLeft; // First back left spawn point
    [SerializeField] private GameObject spawnPlaceRight; // First back right spawn point

    [SerializeField] private GameObject[] additionalSpawnObjectsLeft; // Additional spawn places that can be used to increase the spawn area from the festival
    [SerializeField] private GameObject[] additionalSpawnObjectsRight; // Additional spawn places that can be used to increase the spawn area from the festival

    // Effects
    [SerializeField] private GameObject trussLights; // The GameObject that holds all lights on the stage
    [SerializeField] private GameObject smoke; // The smoke GameObject
    [SerializeField] private GameObject displays; // The GameObject that holds all displays
    [SerializeField] private GameObject background; // The Background GameObject

    private static int increaseCounter = 0;
    private int increaseAmount = 0;
    [SerializeField] private GameObject crowdObject;
    #endregion // Variables

    private void Start()
    {
        increaseAmount = crowdObject.GetComponent<UnitSpawnerProxy>().AmountToSpawn;
    }

    /// <summary>
    /// Looks for key input and react on it. -> Change color and rotate barriers.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            FadeToColor(button.colors.highlightedColor);
            button.onClick.Invoke();
        }
        else if (Input.GetKeyUp(key))
        {
            FadeToColor(button.colors.normalColor);
        }


        if (Input.GetKey(KeyCode.Alpha3))
        {
            // For holding the button down
            // Rotate side barriers
            OnButtonHoldingDownRotateOut();
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            OnButtonHoldingDownRotateIn();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIHandler.instance.mode = "-";
            Actions.instance.convertBarriers = false;
            Actions.instance.fallingTruss = false;
            Actions.instance.groundExplosion = false;
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            EnableOrDisableLODFunction();
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            EnableOrDisableNightMode(); // TODO implement
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            EnableOrDisableOrbitCamera(); // TODO implement
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            EnableOrDisableEffects();
        }
    }

    /// <summary>
    /// Method for changing the color of the specific buton. 
    /// </summary>
    /// <param name="color">The color Input that is choosen. Blue and White -> See inspector</param>
    private void FadeToColor(Color color)
    {
        Graphic graphic = GetComponent<Graphic>();
        graphic.CrossFadeColor(color, button.colors.fadeDuration, true, true);
    }

    /// <summary>
    /// Specific method that is called when the specific button is touched on any way.
    /// </summary>
    /// <param name="slotNumber"></param>
    public void OnButtonClicked(int slotNumber)
    {
        switch (slotNumber)
        {
            case 0:
                // Inspector Index is 0 for slot 1
                // Key 1 was pressed
                Camera.main.GetComponent<UIHandler>().entityAmount += increaseAmount;
                break;
            case 1:
                // Inspector Index is 1 for slot 2
                // Key 2 was pressed
                Camera.main.GetComponent<UIHandler>().entityAmount = 0;
                break;
            case 4:
                // Inspector Index is 4 for slot 5
                // Key 5 was pressed
                AddNewSideBarrier();
                break;
            case 5:
                // Inspector Index is 5 for slot 6
                // Key 6 was pressed
                RemoveNewSideBarrier();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Rotates the two barriers on the left and on the right, so that the user can change the festival area.
    /// </summary>
    private void OnButtonHoldingDownRotateOut()
    {
        if (barrierLeftWithPivot.transform.rotation.y <= .5f && barrierRightWithPivot.transform.rotation.y >= -.5f)
        {
            barrierLeftWithPivot.transform.Rotate(Vector3.up * Time.deltaTime * (rotationSpeed / 4), Space.World);
            barrierRightWithPivot.transform.Rotate(Vector3.down * Time.deltaTime * (rotationSpeed / 4), Space.World);
        }
        else
        {
            Debug.Log("You can only stretch the area out, until you reach a degree of 60°.");
        }
    }

    private void OnButtonHoldingDownRotateIn()
    {
        if (barrierLeftWithPivot.transform.rotation.y <= .51f && barrierRightWithPivot.transform.rotation.y >= -.51f
            && barrierLeftWithPivot.transform.rotation.y > 0.0005f && barrierRightWithPivot.transform.rotation.y < -0.0005f)
        {
            barrierLeftWithPivot.transform.Rotate(Vector3.down * Time.deltaTime * (rotationSpeed / 4), Space.World);
            barrierRightWithPivot.transform.Rotate(Vector3.up * Time.deltaTime * (rotationSpeed / 4), Space.World);
        }
        else
        {
            Debug.Log("You can only stretch the area in, until you reach a degree of 0.0°.");
        }
    }

    private void AddNewSideBarrier()
    {
        if (increaseCounter == additionalBarriersLeft.Length && increaseCounter == additionalBarriersRight.Length)
        {
            Debug.LogWarning("Limit reached!");
            Debug.Log("The barrier limit for this simulation was reached.");
            return;
        }

        additionalBarriersLeft[increaseCounter].SetActive(true);
        additionalBarriersRight[increaseCounter].SetActive(true);

        // Set spawnpoint 1 position to spawnpoint additionalBarriersLeft[increaseCounter] position
        spawnPlaceLeft.transform.position = additionalSpawnObjectsLeft[increaseCounter].transform.position;
        spawnPlaceRight.transform.position = additionalSpawnObjectsRight[increaseCounter].transform.position;

        if (!(increaseCounter == additionalBarriersLeft.Length && increaseCounter == additionalBarriersRight.Length))
        {
            // Only increase the counter when the limit is not reached yet.
            increaseCounter++;
        }
    }

    private void RemoveNewSideBarrier()
    {
        if (increaseCounter != 0) // Two of them necessary because of the following code that needs to be executed
        {
            additionalBarriersLeft[increaseCounter - 1].SetActive(false);
            additionalBarriersRight[increaseCounter - 1].SetActive(false);


            spawnPlaceLeft.transform.position = additionalSpawnObjectsLeft[increaseCounter - 1].transform.position;
            spawnPlaceRight.transform.position = additionalSpawnObjectsRight[increaseCounter - 1].transform.position;

            increaseCounter--;

        }
    }

    private void EnableOrDisableLODFunction()
    {
        LODGroup[] lodsLeft = barrierLeftWithPivot.GetComponentsInChildren<LODGroup>();
        LODGroup[] lodsRight = barrierRightWithPivot.GetComponentsInChildren<LODGroup>();

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

    private void EnableOrDisableNightMode()
    {
        // TODO: implementing Disable or enable night mode function
        // Dark atmosphere etc.
    }

    private void EnableOrDisableOrbitCamera()
    {
        // TODO: implement Disable or enable Orbit Camera function.
        // Camera flying to different (random?) positions.
        // Showing different things
    }

    private void EnableOrDisableEffects()
    {
        // TODO: implement disable or enable lights/smoke/effects function.
        // This function turns on/off all effects.
        // Truss Lights
        if (trussLights.activeInHierarchy)
        {
            trussLights.SetActive(false);
        }
        else
        {
            trussLights.SetActive(true);
        }

        // Smoke
        if (smoke.activeInHierarchy)
        {
            trussLights.SetActive(false);
        }
        else
        {
            smoke.SetActive(true);
        }

        // Displays
        if (displays.activeInHierarchy)
        {
            trussLights.SetActive(false);
        }
        else
        {
            displays.SetActive(true);
        }

        // Background
        if (background.activeInHierarchy)
        {
            trussLights.SetActive(false);
        }
        else
        {
            background.SetActive(true);
        }
    }
}
