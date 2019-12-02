using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Functionality script for Falling Trusses.
/// </summary>
public class InformationAnimationTruss : MonoBehaviour
{
    #region Variables
    // GameObjects
    private GameObject informationArrow; // The Child GameObject that contains the Information Arrow
    #endregion // Variables

    void Start()
    {
        // Get whole arrow Game Object (first in hierarchy)
        informationArrow = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        EnableDisableArrows();
    }

    /// <summary>
    /// React on enableTrussArrows, set whole arrow Game Object On/Off.
    /// </summary>
    private void EnableDisableArrows()
    {
        if (UIHandler.instance.enableTrussArrows)
        {
            // If this bool is true, enable the actual animation and enable the whole child GameObject
            informationArrow.GetComponent<Animator>().SetBool("InformationArrowTruss", true);
            informationArrow.SetActive(true);
        }
        else
        {
            // If this bool is false, disable the actual animation and disable the whole Child GameObject
            informationArrow.GetComponent<Animator>().SetBool("InformationArrowTruss", false);
            informationArrow.SetActive(false);
        }
    }
}
