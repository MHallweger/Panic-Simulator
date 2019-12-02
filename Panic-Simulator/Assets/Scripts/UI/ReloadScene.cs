using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script that allows the user to restart the whole game main scene.
/// </summary>
public class ReloadScene : MonoBehaviour
{
    /// <summary>
    /// Method that reloads the actual main scene.
    /// </summary>
    public void ReloadActualScene()
    {
        DestroyAllEntities();
        StartCoroutine("DisposeAllWorlds");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// A Coroutine that waits for the end of the frame to prevent that new default systems are created when they aren't needed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DisposeAllWorlds()
    {
        yield return new WaitForEndOfFrame();
        Unity.Entities.World.DisposeAllWorlds();
        Unity.Entities.DefaultWorldInitialization.Initialize("Default World", false);
    }

    /// <summary>
    /// A method that destroys all entities in this world.
    /// </summary>
    private void DestroyAllEntities()
    {
        Unity.Entities.EntityManager entityManager = Unity.Entities.World.Active.EntityManager;
        entityManager.DestroyEntity(entityManager.UniversalQuery);
    }
}
