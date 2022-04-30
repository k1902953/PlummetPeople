using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.Dynamics;

public class ResetPuppetHips : MonoBehaviour
{
    public float distanceToTrigger;
    PuppetMaster puppetMaster;
    private float start_pinWeight;
    private bool cooldown = false;

    public List<Transform> transformsToReset = new List<Transform>();

    private void Awake()
    {
        puppetMaster = transform.parent.GetComponent<PuppetMaster>();
        start_pinWeight = puppetMaster.pinWeight;
    }

    private void FixedUpdate()
    {
        if (Vector3.Magnitude(transform.localPosition) > distanceToTrigger && !cooldown && puppetMaster.state != PuppetMaster.State.Dead)
        {
            StartCoroutine(StartReset());
        }
    }

    IEnumerator StartReset()
    {
        //Debug.Log($"triggered reset at hips pos magnitute: {Vector3.Magnitude(transform.localPosition)}");
        cooldown = true;
        //GetComponent<Rigidbody>().isKinematic = true;
        puppetMaster.pinWeight = 1;
        //transform.localPosition = Vector3.zero;
        ResetAllFromList();
        //GetComponent<Rigidbody>().isKinematic = false;
        yield return new WaitForSeconds(2);
        cooldown = false;
        puppetMaster.pinWeight = start_pinWeight;
    }

    private void ResetAllFromList()
    {
        foreach (Transform t in transformsToReset)
        {

            t.GetComponent<Rigidbody>().isKinematic = true;
            t.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //t.GetComponent<Rigidbody>().position = Vector3.zero;
            t.localPosition = Vector3.zero;
            t.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
}
