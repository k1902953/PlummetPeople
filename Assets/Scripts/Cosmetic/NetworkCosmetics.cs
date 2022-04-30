using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkCosmetics : MonoBehaviourPunCallbacks, IPunObservable
{
    private SetPlayerCosmetics setPlayerCosmetics;

    private string cosmeticFromLocal;
    public string jsonCosmetic = "empty";

    public bool netTest = false;

    private void Awake()
    {
        setPlayerCosmetics = GetComponentInChildren<SetPlayerCosmetics>();
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                netTest = !netTest;
                this.jsonCosmetic = $"cosmeticFromLocal: ({cosmeticFromLocal}), jsonCosmetic: ({jsonCosmetic})";
            }
        }        
    }

    private void DoSendJsonNetwork()
    {
        jsonCosmetic = cosmeticFromLocal;
    }

    public void SendJsonOverNetwork(string _json)
    {
        cosmeticFromLocal = _json;
        //jsonCosmetic = _json;
        print($"{PhotonNetwork.NickName}: {_json}");
        //photonView.RPC("SyncJsonString", RpcTarget.All, _json);
        DoSendJsonNetwork();
    }

    [PunRPC]
    void SyncJsonString(string _json)
    {
        print($"{PhotonNetwork.NickName}: {_json}" );
        setPlayerCosmetics.publicCosmeticJson = _json;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(jsonCosmetic);
            stream.SendNext(netTest);
            print("TRYING TO SEND DATA");
        }
        else
        {
            // Network player, receive data
            this.jsonCosmetic = (string)stream.ReceiveNext();
            this.netTest = (bool)stream.ReceiveNext();
            print("RECIEVED DATA");
        }
    }
}
