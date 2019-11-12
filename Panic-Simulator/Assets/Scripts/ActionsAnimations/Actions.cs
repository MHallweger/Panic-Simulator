using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    [SerializeField] private GameObject smallExplosionParticleEffect; // Small Explosion Prefab for the explosion effect
    [SerializeField] private GameObject mediumExplosionParticleEffect; // Medium Explosion Prefab for the explosion effect
    [SerializeField] private GameObject bigExplosionParticleEffect; // Big Explosion Prefab for the explosion effect
    [SerializeField] private GameObject pinPrefab; // The spawned pin for custom exits
    [SerializeField] private GameObject soundSystemPrefab; // The instantiated sound System
    [SerializeField] private GameObject firePrefab; // Fire effect for the fire action
    public bool smallGroundExplosion = false; // Bool for activating the small ground Explosion action/effect
    public bool mediumGroundExplosion = false; // Bool for activating the medium ground Explosion action/effect
    public bool bigGroundExplosion = false; // Bool for activating the big ground Explosion action/effect
    public bool fallingTruss = false; // Bool for activating the fallingTruss action/effect
    public bool trussHasFallen = false; // Bool for checking if a truss fallen and touched the ground. Helps for enabling panic mode on agents
    public bool createExits = false; // Bool for activating the convertBarriers action/effect
    public bool dropSoundSystem = false; // Bool for creating a sound System object on the mouse position
    public bool fire = false; // Bool for activating the fire action/effect
    public bool actionEnabled = false; // Bool for checking if an action is selected (needed in InputSystem, DOTS) // TODO seperate between different states
    public bool actionPlaced = false; // Bool for checking if an action has been placed on the ground
    private float fireBurnTime = 30f; // Float that controls the amount of time the fire burns
    public static Actions instance; // Instance Variable for access
    private GameObject actionsGameObject;


    //[SerializeField] private Animator animator;

    private void Awake()
    {
        instance = this;
        actionsGameObject = GameObject.Find("Actions");
    }

    void Update()
    {
        CheckActionBools();
    }

    private void CheckActionBools()
    {
        if (smallGroundExplosion)
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
                GameObject explosion = Instantiate(smallExplosionParticleEffect, mousePosition, Quaternion.identity);
                actionPlaced = true;
                Destroy(explosion, 3f);
            }
        }
        else if (mediumGroundExplosion)
        {
            // Medium Explosions in Radial menu was selected
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
                GameObject explosion = Instantiate(mediumExplosionParticleEffect, mousePosition, Quaternion.identity);
                actionPlaced = true;
                Destroy(explosion, 3f);
            }
        }
        else if (bigGroundExplosion)
        {
            // Medium Explosions in Radial menu was selected
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
                GameObject explosion = Instantiate(bigExplosionParticleEffect, mousePosition, Quaternion.identity);
                actionPlaced = true;
                Destroy(explosion, 3f);
            }
        }
        else if (createExits)
        {
            // Exit creater in Radial menu was selected
            if (Input.GetMouseButtonDown(0))
            {
                var mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider != null && hit.collider.gameObject.name != "ColliderGround" && hit.collider.gameObject.tag != "Truss")
                    {
                        GameObject pinGo = Instantiate(pinPrefab, hit.collider.gameObject.transform.position, Quaternion.identity);
                        pinGo.transform.SetParent(hit.transform.parent);
                        Vector3 newPinPosition = new Vector3(
                            hit.collider.gameObject.transform.position.x,
                            0.0f,
                            hit.collider.gameObject.transform.position.z);
                        pinGo.transform.position = newPinPosition;

                        // Disable barrier GameObject
                        // <moved to Input System>

                        // Increase Exits Amount
                        UIHandler.instance.InCreaseExitsAmount();
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
                        //UIHandler.instance.userCreatedSoundSystems.Add(soundSystemObject);
                        UIHandler.instance.userCreatedSoundSystems.Add(soundSystemObject);
                        Vector3 newSoundSystemPosition = new Vector3(
                            hit.point.x,
                            0.5f,
                            hit.point.z);
                        soundSystemObject.transform.position = newSoundSystemPosition;
                        soundSystemPrefab.transform.rotation = Quaternion.identity;

                        if (!UIHandler.instance.effectsEnabled)
                        {
                            soundSystemObject.transform.Find("Lights").gameObject.SetActive(false);
                        }
                        else
                        {
                            soundSystemObject.transform.Find("Lights").gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
        else if (fire)
        {
            // Falling Truss in Radial menu was selected
            if (Input.GetMouseButtonDown(0))
            {
                var mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    string hittedGameObjectName = hit.collider.gameObject.name;
                    if (hittedGameObjectName == "Sound System"
                        || hittedGameObjectName == "Sound System_2"
                        || hittedGameObjectName == "Sound System_3"
                        || hittedGameObjectName == "Sound System_4"
                        || hittedGameObjectName == "Sound System_5"
                        || hittedGameObjectName == "Sound System(Clone)")
                    {
                        Fire(hit.collider.gameObject.transform.position);
                        actionPlaced = true;
                    }
                }
            }
        }
    }

    private void Fire(Vector3 SoundSystemPosition)
    {
        GameObject fire = Instantiate(firePrefab);
        fire.transform.SetParent(actionsGameObject.transform); // Set the "Actions" GameObject as parent, to have the option to delete all childs of this parent when deleting all agents later with key 2

        Vector3 newFirePosition = new Vector3(
            SoundSystemPosition.x,
            0.0f,
            SoundSystemPosition.z);
        fire.transform.position = newFirePosition;
    }
}
