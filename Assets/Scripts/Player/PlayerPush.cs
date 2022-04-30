using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using RootMotion.Dynamics;

public class PlayerPush : MonoBehaviourPun, IPunObservable
{
    public PlayerAnimatorManager playerAnimatorManager;
    private PhotonView pv;

    public PuppetMaster puppetMaster;

    private float starting_pinWeight;
    private float starting_muscleWeight;

    public bool isPushedOver = false;

    public float pushProtectionCooldown;

    public float pushForce;
    public float pushRayStartHeight;
    public float pushRayDistance;
    public float pushedStayDownTime;
    public LayerMask pushRayCanHitMask;

    public float pushInputCooldown;
    private bool canInputPush = true;
    private bool doPushRaycast = false;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        starting_pinWeight = puppetMaster.pinWeight;
        starting_muscleWeight = puppetMaster.muscleWeight;
    }

    private void Update()
    {
        if (pv.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        GetPushInput();
    }

    private void FixedUpdate()
    {
        if (doPushRaycast)
        {
            RaycastHit hit;
            if (Physics.SphereCast(transform.position + transform.up * 1, 0.75f, transform.forward, out hit, pushRayDistance, pushRayCanHitMask))
            {
                PhotonView hitPlayerPhotonView = hit.transform.GetComponent<PhotonView>();
                doPushRaycast = false;
                pv.RPC("RPC_PushedPlayer", RpcTarget.All, hitPlayerPhotonView.ViewID, transform.position.x, transform.position.z);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // info.Sender : is the player
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(isPushedOver);
        }
        else
        {
            // Network player, receive data
            this.isPushedOver = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    void RPC_PushedPlayer(int playerId, float x, float z)
    {
        PhotonView pushedPlayerView = PhotonView.Find(playerId);

        if (pushedPlayerView && !pushedPlayerView.gameObject.GetComponent<PlayerPush>().isPushedOver)
        {
            pushedPlayerView.gameObject.GetComponent<PlayerMovement>().GetPushed(new Vector3(x, 0, z), pushForce);
            StartCoroutine(pushedPlayerView.gameObject.GetComponent<PlayerPush>().GetPushed());

            AudioSource audio = Audio.instance.MakeSound(pushedPlayerView.transform.position);
            audio.clip = Audio.instance.clips_pushed[Random.Range(0, Audio.instance.clips_pushed.Count)];
            audio.volume = 0.2f;
            audio.spatialBlend = 1f;
            audio.Play();
            audio.GetComponent<Sound>().StartDelete(audio.clip.length);

            AudioSource audio2 = Audio.instance.MakeSound(pushedPlayerView.transform.position);
            audio2.clip = Audio.instance.clips_vocal_pushHit[Random.Range(0, Audio.instance.clips_vocal_pushHit.Count)];
            audio2.volume = 0.2f;
            audio2.spatialBlend = 1f;
            audio2.Play();
            audio2.GetComponent<Sound>().StartDelete(audio.clip.length);
        }
    }

    public IEnumerator GetPushed()
    {
        if (!isPushedOver)
        {
            isPushedOver = true;
            playerAnimatorManager.SetAnimBool("ragdoll", isPushedOver);

            puppetMaster.pinWeight = 0.0f;
            puppetMaster.muscleWeight = 0.1f;

            PlayerMovement playerMovement = GetComponent<PlayerMovement>();


            playerMovement.speed = 0.1f;
            playerMovement.rotationDisabled = true;

            yield return new WaitForSeconds(pushedStayDownTime);

            puppetMaster.pinWeight = starting_pinWeight;
            puppetMaster.muscleWeight = starting_muscleWeight;

            playerMovement.speed = playerMovement.start_speed;
            playerMovement.rotationDisabled = false;

            playerAnimatorManager.SetAnimBool("ragdoll", false);

            yield return new WaitForSeconds(pushProtectionCooldown);

            isPushedOver = false;
        }
    }

    public void SetDoPushRaycast(bool _set)
    {
        doPushRaycast = _set;
    }

    private void GetPushInput()
    {
        if (Input.GetButtonDown("Push") && canInputPush && !isPushedOver)
        {
            StartCoroutine(PushCooldown());
            StartCoroutine(playerAnimatorManager.SetAnimTrigger("push"));
        }
    }

    IEnumerator PushCooldown()
    {
        canInputPush = false;
        yield return new WaitForSeconds(pushInputCooldown);
        canInputPush = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + transform.up * pushRayStartHeight, transform.forward * pushRayDistance);
        Gizmos.DrawWireSphere(transform.position + transform.up * pushRayStartHeight + (transform.forward * pushRayDistance), 0.5f);
    }
}
