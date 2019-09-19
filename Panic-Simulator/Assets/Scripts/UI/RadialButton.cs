using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Each Button gets it own Class which takes a look if the mouse is hovered over itself or not.
/// React when mouse hover over itself.
/// </summary>
public class RadialButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler // Additional Interfaces that helps identify if the mouse hovered over the buttons
{
    #region Variables
    // Button
    public Image circle; // Button color
    public Image icon; // Button icon
    public string title; // Button title
    private Color defaultColor; // Saves the original color. When enter, the button will be white, when leaving, the button gets its own color back

    // Radial Menu Reference
    public RadialMenu radialMenu; // The radial menu parent
    #endregion // Variables

    /// <summary>
    /// Begin over button hover.
    /// </summary>
    /// <param name="eventData">Contains different data of the enter hover event</param>
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        radialMenu.selected = this; // So selected exists and the Radial Menu knows which button was choosen, it starts working when Tab button is lifted.
        RadialMenuSpawner.instance.updatedMenuText = title; // Sets the mid Radial Menu label to the actual choosen (hovered) button
        defaultColor = circle.color; // Save the original Button color
        circle.color = Color.white; // Set it to white
    }

    /// <summary>
    /// Exit over button hover
    /// </summary>
    /// <param name="eventData">Contains different data of the exit hover event</param>
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        radialMenu.selected = null; // Selected do not exists anymore
        RadialMenuSpawner.instance.updatedMenuText = "Actions"; // Reset the mid Radial Menu text to original "Actions"
        circle.color = defaultColor; // Set the default original button color
    }
}
