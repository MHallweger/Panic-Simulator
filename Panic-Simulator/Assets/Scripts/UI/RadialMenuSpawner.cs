using UnityEngine;

public class RadialMenuSpawner : MonoBehaviour
{
    #region Variables
    // Singleton Instance Variable
    public static RadialMenuSpawner instance;
    public string updatedMenuText = "Actions";
    public RadialMenu radialMenu;

    // Radial Menu
    public RadialMenu menuPrefab; // The instantiated prefab when tab is pressed
    #endregion // Variables

    private void Update()
    {
        if (radialMenu != null)
        {
            radialMenu.label.text = updatedMenuText;
        }
    }

    private void Awake()
    {
        instance = this;
        if (radialMenu != null)
        {
            radialMenu.label.text = updatedMenuText;
        }
    }

    public void SpawnMenu(UIHandler uiHandler)
    {
        radialMenu = Instantiate(menuPrefab) as RadialMenu; // TODO ECS
        radialMenu.transform.SetParent(transform, false); // Set to Canvas transform (Spawner) as parent
        radialMenu.transform.position = Input.mousePosition; // For spawning the Radial Menu on the mouse positio
        radialMenu.label.text = "Actions";
        radialMenu.SpawnButtons(uiHandler);
    }

}
