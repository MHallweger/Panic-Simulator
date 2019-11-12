using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAfterFalling : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject firePrefab;
    private GameObject actionsGameObject;

    private void Awake()
    {
        actionsGameObject = GameObject.Find("Actions");
    }

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

        fireOne.transform.SetParent(actionsGameObject.transform);
        fireTwo.transform.SetParent(actionsGameObject.transform);
        fireThree.transform.SetParent(actionsGameObject.transform);

        Destroy(explosionOne, 3f);
        Destroy(explosionTwo, 3f);
        Destroy(explosionThree, 3f);

        Destroy(fireOne, 300f);
        Destroy(fireTwo, 300f);
        Destroy(fireThree, 300f);
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

        fireOne.transform.SetParent(actionsGameObject.transform);
        fireTwo.transform.SetParent(actionsGameObject.transform);
        fireThree.transform.SetParent(actionsGameObject.transform);

        Destroy(explosionOne, 3f);
        Destroy(explosionTwo, 3f);
        Destroy(explosionThree, 3f);

        Destroy(fireOne, 300f);
        Destroy(fireTwo, 300f);
        Destroy(fireThree, 300f);
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

        fireOne.transform.SetParent(actionsGameObject.transform);
        fireTwo.transform.SetParent(actionsGameObject.transform);
        fireThree.transform.SetParent(actionsGameObject.transform);

        Destroy(explosionOne, 3f);
        Destroy(explosionTwo, 3f);
        Destroy(explosionThree, 3f);

        Destroy(fireOne, 300f);
        Destroy(fireTwo, 300f);
        Destroy(fireThree, 300f);
    }

    private IEnumerator EnableTrussHasFallenBool()
    {
        yield return new WaitForSeconds(.2f);
        Actions.instance.trussHasFallen = true;
        Actions.instance.actionPlaced = true;
    }
}
