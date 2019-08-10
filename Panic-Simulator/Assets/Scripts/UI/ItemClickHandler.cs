using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemClickHandler : MonoBehaviour
{
    [SerializeField] private KeyCode key;
    [SerializeField] private Button button;

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
    }

    private void FadeToColor(Color color)
    {
        Graphic graphic = GetComponent<Graphic>();
        graphic.CrossFadeColor(color, button.colors.fadeDuration, true, true);
    }

    public void OnButtonClicked(int slotNumber)
    {
        Debug.Log("Slot " + (slotNumber + 1) + " wurde gedrückt!");
    }


}
