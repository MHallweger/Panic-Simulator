﻿using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Spawns the different buttons which are needed for the Radial Menu.
/// Listen on Radial Menu User Input. Which Radial Menu Button was choosen.
/// </summary>
public class RadialMenu : MonoBehaviour
{
    #region Variables
    // Radial Buttons
    [SerializeField] private float buttonDistance = 100.0f; // The distance from the mouse to the button slots
    public TextMeshProUGUI label; // Menu Label
    public RadialButton buttonPrefab; // The buttons on the RadialMenu
    public RadialButton selected; // The selected Button (Slot)

    // Instance Variable Access (Actions)
    private Actions actions; // Shortcut for Actions.instance
    #endregion // Variables

    /// <summary>
    /// Sets Label. 
    /// Sets Instance Variable shortcut to prevent using Actions.instance everytime later.
    /// </summary>
    private void Awake()
    {
        label.text = "Actions";

        actions = Actions.instance;
    }

    /// <summary>
    /// Method that calls the Coroutine AnimateButtons() which spawns the buttons.
    /// </summary>
    /// <param name="uiHandler">Used inside the AnimateButtons function</param>
    public void SpawnButtons(UIHandler uiHandler)
    {
        StartCoroutine(AnimateButtons(uiHandler));
    }

    /// <summary>
    /// Spawns the buttons and set the calculated circle position to each of the buttons.
    /// Sets some individual settings to each button and waits until spawning another button. -> Small Animation "Spawn effect".
    /// </summary>
    /// <param name="uiHandler">For accessing different settings which are set in the Inspector.</param>
    /// <returns>Wait until spawning a new button.</returns>
    IEnumerator AnimateButtons(UIHandler uiHandler)
    {
        for (int i = 0; i < uiHandler.options.Length; i++)
        {
            // Create the button, set as a child of the Radial Menu
            RadialButton radialButton = Instantiate(buttonPrefab) as RadialButton;
            radialButton.transform.SetParent(transform, false);

            // Distance around the circle
            float theta = (2 * Mathf.PI / uiHandler.options.Length) * i;

            // Convert theta to x/y position
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);

            // Set the button position
            radialButton.transform.localPosition = new Vector3(xPos, yPos, 0.0f) * buttonDistance;

            // Set individual button settings
            radialButton.circle.color = uiHandler.options[i].color;
            radialButton.icon.sprite = uiHandler.options[i].sprite;
            radialButton.title = uiHandler.options[i].title;
            radialButton.radialMenu = this;

            // Animation
            yield return new WaitForSeconds(.07f);
        }
    }

    /// <summary>
    /// When Tab pressed -> When selected exists -> Set Radial Menu text and listen on which button the selected button is.
    /// Access the correct keyword on the Action instance and set it to true. Actions will now execute a seperate method.
    /// The rest of the keywords is set to false.
    /// The mode is set to this specific keyword, to display it to the user (Statistic Window).
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            CheckRadialMenuSelection();
        }
    }

    /// <summary>
    /// React on selected Buttons, enable/disable bools.
    /// </summary>
    public void CheckRadialMenuSelection()
    {
        if (selected) // The selected Radial Menu Button
        {
            // Set default Radial Menu title
            RadialMenuSpawner.instance.radialMenu.label.text = "Actions";
            RadialMenuSpawner.instance.updatedMenuText = "Actions";

            if (selected.title == "Ausgang erstellen")
            {
                // Create Exits Button choosen
                actions.createExits = true;
                actions.actionEnabled = false;
                actions.fallingTruss = false;
                actions.trussHasFallen = false;
                actions.dropSoundSystem = false;
                actions.smallGroundExplosion = false;
                actions.mediumGroundExplosion = false;
                actions.bigGroundExplosion = false;
                actions.fire = false;
                UIHandler.instance.mode = "Ausgang erstellen";

                // Set bool true that will be used in the animator script
                UIHandler.instance.enableTrussArrows = false;
                UIHandler.instance.enableSoundSystemArrows = false;

                // Set bool in UI Handler (Synch Point) -> Triggers DOTS script to enable the panic System
            }
            else if (selected.title == "Kleine Explosion")
            {
                // Small Explosion Button choosen
                actions.smallGroundExplosion = true;
                actions.actionEnabled = true;

                actions.mediumGroundExplosion = false;
                actions.bigGroundExplosion = false;
                actions.createExits = false;
                actions.fallingTruss = false;
                actions.trussHasFallen = false;
                actions.dropSoundSystem = false;
                actions.fire = false;
                UIHandler.instance.mode = "Kleine Explosion";

                // Set bool true that will be used in the animator script
                UIHandler.instance.enableTrussArrows = false;
                UIHandler.instance.enableSoundSystemArrows = false;
            }
            else if (selected.title == "Mittlere Explosion")
            {
                // Medium Explosion Button choosen
                actions.mediumGroundExplosion = true;
                actions.actionEnabled = true;

                actions.smallGroundExplosion = false;
                actions.bigGroundExplosion = false;
                actions.createExits = false;
                actions.fallingTruss = false;
                actions.trussHasFallen = false;
                actions.dropSoundSystem = false;
                actions.fire = false;
                UIHandler.instance.mode = "Mittlere Explosion";

                // Set bool true that will be used in the animator script
                UIHandler.instance.enableTrussArrows = false;
                UIHandler.instance.enableSoundSystemArrows = false;
            }
            else if (selected.title == "Große Explosion")
            {
                // Big Explosion Button choosen
                actions.bigGroundExplosion = true;
                actions.actionEnabled = true;

                actions.mediumGroundExplosion = false;
                actions.smallGroundExplosion = false;
                actions.createExits = false;
                actions.fallingTruss = false;
                actions.trussHasFallen = false;
                actions.dropSoundSystem = false;
                actions.fire = false;
                UIHandler.instance.mode = "Große Explosion";

                // Set bool true that will be used in the animator script
                UIHandler.instance.enableTrussArrows = false;
                UIHandler.instance.enableSoundSystemArrows = false;
            }
            else if (selected.title == "Fallender Mast")
            {
                // Falling Truss Button choosen
                actions.fallingTruss = true;
                actions.actionEnabled = true;

                actions.createExits = false;
                actions.dropSoundSystem = false;
                actions.smallGroundExplosion = false;
                actions.mediumGroundExplosion = false;
                actions.bigGroundExplosion = false;
                actions.fire = false;
                UIHandler.instance.mode = "Fallender Mast";

                // Set bool true that will be used in the animator script
                UIHandler.instance.enableTrussArrows = true;
                UIHandler.instance.enableSoundSystemArrows = false;
            }
            else if (selected.title == "Sound System erstellen")
            {
                // Drop Sound System Button choosen
                actions.dropSoundSystem = true;
                actions.fallingTruss = false;
                actions.trussHasFallen = false;
                actions.createExits = false;
                actions.smallGroundExplosion = false;
                actions.mediumGroundExplosion = false;
                actions.bigGroundExplosion = false;
                actions.fire = false;
                UIHandler.instance.mode = "Sound System erstellen";

                // Set bool true that will be used in the animator script
                UIHandler.instance.enableTrussArrows = false;
                UIHandler.instance.enableSoundSystemArrows = false;
            }
            else if (selected.title == "Feuer")
            {
                // Fire Button choosen
                actions.fire = true;
                actions.actionEnabled = true;

                actions.dropSoundSystem = false;
                actions.fallingTruss = false;
                actions.trussHasFallen = false;
                actions.createExits = false;
                actions.smallGroundExplosion = false;
                actions.mediumGroundExplosion = false;
                actions.bigGroundExplosion = false;
                UIHandler.instance.mode = "Feuer";

                // Set bool true that will be used in the animator script
                UIHandler.instance.enableTrussArrows = false;
                UIHandler.instance.enableSoundSystemArrows = true;
            }
        }
        // A button was choosen so the Radial Menu is not needed anymore
        Destroy(gameObject);
    }
}
