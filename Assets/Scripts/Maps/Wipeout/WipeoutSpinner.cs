using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WipeoutSpinner : MonoBehaviourPun
{
    public Transform spinnerTop, spinnerBottom;

    public float spinnerTopSpeed = 1, spinnerBottomSpeed = 1;
    [SerializeField]
    private float spinnerTopSpeedWeight = 0.1f, spinnerBottomSpeedWeight = 0.2f;

    public GameObject shockParticlePrefab;

    private void Start()
    {
        //PhotonNetwork.SendRate = 240;
        PhotonNetwork.SerializationRate = 40;
    }

    void FixedUpdate()
    {
        spinnerTopSpeed += spinnerTopSpeedWeight * Time.deltaTime;
        spinnerBottomSpeed += spinnerBottomSpeedWeight * Time.deltaTime;

        spinnerTopSpeed = Mathf.Clamp(spinnerTopSpeed, -175, 175);
        spinnerBottomSpeed = Mathf.Clamp(spinnerBottomSpeed, -200, 200);
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return; // remove this if cant get syncing working
        spinnerTop.transform.Rotate(new Vector3(0, 0, spinnerTopSpeed * Time.deltaTime));
        spinnerBottom.transform.Rotate(new Vector3(0, 0, spinnerBottomSpeed * Time.deltaTime));
    }
}
