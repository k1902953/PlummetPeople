using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNameTag : MonoBehaviourPun
{
    private void Start()
    {
        if (photonView.IsMine) gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }
}
