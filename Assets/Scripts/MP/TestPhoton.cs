using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TestPhoton : MonoBehaviourPunCallbacks, IPunObservable // this all works
{
    public bool tester = false;
    public string stringTester = "default";

    public Text testText;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(tester);
            stream.SendNext(stringTester);

        }
        else
        {
            // Network player, receive data
            this.tester = (bool)stream.ReceiveNext();
            this.stringTester = (string)stream.ReceiveNext();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        if (Input.GetKeyDown(KeyCode.X))
        {
                tester = !tester;
                //stringTester = "changedDefault";
                GetComponent<SetPlayerCosmetics>().SetClothesValues();
                //stringTester = GetComponent<SetPlayerCosmetics>().GetJson();
                //testText.text = GetComponent<SetPlayerCosmetics>().GetJson();
            }
    }
}