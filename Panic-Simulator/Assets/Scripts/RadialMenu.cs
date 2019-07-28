using System.Collections;
using System.Collections.Generic;
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
    #endregion // Variables

    public void SpawnButtons(PanicECS panicECS)
    {
        StartCoroutine(AnimateButtons(panicECS));
    }

    IEnumerator AnimateButtons(PanicECS panicECS)
    {
        for (int i = 0; i < panicECS.options.Length; i++)
        {
            // Create the button, set as a child of the Radial Menu
            RadialButton radialButton = Instantiate(buttonPrefab) as RadialButton; // TODO ECS
            radialButton.transform.SetParent(transform, false);

            // Distance around the circle
            float theta = (2 * Mathf.PI / panicECS.options.Length) * i;

            // Convert theta to x/y position
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);

            // Set the button position
            radialButton.transform.localPosition = new Vector3(xPos, yPos, 0.0f) * buttonDistance;

            // Set individual button settings
            radialButton.circle.color = panicECS.options[i].color;
            radialButton.icon.sprite = panicECS.options[i].sprite;
            radialButton.title = panicECS.options[i].title;
            radialButton.radialMenu = this;

            // Animations here...
            yield return new WaitForSeconds(0.06f);
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (selected)
            {
                Debug.Log(selected.title + " was selected! \n");
                Debug.Log("Call function X...");
            }
            Destroy(gameObject); // TODO ECS
        }
    }
}
