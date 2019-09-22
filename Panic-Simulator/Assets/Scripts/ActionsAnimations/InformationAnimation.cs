using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationAnimation : MonoBehaviour
{
    private GameObject informationArrow; // The Child GameObject that contains the Information Arrow

    void Start()
    {
        informationArrow = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (UIHandler.instance.enableArrows)
        {
            // If this bool is true, enable the actual animation and enable the whole child GameObject
            informationArrow.GetComponent<Animator>().SetBool("InformationArrow", true);
            informationArrow.SetActive(true);
        }
        else
        {
            // If this bool is false, disable the actual animation and disable the whole Child GameObject
            informationArrow.GetComponent<Animator>().SetBool("InformationArrow", false);
            informationArrow.SetActive(false);
        }
    }
}
