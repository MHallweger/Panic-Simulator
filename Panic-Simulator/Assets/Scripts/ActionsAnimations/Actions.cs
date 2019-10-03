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
    public bool actionEnabled; // Bool for checking if an action is selected (needed in InputSystem, DOTS) // TODO seperate between different states
    private float fireBurnTime = 30f; // Float that controls the amount of time the fire burns
    public static Actions instance; // Instance Variable for access

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
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine("Fire");
            }
        }
    }

    private IEnumerator Fire()
    {
        List<GameObject> fireList = new List<GameObject>();
        float increaser = 0.0f;

        var mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null)
            {
                GameObject fire = Instantiate(firePrefab);

                Vector3 newFirePosition = new Vector3(
                    hit.point.x,
                    0.0f,
                    hit.point.z);
                fire.transform.position = newFirePosition;

                yield return new WaitForSeconds(1f);

                for (int i = 0; i < Random.Range(10, 20); i++)
                {
                    GameObject randomGeneratedFire = Instantiate(firePrefab);

                    Vector3 newRandomFirePosition = new Vector3(
                        fire.transform.position.x + Random.Range(-5f + increaser, 5f + increaser),
                        0.0f,
                        fire.transform.position.z + Random.Range(-5f + increaser, 5f + increaser));

                    randomGeneratedFire.transform.position = newRandomFirePosition;

                    fireList.Add(randomGeneratedFire);

                    fire = fireList[Random.Range(0, fireList.Count)];
                    increaser += .5f;

                    yield return new WaitForSeconds(1f);
                }
            }
        }
    }
}
