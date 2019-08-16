using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Script for changing the stage background color
/// </summary>
public class StageBackgroundChange : MonoBehaviour
{
    [SerializeField] private GameObject[] stageBackgrounds = new GameObject[4]; // 4 different background Screens
    [SerializeField] private float waitTime = 3f; // Time to wait between Background Screen change

    private void Start()
    {
        StartCoroutine(ChangeBackground());
    }

    private IEnumerator ChangeBackground()
    {
        for (int i = 0; i < stageBackgrounds.Length; i++)
        {
            stageBackgrounds[i].SetActive(true);
            yield return new WaitForSecondsRealtime(waitTime);

            if (i != stageBackgrounds.Length - 1)
            {
                stageBackgrounds[i + 1].SetActive(true);
            }
            else
            {
                stageBackgrounds[0].SetActive(true);
            }

            yield return new WaitForSeconds(1.0f); // for launching the particle system
            stageBackgrounds[i].SetActive(false);
        }
        StartCoroutine(ChangeBackground());
    }
}
