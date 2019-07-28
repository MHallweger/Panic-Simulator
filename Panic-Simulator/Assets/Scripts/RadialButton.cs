using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RadialButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image circle; // button color
    public Image icon; // the button icon
    public string title;
    public RadialMenu radialMenu; // The radial menu parent

    private Color defaultColor;

    /// <summary>
    /// Begin over button hover
    /// </summary>
    /// <param name="eventData"></param>
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        radialMenu.selected = this;
        defaultColor = circle.color;
        circle.color = Color.white;
    }

    /// <summary>
    /// Exit over button hover
    /// </summary>
    /// <param name="eventData"></param>
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        radialMenu.selected = null;
        circle.color = defaultColor;
    }
}
