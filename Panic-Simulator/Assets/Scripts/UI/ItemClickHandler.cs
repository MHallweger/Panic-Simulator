using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the keys from the "inventory" bar on the bottom.
/// Extra: Handles some user Input.
/// </summary>
public class ItemClickHandler : MonoBehaviour
{
    #region Variables
    // Button specific
    [SerializeField] private KeyCode key; // Selected KeyCode in the inspector
    [SerializeField] private Button button; // .this

    // Barriers
    [SerializeField] private GameObject[] additionalBarriersLeft; // Contains 4 additional Barriers on the left side
    [SerializeField] private GameObject[] additionalBarriersRight; // Contains 4 additional Barriers on the right side
    [SerializeField] private GameObject[] additionalSoundSystems; // Contains 4 additional Sound Systems
    [SerializeField] private GameObject barrierLeftWithPivot;
    [SerializeField] private GameObject barrierRightWithPivot;
    [SerializeField] private float rotationSpeed; // Rotation speed of the rotating barriers
    [SerializeField] private GameObject penultimateSoundSystem; // The Sound System before the last System
    [SerializeField] private GameObject lastSoundSystem; // The last Sound System

    // Spawn places, needed in the DOTS scripts for spawning Agents inside a user created area
    [SerializeField] private GameObject spawnPlaceLeft; // First back left spawn point
    [SerializeField] private GameObject spawnPlaceRight; // First back right spawn point
    [SerializeField] private GameObject[] additionalSpawnObjectsLeft; // Additional spawn places that can be used to increase the spawn area from the festival
    [SerializeField] private GameObject[] additionalSpawnObjectsRight; // Additional spawn places that can be used to increase the spawn area from the festival
    private Vector3 originalStartingSpawnObjectPositionLeft; // ...
    private Vector3 originalStartingSpawnObjectPositionRight; // ...

    // Internals
    [SerializeField] private GameObject crowdObject; // For getting the entity AmountToSpawn
    private bool inside = false; // Bool for checking if the Enable/Disable Effects function was called
    private static int increaseCounter = 0; // For checking how many barriers are active
    private int increaseAmount = 0; // For counting the entitys
    #endregion // Variables

    /// <summary>
    /// Getting the entity AmountToSpawn from the crowdObject.
    /// </summary>
    private void Start()
    {
        increaseAmount = crowdObject.GetComponent<UnitSpawnerProxy>().AmountToSpawn;
        originalStartingSpawnObjectPositionLeft = additionalSpawnObjectsLeft[0].transform.position;
        originalStartingSpawnObjectPositionRight = additionalSpawnObjectsRight[0].transform.position;
    }

    /// <summary>
    /// Looks for key input and react on it. -> Change color and rotate barriers.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            // Color Fade effect (blue) when pressing or hovering over the different slots
            FadeToColor(button.colors.highlightedColor);
            button.onClick.Invoke();
        }
        else if (Input.GetKeyUp(key))
        {
            // Fade back to default color (white)
            FadeToColor(button.colors.normalColor);
        }

        // Rotate Barriers
        if (Input.GetKey(KeyCode.Alpha3) && !InputWindow.instance.inputField.isFocused)
        {
            // For holding the button down
            // Rotate side barriers
            OnButtonHoldingDownRotateOut();
        }
        else if (Input.GetKey(KeyCode.Alpha4) && !InputWindow.instance.inputField.isFocused)
        {
            // For holding the button down
            // Rotate side barriers
            OnButtonHoldingDownRotateIn();
        }
    }

    /// <summary>
    /// Method for changing the color of the specific button. 
    /// </summary>
    /// <param name="color">The color Input that is choosen. Blue and White -> See inspector.</param>
    private void FadeToColor(Color color)
    {
        Graphic graphic = GetComponent<Graphic>();
        graphic.CrossFadeColor(color, button.colors.fadeDuration, true, true);
    }

    /// <summary>
    /// Specific method that is called when the specific button is touched on any way.
    /// </summary>
    /// <param name="slotNumber">The slot number which was set in the Inspector.</param>
    public void OnButtonClicked(int slotNumber)
    {
        switch (slotNumber)
        {
            case 0:
                // Inspector Index is 0 for slot 1
                // Key 1 was pressed, spawn entitys, increase the entity Amount. This way is faster. Calculating the actual entity amount inside Update() would cost too much performance
                Camera.main.GetComponent<UIHandler>().entityAmount += increaseAmount;
                break;
            case 1:
                // Inspector Index is 1 for slot 2
                // Key 2 was pressed, all entitys will be removed, set entity amount to 0
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
    /// Rotates the two pivot barriers on the left and on the right, so that the user can change the festival area.
    /// There is a limit of 60°, so the User cannot rotate infinitely.
    /// </summary>
    private void OnButtonHoldingDownRotateOut()
    {
        if (barrierLeftWithPivot.transform.rotation.y <= .5f && barrierRightWithPivot.transform.rotation.y >= -.5f)
        {
            // Compare the actual rotation of the first pivot to the Unity own rotation value.
            // If -60°/60° is not reached, rotate more.
            barrierLeftWithPivot.transform.Rotate(Vector3.up * Time.deltaTime * (rotationSpeed / 4), Space.World);
            barrierRightWithPivot.transform.Rotate(Vector3.down * Time.deltaTime * (rotationSpeed / 4), Space.World);

            // Control the last two Sound Systems
            // Otherwise they will exists on an empty festival area where no people are
            if (barrierLeftWithPivot.transform.rotation.y <= .25f && barrierRightWithPivot.transform.rotation.y >= -.25f)
            {
                if (penultimateSoundSystem.activeInHierarchy == true && lastSoundSystem.activeInHierarchy == true)
                {
                    penultimateSoundSystem.SetActive(true);
                    lastSoundSystem.SetActive(true);
                }
            }
            else
            {
                penultimateSoundSystem.SetActive(false);
                lastSoundSystem.SetActive(false);
            }
        }
        else
        {
            // If it is, throw a log message.
            Debug.Log("You can only stretch the area out, until you reach a degree of 60°.");
        }
    }

    /// <summary>
    /// Rotates the two pivot barriers on the left and on the right, so that the user can change the festival area.
    /// There is a limit of 0.0°, so the User cannot rotate infinitely.
    /// </summary>
    private void OnButtonHoldingDownRotateIn()
    {
        if (barrierLeftWithPivot.transform.rotation.y <= .51f && barrierRightWithPivot.transform.rotation.y >= -.51f
            && barrierLeftWithPivot.transform.rotation.y > 0.0005f && barrierRightWithPivot.transform.rotation.y < -0.0005f)
        {
            // Compare the actual rotation of the first pivot to the Unity own rotation value.
            // Rotate with inside direction if the barriers are in the given area.
            // Stop with an offset of 0.0005. This helps for stopping on an rotation of Vector3.zero
            barrierLeftWithPivot.transform.Rotate(Vector3.down * Time.deltaTime * (rotationSpeed / 4), Space.World);
            barrierRightWithPivot.transform.Rotate(Vector3.up * Time.deltaTime * (rotationSpeed / 4), Space.World);

            // Control the last two Sound Systems
            // Otherwise they will exists on an empty festival area where no people are
            if (barrierLeftWithPivot.transform.rotation.y <= .25f && barrierRightWithPivot.transform.rotation.y >= -.25f)
            {
                if (lastSoundSystem.transform.parent.Find("Sound System_3").gameObject.activeInHierarchy)
                {
                    penultimateSoundSystem.SetActive(true);
                    lastSoundSystem.SetActive(true);
                }
            }
            else
            {
                penultimateSoundSystem.SetActive(false);
                lastSoundSystem.SetActive(false);
            }
        }
        else
        {
            // If they are not inside the given area
            Debug.Log("You can only stretch the area in, until you reach a degree of 0.0°.");
        }
    }

    /// <summary>
    /// Adds a new barrier to both of the starting barriers.
    /// The barriers exists already, they are disabled and need to be enabled.
    /// There is an increaseCounter which counts all the time for knowing, how many barriers are added.
    /// Extra: Set an extra Sound System for each additional Barrier.
    /// </summary>
    private void AddNewSideBarrier()
    {
        if (!InputWindow.instance.inputField.isFocused)
        {
            if (increaseCounter == additionalBarriersLeft.Length)
            {
                // The counter has the same value like the length of the additionalBarriers Array. The Next Barrier would lead to NullPointer
                // Throw warning/log and return
                Debug.LogWarning("Limit reached!");
                Debug.Log("The barrier limit for this simulation was reached.");
                return;
            }

            // Set the next Barriers and sound system to active
            additionalBarriersLeft[increaseCounter].SetActive(true);
            additionalBarriersRight[increaseCounter].SetActive(true);
            additionalSoundSystems[increaseCounter].SetActive(true);

            // Set spawnpoint 1 position to spawnpoint additionalBarriersLeft[increaseCounter] position
            // The DOTS System will be able now to spawn agents in the new area
            spawnPlaceLeft.transform.position = additionalSpawnObjectsLeft[increaseCounter + 1].transform.position;
            spawnPlaceRight.transform.position = additionalSpawnObjectsRight[increaseCounter + 1].transform.position;
            #region Debug Barriers
            //Debug.Log(spawnPlaceLeft.transform.position);
            //Debug.Log(spawnPlaceRight.transform.position);
            Debug.DrawLine(spawnPlaceLeft.transform.position, new Vector3(spawnPlaceLeft.transform.position.x, spawnPlaceLeft.transform.position.y + 50f, spawnPlaceLeft.transform.position.z), Color.blue, 1f);
            Debug.DrawLine(spawnPlaceRight.transform.position, new Vector3(spawnPlaceRight.transform.position.x, spawnPlaceRight.transform.position.y + 50f, spawnPlaceRight.transform.position.z), Color.blue, 1f);
            #endregion // Debug Barriers
            if (!(increaseCounter == additionalBarriersLeft.Length))
            {
                // Only increase the counter when the limit is not reached yet.
                increaseCounter++;
            }
        }
    }

    /// <summary>
    /// Removes Side Barriers and Sound Systems.
    /// Increase counter helps to identify how many Barriers are active.
    /// </summary>
    private void RemoveNewSideBarrier()
    {
        if (!InputWindow.instance.inputField.isFocused)
        {
            if (increaseCounter != 0) // Prevent of getting the -1th Object of the Arrays != 0
            {
                // Set the Objects to false, to disable the GameObjects and going back with the barriers and the Sound Systems
                additionalBarriersLeft[increaseCounter - 1].SetActive(false);
                additionalBarriersRight[increaseCounter - 1].SetActive(false);
                additionalSoundSystems[increaseCounter - 1].SetActive(false);

                if (increaseCounter == 1)
                {
                    // Change the spawnplace back
                    spawnPlaceLeft.transform.position = originalStartingSpawnObjectPositionLeft;
                    spawnPlaceRight.transform.position = originalStartingSpawnObjectPositionRight;
                }
                else
                {
                    // Change the spawnplace back
                    spawnPlaceLeft.transform.position = additionalSpawnObjectsLeft[increaseCounter - 1].transform.position;
                    spawnPlaceRight.transform.position = additionalSpawnObjectsRight[increaseCounter - 1].transform.position;
                }
                #region Debug Barriers
                //Debug.Log(spawnPlaceLeft.transform.position);
                //Debug.Log(spawnPlaceRight.transform.position);
                Debug.DrawLine(spawnPlaceLeft.transform.position, new Vector3(spawnPlaceLeft.transform.position.x, spawnPlaceLeft.transform.position.y + 50f, spawnPlaceLeft.transform.position.z), Color.red, 1f);
                Debug.DrawLine(spawnPlaceRight.transform.position, new Vector3(spawnPlaceRight.transform.position.x, spawnPlaceRight.transform.position.y + 50f, spawnPlaceRight.transform.position.z), Color.red, 1f);
                #endregion // Debug Barriers
                // Decrease the counter
                increaseCounter--;

            }
            else if (increaseCounter <= 0) // else
            {
                // Throw Warning/log when increaseCounter is 0, return
                Debug.LogWarning("Limit reached!");
                Debug.Log("The minimum limit for the barriers is one.");
                return;
            }
        }
    }
}
