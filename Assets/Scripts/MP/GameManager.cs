using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{

    public static GameManager instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    public GameObject cameraPrefab;


    [SerializeField]
    private List<Vector3> spawnPoints = new List<Vector3>();



    #region Private Methods

    private void OnDrawGizmosSelected()
    {
        foreach (Vector3 points in spawnPoints)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(points, .5f);
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log($"<color=red>CHANGED TO SCENE [0]</color> - PhotonNetwork.IsConnected: {PhotonNetwork.IsConnected}");
            SceneManager.LoadScene(0);
        }

        //if (!photonView.IsMine) return;

        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene().name);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate

                GameObject selfPlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, GetRandomSpawnPoint(), Quaternion.identity, 0);

                if (PlayerManager.localCameraInstance == null)
                {
                    //if (FindObjectOfType<Camera>()) Destroy(FindObjectOfType<Camera>().gameObject);
                    //GameObject selfCamera = Instantiate(cameraPrefab);
                    //print($"<color=red>{selfPlayer.name}</color>");
                    //selfCamera.GetComponent<PlayerCamera>().player = selfPlayer.transform;
                    //selfPlayer.GetComponent<PlayerMovement>().cam = selfCamera.transform;
                    //PlayerManager.localCameraInstance = selfCamera;
                }

            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        // destroy cameras that arn't MainCamera

        foreach (Camera c in FindObjectsOfType<Camera>())
        {
            if (c.tag != "MainCamera")
            {
                Destroy(c.gameObject);
            }
        }

        /*
        if (PlayerManager.localCameraInstance != null)
        {
            foreach (Camera c in FindObjectsOfType<Camera>())
            {
                if (c.tag != "MainCamera")
                {
                    Destroy(c.gameObject);
                }
            }
        }*/
    }

    public Vector3 GetRandomSpawnPoint()
    {
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
    }

    public void SetRandomSpawnPos(Transform _target, bool _withDelay)
    {
        if (_withDelay)
        {
            StartCoroutine(SetRandomSpawnPosWithDelay(_target));
        }
        else
        {
            _target.GetComponent<CharacterController>().enabled = false;
            _target.transform.position = GetRandomSpawnPoint();
            _target.GetComponent<CharacterController>().enabled = true;
        }
       
    }

    private IEnumerator SetRandomSpawnPosWithDelay(Transform _target)
    {
        yield return new WaitForSeconds(0.25f);
        _target.GetComponent<CharacterController>().enabled = false;
        _target.transform.position = GetRandomSpawnPoint();
        _target.GetComponent<CharacterController>().enabled = true;
    }


    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("MP_Lobby"); // MP_Wipeout_CottonCandySpinning // MP_Lobby
                                             //PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
    }


    #endregion

    #region Photon Callbacks


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }


    #endregion


    #region Public Methods


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }


    #endregion

    #region Photon Callbacks


    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            //LoadArena(); // this is why map resets when someone joins (or not)
        }

        MP_Lobby.instance.UpdateUi();
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            //LoadArena();
        }

        MP_Lobby.instance.UpdateUi();
    }


    #endregion
}

