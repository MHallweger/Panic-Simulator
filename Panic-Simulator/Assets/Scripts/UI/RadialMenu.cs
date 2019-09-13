using System.Collections;
using UnityEngine;
using TMPro;

public class RadialMenu : MonoBehaviour
{
    #region Variables
    // Radial Buttons
    public TextMeshProUGUI label;
    public RadialButton buttonPrefab; // The buttons on the RadialMenu
    public RadialButton selected; // The selected Button (Slot)
    [SerializeField] private float buttonDistance = 100.0f; // The distance from the mouse to the button slots
    private Actions actions;
    #endregion // Variables

    private void Awake()
    {
        label.text = "Actions";

        actions = Actions.instance;
    }

    public void SpawnButtons(UIHandler uiHandler)
    {
        StartCoroutine(AnimateButtons(uiHandler));
    }

    IEnumerator AnimateButtons(UIHandler uiHandler)
    {
        for (int i = 0; i < uiHandler.options.Length; i++)
        {
            // Create the button, set as a child of the Radial Menu
            RadialButton radialButton = Instantiate(buttonPrefab) as RadialButton; // TODO ECS
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

            // Animations here...
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (selected)
            {
                RadialMenuSpawner.instance.radialMenu.label.text = "Actions";
                RadialMenuSpawner.instance.updatedMenuText = "Actions";

                if (selected.title == "Create Exits")
                {
                    actions.convertBarriers = true;
                    actions.groundExplosion = false;
                    actions.fallingTruss = false;
                    actions.dropSoundSystem = false;
                    UIHandler.instance.mode = "Create Exits";
                }
                else if (selected.title == "Small Explosion")
                {
                    actions.groundExplosion = true;
                    actions.convertBarriers = false;
                    actions.fallingTruss = false;
                    actions.dropSoundSystem = false;
                    UIHandler.instance.mode = "Small Explosions";
                }
                else if (selected.title == "Falling Truss")
                {
                    actions.fallingTruss = true;
                    actions.groundExplosion = false;
                    actions.convertBarriers = false;
                    actions.dropSoundSystem = false;
                    UIHandler.instance.mode = "Falling Truss";
                }
                else if (selected.title == "Drop Sound System")
                {
                    actions.dropSoundSystem = true;
                    actions.fallingTruss = false;
                    actions.groundExplosion = false;
                    actions.convertBarriers = false;
                    UIHandler.instance.mode = "Drop Sound System";
                }
            }
            Destroy(gameObject);
        }
    }
}
