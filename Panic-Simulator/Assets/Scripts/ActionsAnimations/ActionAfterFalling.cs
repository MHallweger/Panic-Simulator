using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAfterFalling : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject firePrefab;
    private float timeThatFireBurns = 30f;

    private IEnumerator ExplodeAfterFallingForeground()
    {
        GameObject explosionOne = Instantiate(explosionPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z + 6f), transform.rotation);
        yield return new WaitForSeconds(0.1f);
        GameObject explosionTwo = Instantiate(explosionPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z + 12f), transform.rotation);
        yield return new WaitForSeconds(0.1f);
        GameObject explosionThree = Instantiate(explosionPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z + 18f), transform.rotation);

        yield return new WaitForSeconds(1f);

        GameObject fireOne = Instantiate(firePrefab, new Vector3(transform.position.x, 0.5f, transform.position.z + 6f), transform.rotation);
        GameObject fireTwo = Instantiate(firePrefab, new Vector3(transform.position.x, 0.5f, transform.position.z + 12f), transform.rotation);
        GameObject fireThree = Instantiate(firePrefab, new Vector3(transform.position.x, 0.5f, transform.position.z + 18f), transform.rotation);

        Destroy(explosionOne, 3f);
        Destroy(explosionTwo, 3f);
        Destroy(explosionThree, 3f);

        Destroy(fireOne, timeThatFireBurns);
        Destroy(fireTwo, timeThatFireBurns);
        Destroy(fireThree, timeThatFireBurns);
    }

    private IEnumerator ExplodeAfterFallingLeftSide()
    {
        GameObject explosionOne = Instantiate(explosionPrefab, new Vector3(transform.position.x - 6f, 0.5f, transform.position.z), transform.rotation);
        yield return new WaitForSeconds(0.1f);
        GameObject explosionTwo = Instantiate(explosionPrefab, new Vector3(transform.position.x - 12f, 0.5f, transform.position.z), transform.rotation);
        yield return new WaitForSeconds(0.1f);
        GameObject explosionThree = Instantiate(explosionPrefab, new Vector3(transform.position.x - 18f, 0.5f, transform.position.z), transform.rotation);

        yield return new WaitForSeconds(1f);

        GameObject fireOne = Instantiate(firePrefab, new Vector3(transform.position.x - 6f, 0.5f, transform.position.z), transform.rotation);
        GameObject fireTwo = Instantiate(firePrefab, new Vector3(transform.position.x - 12f, 0.5f, transform.position.z), transform.rotation);
        GameObject fireThree = Instantiate(firePrefab, new Vector3(transform.position.x - 18f, 0.5f, transform.position.z), transform.rotation);

        Destroy(explosionOne, 3f);
        Destroy(explosionTwo, 3f);
        Destroy(explosionThree, 3f);

        Destroy(fireOne, timeThatFireBurns);
        Destroy(fireTwo, timeThatFireBurns);
        Destroy(fireThree, timeThatFireBurns);
    }

    private IEnumerator ExplodeAfterFallingRightSide()
    {
        GameObject explosionOne = Instantiate(explosionPrefab, new Vector3(transform.position.x + 6f, 0.5f, transform.position.z), transform.rotation);
        yield return new WaitForSeconds(0.1f);
        GameObject explosionTwo = Instantiate(explosionPrefab, new Vector3(transform.position.x + 12f, 0.5f, transform.position.z), transform.rotation);
        yield return new WaitForSeconds(0.1f);
        GameObject explosionThree = Instantiate(explosionPrefab, new Vector3(transform.position.x + 18f, 0.5f, transform.position.z), transform.rotation);

        yield return new WaitForSeconds(1f);

        GameObject fireOne = Instantiate(firePrefab, new Vector3(transform.position.x + 6f, 0.5f, transform.position.z), transform.rotation);
        GameObject fireTwo = Instantiate(firePrefab, new Vector3(transform.position.x + 12f, 0.5f, transform.position.z), transform.rotation);
        GameObject fireThree = Instantiate(firePrefab, new Vector3(transform.position.x + 18f, 0.5f, transform.position.z), transform.rotation);

        Destroy(explosionOne, 3f);
        Destroy(explosionTwo, 3f);
        Destroy(explosionThree, 3f);

        Destroy(fireOne, timeThatFireBurns);
        Destroy(fireTwo, timeThatFireBurns);
        Destroy(fireThree, timeThatFireBurns);
    }

    public IEnumerator EnableTrussHasFallenBool()
    {
        yield return new WaitForSeconds(2f);
        Actions.instance.trussHasFallen = true;
        Debug.Log("Bool angepasst!");
    }
}
