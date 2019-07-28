using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuSpawner : MonoBehaviour
{
    #region Variables
    // Singleton Instance Variable
    public static RadialMenuSpawner instance;

    // Radial Menu
    public RadialMenu menuPrefab; // The instantiated prefab when tab is pressed
    #endregion // Variables

    private void Awake()
    {
        instance = this;
    }

    public void SpawnMenu(PanicECS panicECS)
    {
        RadialMenu radialMenu = Instantiate(menuPrefab) as RadialMenu; // TODO ECS
        radialMenu.transform.SetParent(transform, false); // Set to Canvas transform (Spawner) as parent
        radialMenu.transform.position = Input.mousePosition; // For spawning the Radial Menu on the mouse positio
        radialMenu.label.text = panicECS.title.ToUpper();
        radialMenu.SpawnButtons(panicECS);
    }

}
