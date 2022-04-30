// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player instance
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;


#pragma warning disable 649

/// <summary>
/// Player manager.
/// Handles fire Input and Beams.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Public Fields

    [Tooltip("The current Health of our player")]
    public float Health = 1f;

    public bool eliminated = false;
    public bool isSpectate = false;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    public static GameObject localCameraInstance;

    public Text playerNameTagText;

    #endregion

    #region Private Fields


    #endregion

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    public void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }

        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    public void Start()
    {
        playerNameTagText.text = photonView.Owner.NickName;

        #if UNITY_5_4_OR_NEWER
        // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        #endif
    }


    public override void OnDisable()
    {
        // Always call the base to remove callbacks
        base.OnDisable();

#if UNITY_5_4_OR_NEWER
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity on every frame.
    /// Process Inputs if local player.
    /// Show and hide the beams
    /// Watch for end of game, when local player health is 0.
    /// </summary>
    public void Update()
    {
        // we only process Inputs and check health if we are the local player
        if (photonView.IsMine)
        {
            if (this.Health <= 0f)
            {
                GameManager.instance.LeaveRoom();
            }
        }
    }

#if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
#endif


    /// <summary>
    /// MonoBehaviour method called after a new level of index 'level' was loaded.
    /// We recreate the Player UI because it was destroy when we switched level.
    /// Also reposition the player if outside the current arena.
    /// </summary>
    /// <param name="level">Level index loaded</param>
    void CalledOnLevelWasLoaded(int level)
    {
        // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
        if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        {
            transform.position = new Vector3(0f, 5f, 0f);
        }
    }

    #endregion

    #region Private Methods

    private void OnLevelWasLoaded(int level)
    {
        /*if (WipeoutManager.instance)
        {
            print($"<color=green>found WipeoutManager.instance</color>");
            if (WipeoutManager.instance.GameIsActive())
            {
                print($"<color=green>WipeoutManager.instance.GameIsActive()</color>");
                eliminated = true;
                ForceSpectate();
                print($"<color=green>{photonView.Owner.NickName} : game is active, force spectate</color>");
                //GetComponent<CharacterController>().enabled = false;
                //GetComponent<CharacterController>().enabled = true;
            }
            else
            {
                print($"<color=green>!WipeoutManager.instance.GameIsActive()</color>");
            }
        }
        else
        {
            print($"<color=green>WipeoutManager.instance not found!</color>");
        }*/
    }


#if UNITY_5_4_OR_NEWER
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.CalledOnLevelWasLoaded(scene.buildIndex);

        eliminated = false;
        GetComponent<PlayerMovement>().puppetMaster.state = RootMotion.Dynamics.PuppetMaster.State.Alive;

        

        if (photonView.IsMine)
        {
            //transform.position = FindObjectOfType<GameManager>().GetRandomSpawnPoint();

            FindObjectOfType<GameManager>().SetRandomSpawnPos(transform, false);
            FindObjectOfType<PlayerCamera>().player = transform;

            

            /*if (PhotonNetwork.IsMasterClient)
            {
                if (scene.name == "MP_Lobby")
                {
                    PhotonNetwork.CurrentRoom.IsOpen = true;
                }
                else
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                }
            }*/

        }
    }
#endif

    public void ForceSpectate()
    {
        foreach (CharacterController c in FindObjectsOfType<CharacterController>())
        {
            if (!c.GetComponent<PlayerManager>().eliminated)
            {
                if (GetComponent<PlayerMovement>().cam)
                    GetComponent<PlayerMovement>().cam.GetComponent<PlayerCamera>().player = c.transform;
                break;
            }
        }
    }

    #endregion

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.Health);
            stream.SendNext(this.eliminated);
            stream.SendNext(this.isSpectate);
        }
        else
        {
            // Network player, receive data
            this.Health = (float)stream.ReceiveNext();
            this.eliminated = (bool)stream.ReceiveNext();
            this.isSpectate = (bool)stream.ReceiveNext();

            if (isSpectate)
                transform.GetChild(0).gameObject.SetActive(false);
            else
                transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    #endregion
}