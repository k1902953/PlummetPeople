using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MP_Lobby : MonoBehaviourPun, IPunObservable
{
    public static MP_Lobby instance;

    [SerializeField]
    private int playersToStartCountdown;

    [SerializeField]
    private Text playerCountText, timerText, hostStartText, hostAlert;

    public float timer;
    public bool disableMovement = false;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // info.Sender : is the player
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            // We own this player: send the others our data
            stream.SendNext(timer);
            stream.SendNext(disableMovement);
        }
        else
        {
            // Network player, receive data
            this.timer = (float)stream.ReceiveNext();
            this.disableMovement = (bool)stream.ReceiveNext();
        }
    }


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(CountDown());
        UpdateUi();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                timer += 5;
            }
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                timer -= 5;
                timer = Mathf.Clamp(timer, 0, Mathf.Infinity);
            }

            if (hostStartText.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    StartCoroutine(StartGame());
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (timerText.gameObject.activeSelf)
            timerText.text = Mathf.Round(timer).ToString();

        /*if (timerText.gameObject.activeSelf)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                timer -= Time.deltaTime;
            }            
            timerText.text = Mathf.Round(timer).ToString();
        }*/
    }

    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(1);

        UpdateUi();

        if (timerText.gameObject.activeSelf)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (timer <= 0)
                {
                    StartCoroutine(StartGame());
                }

                timer -= 1;
                timer = Mathf.Clamp(timer, 0, Mathf.Infinity);
            }

            timerText.text = Mathf.Round(timer).ToString();
        }

        StartCoroutine(CountDown());
    }

    IEnumerator StartGame()
    {
        disableMovement = true;
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.LoadLevel("MP_Wipeout_CottonCandySpinning");
    }

    public void UpdateUi()
    {
        playerCountText.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{playersToStartCountdown}";

        if (PhotonNetwork.CurrentRoom.PlayerCount >= playersToStartCountdown)
        {
            timerText.gameObject.SetActive(true);
        }
        else
        {
            timerText.gameObject.SetActive(false);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            hostAlert.gameObject.SetActive(true);

            if (PhotonNetwork.CurrentRoom.PlayerCount > 1 || PhotonNetwork.CurrentRoom.PlayerCount == playersToStartCountdown)
            {
                hostStartText.gameObject.SetActive(true);
            }
            else
            {
                hostStartText.gameObject.SetActive(false);
            }
        }
        else
        {
            hostAlert.gameObject.SetActive(false);
            hostStartText.gameObject.SetActive(false);
        }
    }
}