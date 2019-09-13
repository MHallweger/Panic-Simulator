using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    [SerializeField] private GameObject explosionParticleEffect; // Explosion Prefab for the explosion effect
    [SerializeField] private GameObject pinPrefab; // The spawned pin for custom exits
    [SerializeField] private GameObject soundSystemPrefab; // The instantiated sound System
    public bool groundExplosion = false; // Bool for activating the ground Explosion action/effect
    public bool fallingTruss = false; // Bool for activating the fallingTruss action/effect
    public bool convertBarriers = false; // Bool for activating the convertBarriers action/effect
    public bool dropSoundSystem = false; // Bool for creating a sound System object on the mouse position
    public static Actions instance;

    //[SerializeField] private Animator animator;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        CheckActionBools();
    }

    private void CheckActionBools()
    {
        if (groundExplosion)
        {
            // Small Explosions in Radial menu was selected
            var mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    mousePosition = new Vector3(hit.point.x, 0.5f, hit.point.z);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                GameObject explosion = Instantiate(explosionParticleEffect, mousePosition, Quaternion.identity);
                Destroy(explosion, 3f);
            }
        }
        else if (convertBarriers)
        {
            // Exit creater in Radial menu was selected
            if (Input.GetMouseButtonDown(0))
            {
                var mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider != null)
                    {
                        GameObject pinGo = Instantiate(pinPrefab, hit.collider.gameObject.transform.position, Quaternion.identity);
                        Vector3 newPinPosition = new Vector3(
                            hit.collider.gameObject.transform.position.x,
                            0.0f,
                            hit.collider.gameObject.transform.position.z);
                        pinGo.transform.position = newPinPosition;

                        // Disable barrier GameObject
                        hit.collider.gameObject.transform.parent.gameObject.SetActive(false);
                    }
                }
            }
        }
        else if (fallingTruss)
        {
            // Falling Truss in Radial menu was selected
            if (Input.GetMouseButtonDown(0))
            {
                var mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject.name != "ColliderGround")
                    {
                        // Prevent from getting Animator Component from the ground
                        hit.collider.gameObject.GetComponent<Animator>().SetTrigger("isFalling");
                    }
                }
            }
        }
        else if (dropSoundSystem)
        {
            // Create Sound System object on mouse position
            if (Input.GetMouseButtonDown(0))
            {
                var mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider != null)
                    {
                        GameObject soundSystemObject = Instantiate(soundSystemPrefab);
                        Vector3 newSoundSystemPosition = new Vector3(
                            hit.point.x,
                            0.5f,
                            hit.point.z);
                        soundSystemObject.transform.position = newSoundSystemPosition;
                        soundSystemPrefab.transform.rotation = Quaternion.identity;
                    }
                }
            }
        }
    }
}
