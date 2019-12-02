using UnityEngine;

/// <summary>
/// Handles the actual Spawn of the Radial Menu.
/// </summary>
public class RadialMenuSpawner : MonoBehaviour
{
    #region Variables
    // Singleton Instance Variable
    public static RadialMenuSpawner instance; // For accessing this instance

    // Radial Menu
    public string updatedMenuText = "Actions"; // Mid text inside Radial Menu
    public RadialMenu radialMenu; // Reference
    public RadialMenu menuPrefab; // The instantiated prefab when tab is pressed
    #endregion // Variables

    /// <summary>
    /// Always set an updated version of the radial menu text.
    /// </summary>
    private void Update()
    {
        if (radialMenu != null)
        {
            radialMenu.label.text = updatedMenuText;
        }
    }

    /// <summary>
    /// Set the String "Actions" at the beginning, otherwise the Radial Menu string would be empty. Assign Instance Variable.
    /// </summary>
    private void Awake()
    {
        instance = this;
        if (radialMenu != null)
        {
            radialMenu.label.text = updatedMenuText;
        }
    }

    /// <summary>
    /// Spawns the actual Radial Menu.
    /// Sets different options to this menu.
    /// </summary>
    /// <param name="uiHandler">Used inside of the SpawnButtons method inside the Radial Menu Instance.</param>
    public void SpawnMenu(UIHandler uiHandler)
    {
        radialMenu = Instantiate(menuPrefab) as RadialMenu; // Instantiate the Radial Menu
        radialMenu.transform.SetParent(transform, false); // Set to Canvas transform (Spawner) as parent
        radialMenu.transform.position = Input.mousePosition; // For spawning the Radial Menu on the mouse position
        radialMenu.label.text = "Actions"; // Sets label Text of the Radial Menu
        radialMenu.SpawnButtons(uiHandler); // Call button method
    }
}
