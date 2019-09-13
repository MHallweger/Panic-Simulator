using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAfterFalling : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;

    private IEnumerator ExplodeAfterFalling()
    {
        GameObject explosionOne = Instantiate(explosionPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z + 6), transform.rotation);
        yield return new WaitForSeconds(0.1f);
        GameObject explosionTwo = Instantiate(explosionPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z + 12), transform.rotation);
        yield return new WaitForSeconds(0.1f);
        GameObject explosionThree = Instantiate(explosionPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z + 18), transform.rotation);
        Destroy(explosionOne, 3f);
        Destroy(explosionTwo, 3f);
        Destroy(explosionOne, 3f);
    }
}
