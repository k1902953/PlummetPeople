using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDelete : MonoBehaviour
{
    [SerializeField]
    private float timeToDelete;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(timeToDelete);
        Destroy(gameObject);
    }
}
