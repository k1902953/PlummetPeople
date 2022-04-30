using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public void StartDelete(float timer)
    {
        StartCoroutine(DeleteTimer(timer));
    }

    IEnumerator DeleteTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        Destroy(gameObject);
    }
}
