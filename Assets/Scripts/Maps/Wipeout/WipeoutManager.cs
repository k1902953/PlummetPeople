using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class WipeoutManager : MonoBehaviourPun, IPunObservable
{
    public bool gameIsActive = false;
    public bool forceUpdater = false;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // info.Sender : is the player
        if (stream.IsWriting)
        {
            if (photonView.AmController)
            {
                // We own this player: send the others our data
                stream.SendNext(this.gameIsActive);
                stream.SendNext(this.forceUpdater);
            }            
        }
        else
        {
            // Network player, receive data
            this.gameIsActive = (bool)stream.ReceiveNext();
            this.forceUpdater = (bool)stream.ReceiveNext();
        }
    }

    IEnumerator ForceUpdateSwitch()
    {
        forceUpdater = !forceUpdater;
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(ForceUpdateSwitch());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gameIsActive = false;
            gameIsActive = true;
        }
    }

    private IEnumerator Start()
    {
        if (photonView.AmController)
        {
            gameIsActive = false;
            yield return new WaitForSeconds(1);
            gameIsActive = true;
            StartCoroutine(ForceUpdateSwitch());
        }        
    }

    public bool GameIsActive()
    {
        return gameIsActive;
    }
}
