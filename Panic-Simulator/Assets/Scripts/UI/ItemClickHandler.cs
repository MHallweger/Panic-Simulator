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
    [SerializeField] private GameObject barrierLeftWithPivot;
    [SerializeField] private GameObject barrierRightWithPivot;
    [SerializeField] private float rotationSpeed;
    #endregion // Variables

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

        if (Input.GetKey(key))
        {
            // For holding the button down
            OnButtonHoldingDown();
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
        Debug.Log("Slot " + (slotNumber + 1) + " wurde gedrückt!");
        // TODO: Starting point for activating the "set new border values" process.##########################################
    }

    /// <summary>
    /// Rotates the two barriers on the left and on the right, so that the user can change the festival area.
    /// </summary>
    private void OnButtonHoldingDown()
    {
        if (barrierLeftWithPivot.transform.rotation.y <= .5f && barrierRightWithPivot.transform.rotation.y >= -.5f)
        {
            barrierLeftWithPivot.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed / 4, Space.World);
            barrierRightWithPivot.transform.Rotate(Vector3.down * Time.deltaTime * rotationSpeed / 4, Space.World);
        }
        else
        {
            Debug.Log("You can only stretch the area, until you reach a degree of 60.");
        }
    }
}
