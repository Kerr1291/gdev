using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Component where an object can be spawn from.
/// </summary>
public class SpawnerPoint : MonoBehaviour
{
    //TODO: review this code, this could lead to issues if the coroutine is called for the same object
    //multiple times in a row
    public void Spawn(GameObject go, float waitTime)
    {
        StartCoroutine(SpawnEnumerator(go, waitTime));
    }

    //this may actually need to be done differently
    public IEnumerator SpawnEnumerator(GameObject go, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (go != null)
        {
            go.transform.position = transform.position;
            go.SetActive(true);
        }
    }
}
